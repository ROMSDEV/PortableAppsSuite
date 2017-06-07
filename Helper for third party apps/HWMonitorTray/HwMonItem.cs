namespace HwMonTray
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HwMonItem
    {
        public string Device;
        public string Type;
        public string ID;
        public string Value;
        public int Count;

        public HwMonItem(string Device, string Type, string ID, string Value)
        {
            this.Device = Device;
            this.Type = Type;
            this.ID = ID;
            this.Value = Value;
            Count = 0;
        }

        public HwMonItem(string Device, string Type, string ID, string Value, int Count)
        {
            this.Device = Device;
            this.Type = Type;
            this.ID = ID;
            this.Value = Value;
            this.Count = Count;
        }
    }
}
