using System;
using System.Collections.Generic;
using System.Text;

namespace ffxiv_crafter
{
    public interface ICraftingMaterial
    {
        string Name { get; set; }
        bool IsCraftable { get; }
        SourceType SourceType { get; set; }
        string Location { get; set; }
    }
}
