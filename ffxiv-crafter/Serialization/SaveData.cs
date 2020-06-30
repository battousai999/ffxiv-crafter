using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter.Serialization
{
    public class SaveData
    {
        public List<CraftingItemData> AllCraftingItems { get; set; }
        public List<CraftingMaterialData> AllCraftingMaterials { get; set; }

        public List<CountedCraftingItemData> ListedItems { get; set; }
    }
}
