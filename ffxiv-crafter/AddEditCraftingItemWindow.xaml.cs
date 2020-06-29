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

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<string> ValidItemNames => definedMaterialItems
            .Cast<ICraftingMaterial>()
            .Concat(definedCraftingItems)
            .Select(x => x.Name);

        public string ItemName { get; set; }
        public SourceType SourceType => SelectedSourceType?.Value ?? SourceType.Alchemy;
        public SourceTypeItem SelectedSourceType { get; set; }
        public IEnumerable<SpecifiedCraftingMaterial> MaterialsList => materialsList.Select(x => x);
        public SpecifiedCraftingMaterial SelectedMaterial;

        public AddEditCraftingItemWindow(IEnumerable<CraftingMaterial> definedCraftingMaterials, IEnumerable<CraftingItem> definedCraftingItems, string suggestedItemName, CraftingItem editItem = null)
        {
            if (editItem == null)
            {
                Title = "New Crafting Item";
                ItemName = suggestedItemName;
                SelectedSourceType = SourceTypes.First();
            }
            else
            {
                ItemName = editItem.Name;
                SelectedSourceType = SourceTypes.FirstOrDefault(x => x.Value == editItem.SourceType) ?? SourceTypes.First();
                materialsList = editItem.Materials.ToList();
            }

            this.definedMaterialItems = definedCraftingMaterials.ToList();
            this.definedCraftingItems = definedCraftingItems.ToList();

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

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (definedCraftingItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, ItemName)))
            {
                MessageBox.Show($"There is already a crafting item called '{ItemName}'.");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
