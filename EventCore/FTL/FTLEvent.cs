using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AngleSharp.Dom;

namespace EventCore.FTL
{
    public class FTLEvent : ICanHaveTextRef
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

        public static FTLEvent ParseEvent(IElement element, ModFile modFile, List<FTLChoice> choices)
        {
            if (IsEventRef(element, out var name)) return new FTLEventRef(element, name, modFile);
            // if (element) return FTLEvent.Nothing;

            var xAttribute = element.GetAttribute("name");
            return new FTLEvent(element, xAttribute, choices, modFile);
        }

        public static FTLEvent NewEvent(IElement parentElement, ModFile modFile)
        {
            var newEventElement = parentElement.AppendNew("event");
            var ftlEvent = new FTLEvent(newEventElement, "NEW_EVENT", new List<FTLChoice>(), modFile);
            ftlEvent.Name = ftlEvent.Name;
            return ftlEvent;
        }


        public static bool IsEventRef(IElement element, [NotNullWhen(true)] out string? name)
        {
            var loadAttr = element.GetAttribute("load");
            if (loadAttr is not null)
            {
                name = loadAttr;
                return true;
            }

            var loadEventElement = element.Element("loadEvent");
            if (loadEventElement is not null)
            {
                name = loadEventElement.TextContent;
                return true;
            }

            name = null;
            return false;
        }

        protected FTLEvent(IElement xElement, ModFile modFile)
        {
            ModFile = modFile;
            Element = xElement;
            if (QuestMode == QuestModeEnum.Define)
            {
                QuestDefinition = new FTLQuestDefinition(Element.Element("quest") ??
                                                         throw new NullReferenceException("quest tag not found"));
            }

            Damages = Element.Children.Where(child => child.TagName == "damage").Select(child => new FTLDamage(child))
                .ToList();
        }

        public FTLEvent(IElement xElement, string? name, List<FTLChoice> choices, ModFile modFile) : this(xElement,
            modFile)
        {
            _name = name;
            Choices = choices;
        }

        public IElement Element { get; }

        public ModFile ModFile { get; }

        private string? _name;

        public virtual string? Name
        {
            get => _name;
            set
            {
                var oldName = _name;
                _name = value;
                if (string.IsNullOrEmpty(value))
                {
                    Element.RemoveAttribute("name");
                }
                else
                {
                    Element.SetAttribute("name", value);
                }

                ModFile.EventNameUpdated(oldName, value, this);
            }
        }

        public string? Text
        {
            get => ((ICanHaveTextRef)this).TextImp;
            set => ((ICanHaveTextRef)this).TextImp = value;
        }

        FTLTextRef? ICanHaveTextRef.TextRef { get; set; }


        public bool Unique
        {
            get => Element.GetAttribute("unique") == "true";
            set => Element.SetAttribute("unique", value);
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

        public string? WeaponReward
        {
            get => Element.Element("weapon")?.GetAttribute("name");
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Element.RemoveChildElement("weapon");
                    return;
                }

                var weaponElement = Element.Element("weapon") ?? Element.AppendNew("weapon");
                weaponElement.SetAttribute("name", value);
            }
        }

        public string? BoarderClass
        {
            get => Element.Element("boarders")?.GetAttribute("class");
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Element.RemoveChildElement("boarders");
                    return;
                }

                var boarderElement = Element.Element("boarders") ?? Element.AppendNew("boarders");
                boarderElement.SetAttribute("class", value);
            }
        }

        public int BoarderMin
        {
            get => int.Parse(Element.Element("boarders")?.GetAttribute("min") ?? "0");
            set => Element.Element("boarders")?.SetAttribute("min", value.ToString());
        }

        public int BoarderMax
        {
            get => int.Parse(Element.Element("boarders")?.GetAttribute("max") ?? "0");
            set => Element.Element("boarders")?.SetAttribute("max", value.ToString());
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
                    Element.RemoveChildElement("quest");
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

        public bool HasShip
        {
            get => Element.Element("ship") != null;
            set => Element.ToggleChildElement("ship", value);
        }

        public bool ShipHostile
        {
            get => Element.Element("ship")?.GetAttribute("hostile") == "true";
            set => Element.Element("ship")?.SetAttribute("hostile", value);
        }

        public string ShipLoad
        {
            get => Element.Element("ship")?.GetAttribute("load") ?? "";
            set => Element.Element("ship")?.SetAttributeRemoveIfBlank("load", value);
        }

        public List<FTLChoice> Choices { get; } = new();

        public FTLChoice AddNewChoice()
        {
            //todo format output xml better
            var choiceElement = Element.AppendNew("choice");
            choiceElement.AppendNew("text", "placeholder");
            var choiceEventElement = choiceElement.AppendNew("event");
            var ftlChoice = new FTLChoice(Choices.Count,
                ParseEvent(choiceEventElement, ModFile, new List<FTLChoice>()),
                choiceElement,
                ModFile);
            Choices.Add(ftlChoice);
            return ftlChoice;
        }

        public List<FTLDamage> Damages { get; }

        public FTLDamage AddNewDamage()
        {
            var damageElement = Element.AppendNew("damage");
            var ftlDamage = new FTLDamage(damageElement)
            {
                Amount = 0
            };
            return ftlDamage;
        }

        public virtual bool IsUnknownRef => false;

        public virtual bool IsRef => false;

        public void RemoveDamage(FTLDamage ftlDamage)
        {
            Element.RemoveChild(ftlDamage.Element);
            Damages.Remove(ftlDamage);
        }
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
            _refName = refName;
        }

        // public override List<FTLChoice> Choices => ActualEvent?.Choices ?? base.Choices;
        public override string? Name
        {
            get => base.Name ?? RefName;
            set
            {
                if (base.Name != null)
                {
                    base.Name = value;
                    return;
                }
                RefName = value ?? "";
            }
        }

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

        public void FindRef(IReadOnlyDictionary<string, FTLEvent> events)
        {
            FTLEvent? foundEvent;
            events.TryGetValue(_refName, out foundEvent);
            ActualEvent = foundEvent;
        }
    }

    public class FTLEventList : FTLEvent
    {
        public List<FTLEvent> FtlEvents { get; }

        public FTLEventList(IElement xElement, List<FTLEvent> ftlEvents, ModFile modFile) : base(xElement,
            xElement.GetAttribute("name"),
            new(),
            modFile)
        {
            FtlEvents = ftlEvents;
        }

        public FTLEvent AddNewEvent()
        {
            var newEvent = FTLEvent.NewEvent(Element, ModFile);
            FtlEvents.Add(newEvent);
            return newEvent;
        }
    }
}
