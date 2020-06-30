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

        private List<CraftingMaterial> definedMaterialItems = new List<CraftingMaterial>();
        private List<CraftingItem> definedCraftingItems = new List<CraftingItem>();
        private List<SpecifiedCraftingMaterial> materialsList = new List<SpecifiedCraftingMaterial>();
        private Action<CraftingMaterial> registerNewCraftingMaterial;
        private Action<CraftingItem> registerNewCraftingItem;
        private bool isEditing;

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

        public AddEditCraftingItemWindow(
            IEnumerable<CraftingMaterial> definedCraftingMaterials, 
            IEnumerable<CraftingItem> definedCraftingItems, 
            Action<CraftingMaterial> registerNewCraftingMaterial, 
            Action<CraftingItem> registerNewCraftingItem,
            string suggestedItemName, 
            CraftingItem editItem = null)
        {
            if (editItem == null)
            {
                isEditing = false;
                Title = "New Crafting Item";
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

            cbxSourceType.ItemsSource = SourceTypes;
            txtItemName.Focus();
        }

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var itemName = txtAddItemName.Text;
            var itemCountStr = txtAddItemCount.Text;

            if (String.IsNullOrWhiteSpace(itemName))
            {
                MessageBox.Show("Cannot add item with no given name.");
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
                if (MessageBox.Show("This material item hasn't been defined yet. Do you want to define it now?", "Create new material item?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                    return;

                var choiceWindow = new ChooseMaterialTypeWindow();

                choiceWindow.Owner = this;

                if (!(choiceWindow.ShowDialog() ?? false))
                    return;

                if (choiceWindow.MaterialType == ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingItem)
                {
                    var childWindow = new AddEditCraftingItemWindow(definedMaterialItems, definedCraftingItems, registerNewCraftingMaterial, registerNewCraftingItem, itemName);

                    childWindow.Owner = this;

                    if (!(childWindow.ShowDialog() ?? false))
                        return;

                    foundCraftingMaterial = new CraftingItem
                    {
                        Name = childWindow.ItemName,
                        SourceType = childWindow.SourceType
                    };

                    ((CraftingItem)foundCraftingMaterial).SetMaterials(childWindow.MaterialsList.ToList());

                    definedCraftingItems.Add((CraftingItem)foundCraftingMaterial);
                    registerNewCraftingItem((CraftingItem)foundCraftingMaterial);
                    Notify("ValidItemNames");
                }
                else
                {
                    var childWindow = new AddEditCraftingMaterialWindow(itemName, null, name => definedMaterialItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name)));

                    childWindow.Owner = this;

                    if (!(childWindow.ShowDialog() ?? false))
                        return;

                    foundCraftingMaterial = new CraftingMaterial
                    {
                        Name = childWindow.MaterialName,
                        SourceType = childWindow.SourceType,
                        Location = childWindow.Location
                    };

                    definedMaterialItems.Add((CraftingMaterial)foundCraftingMaterial);
                    registerNewCraftingMaterial((CraftingMaterial)foundCraftingMaterial);
                    Notify("ValidItemNames");
                }
            }

            var foundSpecifiedMaterial = materialsList.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, foundCraftingMaterial.Name));

            if (foundSpecifiedMaterial == null)
                materialsList.Add(new SpecifiedCraftingMaterial { Material = foundCraftingMaterial, Count = itemCount });
            else
                foundSpecifiedMaterial.Count += 1;

            txtAddItemName.Text = "";
            txtAddItemCount.Text = "";

            Notify("MaterialsList");
            Utils.ResizeGridViewColumn(gvcMaterialName);

            txtAddItemName.Focus();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing && definedCraftingItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, ItemName)))
            {
                MessageBox.Show($"There is already a crafting item called '{ItemName}'.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterial == null)
                return;

            var childWindow = new EditCountWindow(SelectedMaterial.Count);

            childWindow.Owner = this;

            if (!(childWindow.ShowDialog() ?? false))
                return;

            SelectedMaterial.Count = childWindow.CountValue;
            Notify("MaterialsList");
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterial == null)
                return;

            materialsList.Remove(SelectedMaterial);
            Notify("MaterialsList");
        }
    }
}
