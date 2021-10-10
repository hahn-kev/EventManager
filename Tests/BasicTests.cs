using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Xml.Parser;
using EventCore;
using JetBrains.dotMemoryUnit;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class BasicTests
    {
        private const string TestData = @"D:\Games\FTL Stuff\EventManager\Tests\TestData\data";

        private const string TestFile =
            @"D:\Games\FTL Stuff\EventManager\Tests\TestData\data\dlcEvents_anaerobic.xml.append";

        private readonly ITestOutputHelper output;

        public BasicTests(ITestOutputHelper output)
        {
            this.output = output;
            DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
        }

        [Fact]
        public async Task CanLoadMod()
        {
            var modRoot = await new ModLoader(TestData).Load();
            modRoot.ShouldNotBeNull();
        }

        [Fact]
        public async Task MemoryUsage()
        {
            var xmlElementType = typeof(XmlParser).Assembly.GetType("AngleSharp.Xml.Dom.XmlElement");
            xmlElementType.ShouldNotBeNull();

            var modRoot = await new ModLoader(TestData).Load();

            dotMemory.Check(memory =>
            {
                memory.GetObjects(property => property.Type.Is(xmlElementType))
                    .ObjectsCount.ShouldBeLessThan(110_000);

                var memorySizeInMb = memory.SizeInBytes / 1_000_000f;
                memorySizeInMb.ShouldBe(111, 1);
            });
        }

        [Fact]
        public async Task Testing()
        {
            var modRoot = await new ModLoader(TestData).Load();

            var names = modRoot.TopLevelEvents
                .SelectMany(ftlEvent => Events(ftlEvent))
                .SelectMany(element => element.Children, (element, attr) => attr.TagName).Distinct();
            // .SelectMany(element => element.Attributes, (element, attr) => attr.Name).Distinct();
            output.WriteLine("[\"{0}\"]", string.Join("\", \"", names));
        }

        private IEnumerable<IElement> Choices(FTLEvent @event)
        {
            if (@event.Choices.Count == 0) yield break;
            var stack = new Stack<FTLChoice>(@event.Choices);
            while (stack.Count > 0)
            {
                var ftlChoice = stack.Pop();
                yield return ftlChoice.Element;
                if (ftlChoice.Event.IsRef) continue;
                foreach (var eventChoice in ftlChoice.Event.Choices)
                {
                    stack.Push(eventChoice);
                }
            }
        }
        private IEnumerable<IElement> Events(FTLEvent @event)
        {
            yield return @event.Element;
            if (@event.Choices.Count == 0) yield break;
            var stack = new Stack<FTLChoice>(@event.Choices);
            while (stack.Count > 0)
            {
                var ftlChoice = stack.Pop();
                yield return ftlChoice.Event.Element;
                if (ftlChoice.Event.IsRef) continue;
                foreach (var eventChoice in ftlChoice.Event.Choices)
                {
                    stack.Push(eventChoice);
                }
            }
        }


        [Fact]
        public async Task CanLoadFile()
        {
            var fileLoader = new ModFileLoader(TestFile);
            fileLoader.Load();
            fileLoader.Events.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task CanSaveFile()
        {
            var fileLoader = new ModFileLoader(TestFile);
            fileLoader.Load();
            var modFile = fileLoader.ModFile;
            var fileSaver = new ModSaver();
            var ms = new MemoryStream();

            await fileSaver.SaveFile(modFile, ms);
            var xml = Encoding.Default.GetString(ms.ToArray());
            xml.ShouldMatchApproved(builder => builder.DoNotIgnoreLineEndings().SubFolder("TestApprovals"));
        }

        [Fact]
        public void ChangingFtlEventMarksFileDirty()
        {
            var fileLoader = new ModFileLoader(TestFile);
            fileLoader.Load();
            var modFile = fileLoader.ModFile;
            var ftlEvent = modFile.Events.Values.First();
            ftlEvent.Name += "_1";

            modFile.Dirty.ShouldBeTrue();
        }

        [Fact]
        public void NotChangingFtlEventKeepsTheModClean()
        {
            var fileLoader = new ModFileLoader(TestFile);
            fileLoader.Load();
            var modFile = fileLoader.ModFile;
            var ftlEvent = modFile.Events.Values.First();

            modFile.Dirty.ShouldBeFalse();
        }

        [Fact]
        public void SpecificEvent()
        {
            var eventName = "LANIUS_LANGUAGE";

            var modFile = new ModFileLoader(TestFile);
            modFile.Load();

            modFile.Events.ShouldContainKey(eventName);
            var ftlEvent = modFile.Events[eventName];

            ftlEvent.Name.ShouldBe(eventName);
            ftlEvent.Choices.Count.ShouldBe(2);

            var agreeChoice = ftlEvent.Choices[0];
            agreeChoice.Hidden.ShouldBeTrue();
            agreeChoice.Text.ShouldStartWith("Agree, knowing it");
            agreeChoice.Event.Text.ShouldStartWith("As expected, the data");
            agreeChoice.Event.Choices.Count.ShouldBe(2);

            var payChoice = agreeChoice.Event.Choices[0];
            payChoice.Text.ShouldBe("Accept the pay.");
            payChoice.Event.HasReward.ShouldBe(true);
            payChoice.Event.RewardLevel.ShouldBe("MED");
            payChoice.Event.RewardType.ShouldBe("standard");

            var joinCrewChoice = agreeChoice.Event.Choices[1];
            joinCrewChoice.Text.ShouldStartWith("Suggest the Lanius join");
            joinCrewChoice.Event.Text.ShouldStartWith("The Lanius agrees");
            joinCrewChoice.Event.HasCrew.ShouldBeTrue();
            joinCrewChoice.Event.CrewAmount.ShouldBe(1);
            joinCrewChoice.Event.CrewClass.ShouldBe("anaerobic");


            var refuseChoice = ftlEvent.Choices[1];
            refuseChoice.Text.ShouldBe("Refuse.");
            refuseChoice.Event.Text.ShouldStartWith("\"Ok. Thanks many");

            var storageCheck = refuseChoice.Event.Choices[0];
            storageCheck.Event.Name.ShouldBe("STORAGE_CHECK");
        }

        [Fact]
        public async Task HyperspaceEventsPresent()
        {
            var modRoot = await new ModLoader(TestData).Load();
            modRoot.EventsLookup.ShouldContainKey("SHOWDOWN_WIN");
            // var ftlEvent = modRoot.EventsLookup["SHOWDOWN_WIN"];
        }
    }
}
