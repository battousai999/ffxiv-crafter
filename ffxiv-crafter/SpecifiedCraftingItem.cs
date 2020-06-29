using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace ffxiv_crafter
{
    public class SpecifiedCraftingItem
    {
        public CraftingItem Item { get; set; }
        public string Name => Item?.Name;
        public SourceType SourceType => Item?.SourceType ?? SourceType.Alchemy;
        public List<SpecifiedCraftingMaterial> Materials => Item?.Materials ?? Enumerable.Empty<SpecifiedCraftingMaterial>().ToList();
        public int Count { get; set; }
    }
}
