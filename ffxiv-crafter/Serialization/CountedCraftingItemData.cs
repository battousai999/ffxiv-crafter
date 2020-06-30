using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter.Serialization
{
    public class CountedCraftingItemData
    {
        public Guid ItemId { get; set; }
        public int Count { get; set; }
        public bool IsMaterial { get; set; }
    }
}
