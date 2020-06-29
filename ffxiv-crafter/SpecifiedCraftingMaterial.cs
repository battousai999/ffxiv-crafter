using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter
{
    public class SpecifiedCraftingMaterial
    {
        public ICraftingMaterial Material { get; set; }
        public string Name => Material?.Name;
        public int Count { get; set; }
    }
}
