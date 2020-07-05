using System;
using System.Collections.Generic;
using System.Text;
using ffxiv_crafter.Models;

namespace ffxiv_crafter.Serialization
{
    public class CraftingMaterialData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SourceType SourceType { get; set; }
        public string Location { get; set; }
    }
}
