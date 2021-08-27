using ffxiv_crafter.Models;
using ffxiv_crafter.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for ConfigureCraftingItemsWindow.xaml
    /// </summary>
    public partial class ConfigureCraftingItemsWindow : Window, INotifyPropertyChanged
    {
        private readonly IChildWindowProvider childWindowProvider;
        private readonly List<CraftingItem> craftingItems;
        private readonly List<CraftingMaterial> craftingMaterials;
        private readonly Action<CraftingMaterial> registerNewCraftingMaterial;
        private readonly Action<CraftingItem> deleteCraftingItem;

        public IEnumerable<CraftingItem> CraftingItems => craftingItems.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        public CraftingItem SelectedCraftingItem { get; set; }

        public ConfigureCraftingItemsWindow(
            IChildWindowProvider childWindowProvider,
            List<CraftingItem> craftingItems,
            List<CraftingMaterial> craftingMaterials,
            Action<CraftingMaterial> registerNewCraftingMaterial,
            Action<CraftingItem> deleteCraftingItem)
        {
            this.childWindowProvider = childWindowProvider;
            this.craftingItems = craftingItems;
            this.craftingMaterials = craftingMaterials;
            this.registerNewCraftingMaterial = registerNewCraftingMaterial;
            this.deleteCraftingItem = deleteCraftingItem;

            DataContext = this;

            InitializeComponent();

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResizeGridViewColumns()
        {
            Utils.ResizeGridViewColumn(gvcCraftingItemName);
            Utils.ResizeGridViewColumn(gvcCraftingItemSourceType);
        }

        public void NewItem_Click(object sender, RoutedEventArgs e)
        {
            var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                this,
                craftingMaterials,
                craftingItems,
                registerNewCraftingMaterial,
                item => { craftingItems.Add(item); Notify(nameof(CraftingItems)); },
                null,
                null);

            if (results == null)
                return;

            var newItem = new CraftingItem
            {
                Name = results.Value.ItemName,
                SourceType = results.Value.SourceType
            };

            newItem.SetMaterials(results.Value.Materials);

            craftingItems.Add(newItem);

            Notify(nameof(CraftingItems));
            ResizeGridViewColumns();
        }

        public void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                this,
                craftingMaterials,
                craftingItems,
                registerNewCraftingMaterial,
                item => { craftingItems.Add(item); Notify(nameof(CraftingItems)); },
                null,
                SelectedCraftingItem);

            if (results == null)
                return;

            SelectedCraftingItem.Name = results.Value.ItemName;
            SelectedCraftingItem.SourceType = results.Value.SourceType;
            SelectedCraftingItem.SetMaterials(results.Value.Materials);

            Notify(nameof(CraftingItems));
            ResizeGridViewColumns();
        }

        public void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            craftingItems.Remove(SelectedCraftingItem);
            deleteCraftingItem(SelectedCraftingItem);

            SelectedCraftingItem = null;

            Notify(nameof(CraftingItems));
            ResizeGridViewColumns();
        }

        public void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
