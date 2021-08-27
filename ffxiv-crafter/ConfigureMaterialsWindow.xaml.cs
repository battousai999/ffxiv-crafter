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
using ffxiv_crafter.Models;
using ffxiv_crafter.Services;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for ConfigureMaterialsWindow.xaml
    /// </summary>
    public partial class ConfigureMaterialsWindow : Window, INotifyPropertyChanged
    {
        private readonly IChildWindowProvider childWindowProvider;
        private readonly List<CraftingMaterial> materialItems;

        public IEnumerable<CraftingMaterial> MaterialItems => materialItems.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        public CraftingMaterial SelectedMaterialItem { get; set; }

        public ConfigureMaterialsWindow(IChildWindowProvider childWindowProvider, List<CraftingMaterial> materialItems)
        {
            this.childWindowProvider = childWindowProvider;
            this.materialItems = materialItems;
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
            Utils.ResizeGridViewColumn(gvcMaterialName);
            Utils.ResizeGridViewColumn(gvcMaterialSourceType);
            Utils.ResizeGridViewColumn(gvcMaterialLocation);
        }

        public void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void NewItem_Click(object sender, RoutedEventArgs e)
        {
            var results = childWindowProvider.ShowAddEditCraftingMaterialWindow(
                this,
                null,
                null,
                name => materialItems.Any(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name)));

            if (results == null)
                return;

            var newItem = new CraftingMaterial
            {
                Name = results.Value.MaterialName,
                SourceType = results.Value.SourceType,
                Location = results.Value.Location
            };

            materialItems.Add(newItem);

            Notify(nameof(MaterialItems));
            ResizeGridViewColumns();
        }

        public void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterialItem == null)
                return;

            var results = childWindowProvider.ShowAddEditCraftingMaterialWindow(
                this,
                null,
                SelectedMaterialItem);

            if (results == null)
                return;

            SelectedMaterialItem.Name = results.Value.MaterialName;
            SelectedMaterialItem.SourceType = results.Value.SourceType;
            SelectedMaterialItem.Location = results.Value.Location;

            Notify(nameof(MaterialItems));
            ResizeGridViewColumns();
        }

        public void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedMaterialItem == null)
                return;

            materialItems.Remove(SelectedMaterialItem);

            SelectedMaterialItem = null;

            Notify(nameof(MaterialItems));
            ResizeGridViewColumns();
        }
    }
}
