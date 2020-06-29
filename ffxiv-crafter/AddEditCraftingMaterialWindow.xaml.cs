﻿using System;
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
        public string Location { get; set; }

        public AddEditCraftingMaterialWindow(CraftingMaterial editMaterial = null)
        {
            if (editMaterial == null)
            {
                Title = "New Crafting Material";
                SelectedSourceType = SourceTypes.First();
            }
            else
            {
                MaterialName = editMaterial.Name;
                SelectedSourceType = SourceTypes.FirstOrDefault(x => x.Value == editMaterial.SourceType) ?? SourceTypes.First();
                Location = editMaterial.Location;
            }

            DataContext = this;

            InitializeComponent();

            cbxSourceType.ItemsSource = SourceTypes;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MaterialName))
            {
                MessageBox.Show("Cannot save material name with no name.");
                return;
            }

            if (String.IsNullOrWhiteSpace(Location))
            {
                if (MessageBox.Show("Are you sure that you want to leave the location empty?", "Location empty", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                    return;
            }

            DialogResult = true;
            Close();
        }
    }
}
