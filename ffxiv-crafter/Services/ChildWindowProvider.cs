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

        (string MaterialName, SourceType SourceType, string Location)? ShowAddEditCraftingMaterialWindow(
            Window owner,
            string suggestedName = null,
            CraftingMaterial editMaterial = null,
            Func<string, bool> nameAlreadyExists = null);

        IEnumerable<CraftingMaterial> ShowConfigureMaterialsWindow(Window owner, List<CraftingMaterial> materials);
        int? ShowEditCountWindow(Window owner, int count);
        void ShowGenerateListWindow(Window owner, List<SpecifiedCraftingItem> craftingItems);
        ChooseMaterialTypeWindow.MaterialTypeChoice? ShowChooseMaterialTypeWindow(Window owner);
    }

    public class ChildWindowProvider : IChildWindowProvider
    {
        private readonly INotificationService notificationService;

        public ChildWindowProvider(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

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
                notificationService,
                this,
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

        public (string MaterialName, SourceType SourceType, string Location)? ShowAddEditCraftingMaterialWindow(
            Window owner,
            string suggestedName = null, 
            CraftingMaterial editMaterial = null, 
            Func<string, bool> nameAlreadyExists = null)
        {
            var window = new AddEditCraftingMaterialWindow(notificationService, suggestedName, editMaterial, nameAlreadyExists);

            window.Owner = owner;

            if (!(window.ShowDialog() ?? false))
                return null;

            return (window.MaterialName, window.SourceType, window.Location);
        }

        public IEnumerable<CraftingMaterial> ShowConfigureMaterialsWindow(Window owner, List<CraftingMaterial> materials)
        {
            var window = new ConfigureMaterialsWindow(this, materials);

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

        public ChooseMaterialTypeWindow.MaterialTypeChoice? ShowChooseMaterialTypeWindow(Window owner)
        {
            var window = new ChooseMaterialTypeWindow();

            window.Owner = owner;

            if (!(window.ShowDialog() ?? false))
                return null;

            return window.MaterialType;
        }
    }
}
