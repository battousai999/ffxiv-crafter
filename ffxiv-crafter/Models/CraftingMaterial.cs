using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter.Models
{
    public class CraftingMaterial : ICraftingMaterial
    {
        private SourceType sourceType;

        public string Name { get; set; }
        public string Location { get; set; }
        
        public SourceType SourceType 
        {
            get => sourceType;
            set
            {
                if (value != SourceType.None && value != SourceType.Botany && value != SourceType.Mining && value != SourceType.Fishing)
                {
                    throw new InvalidOperationException("Regular crafting materials cannot be sourced from a crafting profession—use the CraftingItem class instead.");
                }

                sourceType = value;
            }
        }

        public string SourceTypeName => SourceType == SourceType.None ? "Drop" : SourceType.ToString();
        public bool IsCraftable => false;

        public CraftingMaterial Clone()
        {
            return new CraftingMaterial
            {
                Name = this.Name,
                SourceType = this.SourceType,
                Location = this.Location
            };
        }
    }
}
