using ffxiv_crafter.Models;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Windows;

namespace ffxiv_crafter.Services
{
    public interface IChildWindowProvider
    {
        (string ItemName, SourceType SourceType, IEnumerable<SpecifiedCraftingMaterial> Materials)? ShowAddEditCraftingItemWindow(
            Window owner,
            IEnumerable<CraftingMaterial> definedCraftingMaterials,
            IEnumerable<CraftingItem> definedCraftingItems,
            Action<CraftingMaterial> registerNewCraftingMaterial,
            Action<CraftingItem> registerNewCraftingItem,
            string suggestedItemName,
            CraftingItem editItem = null);

        IEnumerable<CraftingMaterial> ShowConfigureMaterialsWindow(Window owner, List<CraftingMaterial> materials);
        int? ShowEditCountWindow(Window owner, int count);
        void ShowGenerateListWindow(Window owner, List<SpecifiedCraftingItem> craftingItems);
    }

    public class ChildWindowProvider : IChildWindowProvider
    {
        public (string ItemName, SourceType SourceType, IEnumerable<SpecifiedCraftingMaterial> Materials)? ShowAddEditCraftingItemWindow(
            Window owner,
            IEnumerable<CraftingMaterial> definedCraftingMaterials, 
            IEnumerable<CraftingItem> definedCraftingItems, 
            Action<CraftingMaterial> registerNewCraftingMaterial, 
            Action<CraftingItem> registerNewCraftingItem, 
            string suggestedItemName, 
            CraftingItem editItem = null)
        {
            var window = new AddEditCraftingItemWindow(
                definedCraftingMaterials,
                definedCraftingItems,
                registerNewCraftingMaterial,
                registerNewCraftingItem,
                suggestedItemName,
                editItem);

            window.Owner = owner;

            if (!(window.ShowDialog() ?? false))
                return null;

            return (window.ItemName, window.SourceType, window.MaterialsList);
        }

        public IEnumerable<CraftingMaterial> ShowConfigureMaterialsWindow(Window owner, List<CraftingMaterial> materials)
        {
            var window = new ConfigureMaterialsWindow(materials);

            window.Owner = owner;

            if (!(window.ShowDialog() ?? false))
                return null;

            return window.MaterialItems;
        }

        public int? ShowEditCountWindow(Window owner, int count)
        {
            var window = new EditCountWindow(count);

            window.Owner = owner;

            if (!(window.ShowDialog() ?? false))
                return null;

            return window.CountValue;
        }

        public void ShowGenerateListWindow(Window owner, List<SpecifiedCraftingItem> craftingItems)
        {
            var window = new GenerateListWindow(craftingItems);

            window.Owner = owner;

            window.ShowDialog();
        }
    }
}
