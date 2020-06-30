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

        public IEnumerable<SpecifiedCraftingMaterial> GetAllMaterials()
        {
            var allMaterials = Materials.SelectMany(x => x.GetAllMaterials());
            var dedupedMaterials = allMaterials.GroupBy(x => x.Name, (key, materials) => new SpecifiedCraftingMaterial { Material = materials.First().Material, Count = materials.Sum(x => x.Count) });

            return dedupedMaterials.Select(x => new SpecifiedCraftingMaterial { Material = x.Material, Count = x.Count * Count });
        }

        public static IEnumerable<SpecifiedCraftingMaterial> GetMaterialsFor(IEnumerable<SpecifiedCraftingItem> items)
        {
            var allMaterials = items.SelectMany(x => x.GetAllMaterials());
            var dedupedMaterials = allMaterials.GroupBy(x => x.Name, (key, materials) => new SpecifiedCraftingMaterial { Material = materials.First().Material, Count = materials.Sum(x => x.Count) });

            return dedupedMaterials;
        }
    }
}
