namespace HwMonTray
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class HwMonTable : List<HwMonItem>
    {
        public void AddCount(HwMonItem item)
        {
            item.Count = CountID(item);
            Add(item);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private int CountID(HwMonItem item)
        {
            var num = 0;
            foreach (var current in this)
                if (current.ID == item.ID)
                    num++;
            return num;
        }
    }
}
