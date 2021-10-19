using AngleSharp.Dom;

namespace EventCore.FTL
{
    public class FTLQuestDefinition
    {
        private IElement _questElement;

        public FTLQuestDefinition(IElement questElement)
        {
            _questElement = questElement;
        }

        private void SetBool(string name, bool value)
        {
            _questElement.SetChildElementText(name, value ? "true" : "false", true);
        }

        private bool GetBool(string name, bool defaultValue = false)
        {
            var value = _questElement.Element(name)?.TextContent;
            if (value == null) return defaultValue;
            return value == "true";
        }

        public bool NonNebulaBeacon
        {
            get => GetBool("nonNebulaBeacon");
            set => SetBool("nonNebulaBeacon", value);
        }

        public bool NebulaBeacon
        {
            get => GetBool("nebulaBeacon");
            set => SetBool("nebulaBeacon", value);
        }

        public bool CreateNebula
        {
            get => GetBool("createNebula");
            set => SetBool("createNebula", value);
        }
        public bool CurrentSector
        {
            get => GetBool("currentSector");
            set => SetBool("currentSector", value);
        }
        public bool NextSector
        {
            get => GetBool("nextSector");
            set => SetBool("nextSector", value);
        }

        public int Aggressive
        {
            get => int.Parse(_questElement.Element("aggressive")?.TextContent ?? "");
            set => _questElement.SetChildElementText("aggressive", value.ToString(), true);
        }

        public bool SectorEight
        {
            get => GetBool("sectorEight");
            set => SetBool("sectorEight", value);
        }

        public bool LastStand
        {
            get => GetBool("lastStand");
            set => SetBool("lastStand", value);
        }
    }
}
