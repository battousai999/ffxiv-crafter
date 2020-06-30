using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ffxiv_crafter
{
    public class CraftingItem : ICraftingMaterial
    {
        private SourceType sourceType = SourceType.Alchemy;

        public string Name { get; set; }
        public bool IsCraftable => true;

        public string Location
        {
            get => "";
            set { }
        }

        public SourceType SourceType
        {
            get => sourceType;
            set
            {
                if (value == SourceType.None || value == SourceType.Botany || value == SourceType.Mining)
                {
                    throw new InvalidOperationException("Crafted crafting materials can only be sourced from a crafting profession—use the CraftingMaterial class instead.");
                }

                sourceType = value;
            }
        }

        public List<SpecifiedCraftingMaterial> Materials { get; private set; } = new List<SpecifiedCraftingMaterial>();

        public void SetMaterials(IEnumerable<SpecifiedCraftingMaterial> materials)
        {
            Materials = materials.ToList();
        }

        internal void DeleteMaterials(List<CraftingMaterial> itemsToDelete)
        {
            Materials.RemoveAll(x => itemsToDelete.Any(y => StringComparer.OrdinalIgnoreCase.Equals(y.Name, x.Name)));
            Materials.OfType<CraftingItem>().ToList().ForEach(x => x.DeleteMaterials(itemsToDelete));
        }

        public IEnumerable<SpecifiedCraftingMaterial> GetAllMaterials()
        {
            var allMaterials = Materials.SelectMany(x => x.GetAllMaterials());
            var dedupedMaterials = allMaterials.GroupBy(x => x.Name, (key, materials) => new SpecifiedCraftingMaterial { Material = materials.First().Material, Count = materials.Sum(x => x.Count) });

            return dedupedMaterials;
        }
    }
}
