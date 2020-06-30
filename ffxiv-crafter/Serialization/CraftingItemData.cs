using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter.Serialization
{
    public class CraftingItemData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SourceType SourceType { get; set; }
        public List<CountedCraftingItemData> Materials { get; set; }
    }
}
