using System;
using System.Threading.Tasks;
using EventCore;
using Shouldly;
using Xunit;

namespace Tests
{
    public class BasicTests
    {
        private const string TestData = @"D:\Games\FTL Stuff\EventManager\Tests\TestData\data";

        private const string TestFile =
            @"D:\Games\FTL Stuff\EventManager\Tests\TestData\data\dlcEvents_anaerobic.xml.append";

        [Fact]
        public async Task CanLoadMod()
        {
            var modRoot = await new ModLoader(TestData).Load();
            modRoot.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanLoadFile()
        {
            var modFile = new ModFileLoader(TestFile);
            modFile.Load();
            modFile.Events.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task SpecificEvent()
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
    }
}
