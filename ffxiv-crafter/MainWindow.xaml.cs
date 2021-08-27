using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using ffxiv_crafter.Serialization;
using System.IO;
using ffxiv_crafter.Models;
using ffxiv_crafter.Services;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<CraftingItem> validCraftingItems;
        private List<CraftingMaterial> materialItems;
        private List<SpecifiedCraftingItem> craftingItems = new List<SpecifiedCraftingItem>();
        private readonly IChildWindowProvider childWindowProvider;
        private readonly IFileSystemService fileSystemService;
        private readonly INotificationService notificationService;
        private string itemName = "";
        private string itemCount = "";

        public IEnumerable<SpecifiedCraftingItem> CraftingItems => craftingItems.Select(x => x);
        public IEnumerable<string> ValidItemNames => validCraftingItems.Select(x => x.Name);
        public IEnumerable<string> MaterialItemNames => materialItems.Select(x => x.Name);
        public SpecifiedCraftingItem SelectedCraftingItem { get; set; }

        public string ItemName 
        {
            get => itemName;
            set
            {
                itemName = value;
                Notify(nameof(ItemName));
            }
        }

        public string ItemCount 
        { 
            get => itemCount; 
            set
            {
                itemCount = value;
                Notify(nameof(ItemCount));
            }
        }

        public MainWindow(
            IInitialDataService initialDataService, 
            IChildWindowProvider childWindowProvider, 
            IFileSystemService fileSystemService,
            INotificationService notificationService)
        {
            this.childWindowProvider = childWindowProvider;
            this.fileSystemService = fileSystemService;
            this.notificationService = notificationService;

            validCraftingItems = initialDataService.GetCraftingItems().ToList();
            materialItems = initialDataService.GetCraftingMaterials().ToList();

            DataContext = this;

            InitializeComponent();

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var itemCountStr = ItemCount;

            if (String.IsNullOrWhiteSpace(ItemName))
            {
                notificationService.ShowMessage("Cannot add item with no given name.");
                return;
            }

            if (String.IsNullOrWhiteSpace(itemCountStr) || !Int32.TryParse(itemCountStr, out var itemCount))
                itemCount = 1;

            if (itemCount <= 0)
                itemCount = 1;

            var foundCraftingItem = validCraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, ItemName));

            if (foundCraftingItem == null)
            {
                var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                    this,
                    materialItems,
                    validCraftingItems,
                    item => materialItems.Add(item),
                    item => { validCraftingItems.Add(item); Notify(nameof(ValidItemNames)); },
                    ItemName);

                if (results == null)
                    return;

                var craftingItem = new CraftingItem
                {
                    Name = results.Value.ItemName,
                    SourceType = results.Value.SourceType
                };

                craftingItem.SetMaterials(results.Value.Materials);

                validCraftingItems.Add(craftingItem);
                foundCraftingItem = craftingItem;
            }

            var foundSpecifiedItem = craftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, foundCraftingItem.Name));

            if (foundSpecifiedItem == null)
                craftingItems.Add(new SpecifiedCraftingItem { Item = foundCraftingItem, Count = itemCount });
            else
                foundSpecifiedItem.Count += 1;

            ItemName = "";
            ItemCount = "";

            Notify(nameof(CraftingItems));
            Utils.ResizeGridViewColumn(gvcItemName);

            txtAddItemName.Focus();
        }

        public void ConfigureMaterials_Click(object sender, RoutedEventArgs e)
        {
            var results = childWindowProvider.ShowConfigureMaterialsWindow(this, materialItems.ToList());

            if (results == null)
                return;

            var deletedItems = materialItems.Except(results).ToList();
            materialItems = results.ToList();

            validCraftingItems.ForEach(x => x.DeleteMaterials(deletedItems));
        }

        public void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            craftingItems.Remove(SelectedCraftingItem);

            Notify(nameof(CraftingItems));
            Utils.ResizeGridViewColumn(gvcItemName);
        }

        public void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                this,
                materialItems,
                validCraftingItems,
                item => materialItems.Add(item),
                item => { validCraftingItems.Add(item); Notify(nameof(ValidItemNames)); },
                null,
                SelectedCraftingItem.Item);

            if (results == null)
                return;

            SelectedCraftingItem.Item.Name = results.Value.ItemName;
            SelectedCraftingItem.Item.SourceType = results.Value.SourceType;
            SelectedCraftingItem.Item.SetMaterials(results.Value.Materials);

            Notify(nameof(CraftingItems));
            Utils.ResizeGridViewColumn(gvcItemName);
        }

        public void EditCount_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            var newCount = childWindowProvider.ShowEditCountWindow(this, SelectedCraftingItem.Count);

            if (newCount == null)
                return;

            SelectedCraftingItem.Count = newCount.Value;

            Notify(nameof(CraftingItems));
        }

        public void GenerateList_Click(object sender, RoutedEventArgs e)
        {
            childWindowProvider.ShowGenerateListWindow(this, craftingItems);
        }

        public void Load_Click(object sender, RoutedEventArgs e)
        {
            var filename = fileSystemService.GetOpenFilename();

            if (filename == null)
                return;

            var fileData = fileSystemService.ReadAllText(filename);
            var data = SaveDataJsonAdapter.FromJson(fileData);

            validCraftingItems = data.DefinedCraftingItems;
            materialItems = data.DefinedCraftingMaterials;
            craftingItems = data.CraftingItems;

            Notify(nameof(ValidItemNames));
            Notify(nameof(CraftingItems));
            Utils.ResizeGridViewColumn(gvcItemName);
        }

        public void Save_Click(object sender, RoutedEventArgs e)
        {
            var filename = fileSystemService.GetSaveFilename();

            if (filename == null)
                return;

            var data = SaveDataJsonAdapter.ToJson(validCraftingItems, materialItems, craftingItems);

            fileSystemService.WriteAllText(filename, data);
        }

        public void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (!notificationService.ShowConfirmation("Are you sure that you want to clear all crafting items?", "Clear All Items"))
                return;

            craftingItems.RemoveAll(_ => true);

            Notify(nameof(CraftingItems));
        }

        private void txtAddItemName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddItem_Click(sender, new RoutedEventArgs());
        }

        public void ConfigureItems_Click(object sender, RoutedEventArgs e)
        {
            var results = childWindowProvider.ShowConfigureItemsWindow(
                this,
                validCraftingItems,
                materialItems,
                item => materialItems.Add(item),
                item => craftingItems.RemoveAll(x => x.Item == item));

            if (results == null)
                return;

            validCraftingItems = results.ToList();

            Notify(nameof(ValidItemNames));
            Notify(nameof(CraftingItems));
            Utils.ResizeGridViewColumn(gvcItemName);
        }
    }
}
