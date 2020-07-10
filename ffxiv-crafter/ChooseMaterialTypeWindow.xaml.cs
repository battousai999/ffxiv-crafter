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

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            this.KeyUp += ChooseMaterialTypeWindow_KeyUp;
        }

        public void ChooseMaterialTypeWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.M)
            {
                CraftingMaterial_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.I)
            {
                CraftingItem_Click(this, new RoutedEventArgs());
            }
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public void CraftingItem_Click(object sender, RoutedEventArgs e)
        {
            MaterialType = MaterialTypeChoice.CraftingItem;

            DialogResult = true;
            Close();
        }

        public void CraftingMaterial_Click(object sender, RoutedEventArgs e)
        {
            MaterialType = MaterialTypeChoice.CraftingMaterial;

            DialogResult = true;
            Close();
        }
    }
}
