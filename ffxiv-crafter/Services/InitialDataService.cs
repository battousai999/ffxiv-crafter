using ffxiv_crafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ffxiv_crafter.Services
{
    public interface IInitialDataService
    {
        IEnumerable<CraftingItem> GetCraftingItems();
        IEnumerable<CraftingMaterial> GetCraftingMaterials();
    }

    public class InitialDataService : IInitialDataService
    {
        public IEnumerable<CraftingItem> GetCraftingItems()
        {
            return Enumerable.Empty<CraftingItem>();
        }

        public IEnumerable<CraftingMaterial> GetCraftingMaterials()
        {
            return Enumerable.Empty<CraftingMaterial>();
        }
    }
}
