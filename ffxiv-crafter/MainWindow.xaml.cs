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

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<CraftingItem> validCraftingItems = new List<CraftingItem>();
        private List<CraftingMaterial> materialItems = new List<CraftingMaterial>();
        private List<SpecifiedCraftingItem> craftingItems = new List<SpecifiedCraftingItem>();

        public IEnumerable<SpecifiedCraftingItem> CraftingItems => craftingItems.Select(x => x);
        public IEnumerable<string> ValidItemNames => validCraftingItems.Select(x => x.Name);
        public SpecifiedCraftingItem SelectedCraftingItem { get; set; }

        public MainWindow()
        {
            DataContext = this;

            validCraftingItems.Add(new CraftingItem { Name = "Crossbow", SourceType = SourceType.Woodworking });
            validCraftingItems.Add(new CraftingItem { Name = "Iron sword", SourceType = SourceType.Blacksmith });

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

            var foundCraftingItem = validCraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, itemName));

            if (foundCraftingItem == null)
            {
                if (MessageBox.Show("This crafting item hasn't been defined yet. Do you want to define it now?", "Create new crafting item?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                    return;

                var childWindow = new AddEditCraftingItemWindow(
                    materialItems,
                    validCraftingItems,
                    item => materialItems.Add(item),
                    item => { validCraftingItems.Add(item); Notify("ValidItemNames"); },
                    itemName);

                childWindow.Owner = this;

                if (childWindow.ShowDialog() ?? false)
                {
                    var craftingItem = new CraftingItem
                    {
                        Name = childWindow.ItemName,
                        SourceType = childWindow.SourceType
                    };

                    craftingItem.SetMaterials(childWindow.MaterialsList);

                    foundCraftingItem = craftingItem;
                }
                else
                    return;
            }

            var foundSpecifiedItem = craftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, foundCraftingItem.Name));

            if (foundSpecifiedItem == null)
                craftingItems.Add(new SpecifiedCraftingItem { Item = foundCraftingItem, Count = itemCount });
            else
                foundSpecifiedItem.Count += 1;

            txtAddItemName.Text = "";
            txtAddItemCount.Text = "";

            Notify("CraftingItems");
            Utils.ResizeGridViewColumn(gvcItemName);

            txtAddItemName.Focus();
        }

        private void ConfigureItems_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new ConfigureMaterialsWindow(materialItems.ToList());

            childWindow.Owner = this;

            if (childWindow.ShowDialog() ?? false)
            {
                materialItems = childWindow.MaterialItems.ToList();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            craftingItems.Remove(SelectedCraftingItem);

            Notify("CraftingItems");
            Utils.ResizeGridViewColumn(gvcItemName);
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCraftingItem == null)
                return;

            var childWindow = new AddEditCraftingItemWindow(
                materialItems, 
                validCraftingItems,
                item => materialItems.Add(item),
                item => { validCraftingItems.Add(item); Notify("ValidItemNames"); },
                null, 
                SelectedCraftingItem.Item);

            childWindow.Owner = this;

            if (childWindow.ShowDialog() ?? false)
            {
                SelectedCraftingItem.Item.Name = childWindow.ItemName;
                SelectedCraftingItem.Item.SourceType = childWindow.SourceType;
                SelectedCraftingItem.Item.SetMaterials(childWindow.MaterialsList);

                Notify("CraftingItems");
                Utils.ResizeGridViewColumn(gvcItemName);
            }
        }
    }
}
