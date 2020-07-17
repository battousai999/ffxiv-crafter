using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Security.Policy;
using System.ComponentModel;
using ffxiv_crafter.Models;
using ffxiv_crafter.Services;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for AddEditCraftingItemWindow.xaml
    /// </summary>
    public partial class AddEditCraftingItemWindow : Window, INotifyPropertyChanged
    {
        public class SourceTypeItem
        {
            public string Name { get; set; }
            public SourceType Value { get; set; }
        }

        public List<SourceTypeItem> SourceTypes = new List<SourceTypeItem>
        {
            new SourceTypeItem { Name = "Alchemy", Value = SourceType.Alchemy },
            new SourceTypeItem { Name = "Armorcraft", Value = SourceType.Armorcraft },
            new SourceTypeItem { Name = "Blacksmith", Value = SourceType.Blacksmith },
            new SourceTypeItem { Name = "Cooking", Value = SourceType.Cooking },
            new SourceTypeItem { Name = "Goldsmith", Value = SourceType.Goldsmith },
            new SourceTypeItem { Name = "Leatherworking", Value = SourceType.Leatherworking },
            new SourceTypeItem { Name = "Weaving", Value = SourceType.Weaving },
            new SourceTypeItem { Name = "Woodworking", Value = SourceType.Woodworking }
        };

        private readonly INotificationService notificationService;
        private readonly IChildWindowProvider childWindowProvider;
        private List<CraftingMaterial> definedMaterialItems = new List<CraftingMaterial>();
        private List<CraftingItem> definedCraftingItems = new List<CraftingItem>();
        private List<SpecifiedCraftingMaterial> materialsList = new List<SpecifiedCraftingMaterial>();
        private Action<CraftingMaterial> registerNewCraftingMaterial;
        private Action<CraftingItem> registerNewCraftingItem;
        private bool isEditing;
        private string addItemName = "";
        private string addItemCount = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> ValidItemNames => definedMaterialItems
            .Cast<ICraftingMaterial>()
            .Concat(definedCraftingItems)
            .Select(x => x.Name);

        public string ItemName { get; set; }
        public SourceType SourceType => SelectedSourceType?.Value ?? SourceType.Alchemy;
        public SourceTypeItem SelectedSourceType { get; set; }
        public IEnumerable<SpecifiedCraftingMaterial> MaterialsList => materialsList.Select(x => x);
        public SpecifiedCraftingMaterial SelectedMaterial { get; set; }

        public string AddItemName
        {
            get => addItemName;
            set
            {
                addItemName = value;
                Notify(nameof(AddItemName));
            }
        }

        public string AddItemCount
        {
            get => addItemCount;
            set
            {
                addItemCount = value;
                Notify(nameof(AddItemCount));
            }
        }

        public AddEditCraftingItemWindow(
            INotificationService notificationService,
            IChildWindowProvider childWindowProvider,
            IEnumerable<CraftingMaterial> definedCraftingMaterials, 
            IEnumerable<CraftingItem> definedCraftingItems, 
            Action<CraftingMaterial> registerNewCraftingMaterial, 
            Action<CraftingItem> registerNewCraftingItem,
            string suggestedItemName, 
            CraftingItem editItem = null)
        {
            this.notificationService = notificationService;
            this.childWindowProvider = childWindowProvider;

            if (editItem == null)
            {
                isEditing = false;
                ItemName = suggestedItemName;
                SelectedSourceType = SourceTypes.First();
            }
            else
            {
                isEditing = true;
                ItemName = editItem.Name;
                SelectedSourceType = SourceTypes.FirstOrDefault(x => x.Value == editItem.SourceType) ?? SourceTypes.First();
                materialsList = editItem.Materials.ToList();
            }

            this.definedMaterialItems = definedCraftingMaterials.ToList();
            this.definedCraftingItems = definedCraftingItems.ToList();
            this.registerNewCraftingItem = registerNewCraftingItem;
            this.registerNewCraftingMaterial = registerNewCraftingMaterial;

            DataContext = this;

            InitializeComponent();

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            cbxSourceType.ItemsSource = SourceTypes;

            if (!isEditing)
                Title = "New Crafting Item";

            txtAddItemName.Focus();
        }

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var itemName = AddItemName;
            var itemCountStr = AddItemCount;

            if (String.IsNullOrWhiteSpace(itemName))
            {
                notificationService.ShowMessage("Cannot add item with no given name.");
                return;
            }

            if (String.IsNullOrWhiteSpace(itemCountStr) || !Int32.TryParse(itemCountStr, out var itemCount))
                itemCount = 1;

            if (itemCount <= 0)
                itemCount = 1;

            ICraftingMaterial foundCraftingMaterial = definedCraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, itemName));

            if (foundCraftingMaterial == null)
                foundCraftingMaterial = definedMaterialItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, itemName));

            if (foundCraftingMaterial == null)
            {
                var materialType = childWindowProvider.ShowChooseMaterialTypeWindow(this);

                if (materialType == null)
                    return;

                if (materialType == ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingItem)
                {
                    var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                        this,
                        definedMaterialItems,
                        definedCraftingItems,
                        x =>
                        {
                            registerNewCraftingMaterial(x);
                            definedMaterialItems.Add((CraftingMaterial)x);
                            Notify(nameof(ValidItemNames));
                        },
                        x =>
                        {
                            registerNewCraftingItem(x);
                            definedCraftingItems.Add((CraftingItem)x);
                            Notify(nameof(ValidItemNames));
                        },
                        itemName);

                    if (results == null)
                        return;

                    foundCraftingMaterial = new CraftingItem
                    {
                        Name = results.Value.ItemName,
                        SourceType = results.Value.SourceType
                    };

                    ((CraftingItem)foundCraftingMaterial).SetMaterials(results.Value.Materials.ToList());

                    definedCraftingItems.Add((CraftingItem)foundCraftingMaterial);
                    registerNewCraftingItem((CraftingItem)foundCraftingMaterial);
                    Notify(nameof(ValidItemNames));
                }
                else
                {
                    var results = childWindowProvider.ShowAddEditCraftingMaterialWindow(
                        this,
                        itemName,
                        null,
                        name => definedMaterialItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name)));

                    if (results == null)
                        return;

                    foundCraftingMaterial = new CraftingMaterial
                    {
                        Name = results.Value.MaterialName,
                        SourceType = results.Value.SourceType,
                        Location = results.Value.Location
                    };

                    definedMaterialItems.Add((CraftingMaterial)foundCraftingMaterial);
                    registerNewCraftingMaterial((CraftingMaterial)foundCraftingMaterial);
                    Notify(nameof(ValidItemNames));
                }
            }

            var foundSpecifiedMaterial = materialsList.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, foundCraftingMaterial.Name));

            if (foundSpecifiedMaterial == null)
                materialsList.Add(new SpecifiedCraftingMaterial { Material = foundCraftingMaterial, Count = itemCount });
            else
                foundSpecifiedMaterial.Count += 1;

            AddItemName = "";
            AddItemCount = "";

            Notify(nameof(MaterialsList));
            Utils.ResizeGridViewColumn(gvcMaterialName);

            txtAddItemName.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditing && definedCraftingItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, ItemName)))
            {
                notificationService.ShowMessage($"There is already a crafting item called '{ItemName}'.");
                return;
            }

            DialogResult = true;
            Close();
        }

        public void EditCount_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterial == null)
                return;

            var newCount = childWindowProvider.ShowEditCountWindow(this, SelectedMaterial.Count);

            if (newCount == null)
                return;

            SelectedMaterial.Count = newCount.Value;
            Notify(nameof(MaterialsList));
        }

        public void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterial == null)
                return;

            materialsList.Remove(SelectedMaterial);
            Notify(nameof(MaterialsList));
        }

        public void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterial == null)
                return;

            if (SelectedMaterial.Material is CraftingItem)
            {
                var craftingItem = SelectedMaterial.Material as CraftingItem;

                var results = childWindowProvider.ShowAddEditCraftingItemWindow(
                    this,
                    definedMaterialItems,
                    definedCraftingItems,
                    x =>
                    {
                        registerNewCraftingMaterial(x);
                        definedMaterialItems.Add((CraftingMaterial)x);
                        Notify(nameof(ValidItemNames));
                    },
                    x =>
                    {
                        registerNewCraftingItem(x);
                        definedCraftingItems.Add((CraftingItem)x);
                        Notify(nameof(ValidItemNames));
                    },
                    null,
                    craftingItem.Clone());

                if (results == null)
                    return;

                craftingItem.Name = results.Value.ItemName;
                craftingItem.SourceType = results.Value.SourceType;
                craftingItem.SetMaterials(results.Value.Materials);
            }
            else
            {
                var material = SelectedMaterial.Material as CraftingMaterial;

                var results = childWindowProvider.ShowAddEditCraftingMaterialWindow(
                    this,
                    null,
                    material.Clone(),
                    name => definedMaterialItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name)));

                if (results == null)
                    return;

                material.Name = results.Value.MaterialName;
                material.SourceType = results.Value.SourceType;
                material.Location = results.Value.Location;
            }

            Notify(nameof(ValidItemNames));
            Notify(nameof(MaterialsList));
            Utils.ResizeGridViewColumn(gvcMaterialName);
        }
    }
}
