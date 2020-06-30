using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter
{
    public class SpecifiedCraftingMaterial
    {
        public ICraftingMaterial Material { get; set; }
        public string Name => Material?.Name;
        public int Count { get; set; }

        public IEnumerable<SpecifiedCraftingMaterial> GetAllMaterials()
        {
            var craftingMaterial = Material as CraftingMaterial;

            if (craftingMaterial != null)
                return this.ToSingleton();

            var craftingItem = Material as CraftingItem;

            if (craftingItem == null)
                throw new InvalidOperationException("Unexpected type of material.");

            return craftingItem.GetAllMaterials().Select(x => new SpecifiedCraftingMaterial { Material = x.Material, Count = x.Count * Count });
        }
    }
}
