using System;
using System.Collections.Generic;
using System.Xml.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace EventCore
{
    public class FTLEvent
    {
        //possible valid attributes
        private static readonly string[] ValidAttributes = new[] { "name", "hidden", "unique" };

        private static readonly string[] ValidChildElements = new[]
        {
            "text", "quest", "ship", "environment", "choice", "weapon", "unlockCustomShip", "autoReward", "item_modify",
            "damage", "upgrade", "modifyPursuit", "crewMember", "img", "removeCrew", "status", "distressBeacon",
            "restartEvent", "drone", "beaconType", "recallBoarders", "loadEvent", "instantEscape", "customFleet",
            "preventQuest", "preventFleet", "augment", "store", "event", "triggeredEvent", "hiddenAug", "boarders",
            "remove", "reveal_map", "surrender", "aggressive", "removeHazards", "secretSectorWarp", "secretSector",
            "checkCargo", "transformRace", "changeBackground", "playSound", "jumpEvent", "clearTriggeredEvent",
            "enemyDamage", "lose", "fleet", "system", "noQuestText", "replaceSector", "superBarrage", "superDrones",
            "clearSuperDrones", "removeItem", "loadEventList", "superShields", "runFromFleet", "preventBossFleet",
            "resetFtl", "win", "disableScrapScore", "unlockShip"
        };


        protected FTLEvent(IElement xElement, ModFile modFile)
        {
            ModFile = modFile;
            Element = xElement;
            if (QuestMode == QuestModeEnum.Define)
            {
                QuestDefinition = new FTLQuestDefinition(Element.Element("quest") ?? throw new NullReferenceException("quest tag not found"));
            }
        }

        public FTLEvent(IElement xElement, string? name, List<FTLChoice> choices, ModFile modFile) : this(xElement,
            modFile)
        {
            _name = name;
            Choices = choices;
        }

        public IElement Element { get; init; }
        public ModFile ModFile { get; set; }
        private string? _name;

        public string? Name
        {
            get => _name;
            set
            {
                _name = value;
                if (value != null)
                    Element.SetAttribute("name", value);
                else
                    Element.RemoveAttribute("name");
            }
        }

        public string? Text
        {
            get => Element.Element("text")?.TextContent;
            set => Element.SetChildElementText("text", value ?? "", true, true);
        }

        public bool Unique
        {
            get => Element.GetAttribute("unique") == "true";
            set => Element.SetAttribute("unique", value ? "true" : "false");
        }

        public bool HasReward
        {
            get => Element.Element("autoReward") != null;
            set
            {
                if (value == HasReward) return;
                Element.ToggleChildElement("autoReward", value);
            }
        }

        public string? RewardLevel
        {
            get => Element.Element("autoReward")?.GetAttribute("level");
            set => Element.Element("autoReward")?.SetAttribute("level", value ?? "");
        }

        public string? RewardType
        {
            get => Element.Element("autoReward")?.TextContent;
            set => Element.SetChildElementText("autoReward", value ?? "");
        }

        public bool HasCrew
        {
            get => Element.Element("crewMember") != null;
            set => Element.ToggleChildElement("crewMember", value);
        }

        public int CrewAmount
        {
            get => int.Parse(Element.Element("crewMember")?.GetAttribute("amount") ?? "0");
            set => Element.Element("crewMember")?.SetAttribute("amount", value.ToString());
        }

        public string? CrewClass
        {
            get => Element.Element("crewMember")?.GetAttribute("class");
            set => Element.Element("crewMember")?.SetAttribute("class", value ?? "");
        }

        public string? CrewName
        {
            get => Element.Element("crewMember")?.TextContent;
            set => Element.SetChildElementText("crewMember", value ?? "");
        }

        public enum QuestModeEnum
        {
            None,
            Start,
            Define
        }

        public QuestModeEnum QuestMode
        {
            get
            {
                var questElement = Element.Element("quest");
                if (questElement == null) return QuestModeEnum.None;

                return questElement.HasAttribute("event") ? QuestModeEnum.Start : QuestModeEnum.Define;
            }
            set
            {
                var currentMode = QuestMode;
                if (currentMode == value) return;
                if (value == QuestModeEnum.None)
                {
                    Element.ToggleChildElement("quest", false);
                    return;
                }

                var questElement = Element.Element("quest") ?? Element.AppendNew("quest");
                if (value == QuestModeEnum.Start)
                {
                    questElement.SetAttribute("event", "");
                }
                else
                {
                    questElement.RemoveAttribute("event");
                    QuestDefinition = new FTLQuestDefinition(questElement);
                }
            }
        }

        public string? QuestEvent
        {
            get => Element.Element("quest")?.GetAttribute("event");
            set => Element.Element("quest")?.SetAttribute("event", value ?? "");
        }

        public FTLQuestDefinition? QuestDefinition { get; private set; }

        public List<FTLChoice> Choices { get; } = new();
        public virtual bool IsUnknownRef => false;
        public virtual bool IsRef => false;
    }

    public class FTLEventRef : FTLEvent
    {
        public FTLEvent? ActualEvent { get; private set; }
        private string _refName;

        public FTLEventRef(IElement xElement, string refName, ModFile modFile) : base(xElement,
            xElement.GetAttribute("name"),
            new(),
            modFile)
        {
            Element = xElement;
            _refName = refName;
        }

        // public override List<FTLChoice> Choices => ActualEvent?.Choices ?? base.Choices;

        public string RefName
        {
            get => _refName;
            set
            {
                _refName = value!;
                if (Element.HasAttribute("load"))
                {
                    Element.SetAttribute("load", _refName);
                }
                else
                {
                    Element.SetChildElementText("loadEvent", _refName, true);
                }

                FindRef(ModFile.ModRoot?.EventsLookup ?? ModFile.Events);
            }
        }

        public override bool IsUnknownRef => ActualEvent == null;

        public override bool IsRef => true;

        public void FindRef(Dictionary<string, FTLEvent> events)
        {
            FTLEvent? foundEvent;
            events.TryGetValue(_refName, out foundEvent);
            ActualEvent = foundEvent;
        }
    }
}
