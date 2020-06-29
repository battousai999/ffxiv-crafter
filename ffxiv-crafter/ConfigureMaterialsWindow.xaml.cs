using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for ConfigureMaterialsWindow.xaml
    /// </summary>
    public partial class ConfigureMaterialsWindow : Window, INotifyPropertyChanged
    {
        private List<CraftingMaterial> materialItems;

        public IEnumerable<CraftingMaterial> MaterialItems => materialItems.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        public CraftingMaterial SelectedMaterialItem { get; set; }

        public ConfigureMaterialsWindow(List<CraftingMaterial> materialItems)
        {
            this.materialItems = materialItems;
            DataContext = this;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ResizeGridViewColumn(GridViewColumn column)
        {
            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        private void ResizeGridViewColumns()
        {
            ResizeGridViewColumn(gvcMaterialName);
            ResizeGridViewColumn(gvcMaterialSourceType);
            ResizeGridViewColumn(gvcMaterialLocation);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new AddEditCraftingMaterialWindow();

            childWindow.Owner = this;

            if (childWindow.ShowDialog() ?? false)
            {
                var newItem = new CraftingMaterial
                {
                    Name = childWindow.MaterialName,
                    SourceType = childWindow.SourceType,
                    Location = childWindow.Location
                };

                materialItems.Add(newItem);

                Notify("MaterialItems");
                ResizeGridViewColumns();
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterialItem == null)
                return;

            var childWindow = new AddEditCraftingMaterialWindow(SelectedMaterialItem);

            childWindow.Owner = this;

            if (childWindow.ShowDialog() ?? false)
            {
                SelectedMaterialItem.Name = childWindow.MaterialName;
                SelectedMaterialItem.SourceType = childWindow.SourceType;
                SelectedMaterialItem.Location = childWindow.Location;

                Notify("MaterialItems");
                ResizeGridViewColumns();
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterialItem == null)
                return;

            materialItems.Remove(SelectedMaterialItem);

            Notify("MaterialItems");
            ResizeGridViewColumns();
        }
    }
}
