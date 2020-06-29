using System;
using System.Collections.Generic;
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

        public List<SpecifiedCraftingMaterial> Materials { get; } = new List<SpecifiedCraftingMaterial>();
    }
}
