using System;
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

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for ChooseMaterialTypeWindow.xaml
    /// </summary>
    public partial class ChooseMaterialTypeWindow : Window
    {
        public enum MaterialTypeChoice
        {
            CraftingItem,
            CraftingMaterial
        }

        public MaterialTypeChoice MaterialType { get; set; }

        public ChooseMaterialTypeWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CraftingItem_Click(object sender, RoutedEventArgs e)
        {
            MaterialType = MaterialTypeChoice.CraftingItem;

            DialogResult = true;
            Close();
        }

        private void CraftingMaterial_Click(object sender, RoutedEventArgs e)
        {
            MaterialType = MaterialTypeChoice.CraftingMaterial;

            DialogResult = true;
            Close();
        }
    }
}
