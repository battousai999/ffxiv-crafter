using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using ffxiv_crafter.Models;
using ffxiv_crafter.Services;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for AddEditCraftingMaterialWindow.xaml
    /// </summary>
    public partial class AddEditCraftingMaterialWindow : Window
    {
        public class SourceTypeItem
        {
            public string Name { get; set; }
            public SourceType Value { get; set; }
        }

        private Func<string, bool> nameAlreadyExists;
        private readonly INotificationService notificationService;

        public List<SourceTypeItem> SourceTypes = new List<SourceTypeItem>
        {
            new SourceTypeItem { Name = "Drop", Value = SourceType.None },
            new SourceTypeItem { Name = "Botany", Value = SourceType.Botany },
            new SourceTypeItem { Name = "Mining", Value = SourceType.Mining },
            new SourceTypeItem { Name = "Fishing", Value = SourceType.Fishing }
        };

        public string MaterialName { get; set; }
        public SourceTypeItem SelectedSourceType { get; set; }
        public SourceType SourceType => SelectedSourceType?.Value ?? SourceType.None;
        public string Location { get; set; } = "";

        public AddEditCraftingMaterialWindow(INotificationService notificationService, string suggestedName = null, CraftingMaterial editMaterial = null, Func<string, bool> nameAlreadyExists = null)
        {
            this.notificationService = notificationService;

            if (editMaterial == null)
            {
                MaterialName = suggestedName ?? "";
                SelectedSourceType = SourceTypes.First();
            }
            else
            {
                MaterialName = editMaterial.Name;
                SelectedSourceType = SourceTypes.FirstOrDefault(x => x.Value == editMaterial.SourceType) ?? SourceTypes.First();
                Location = editMaterial.Location;
            }

            this.nameAlreadyExists = (nameAlreadyExists ?? (_ => false));

            DataContext = this;

            InitializeComponent();

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            if (editMaterial == null)
                Title = "New Crafting Material";

            cbxSourceType.ItemsSource = SourceTypes;
            txtMaterialName.Focus();
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MaterialName))
            {
                notificationService.ShowMessage("Cannot save material name with no name.");
                return;
            }

            if (nameAlreadyExists(MaterialName))
            {
                notificationService.ShowMessage($"There is already a crafting item called '{MaterialName}'.");
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
