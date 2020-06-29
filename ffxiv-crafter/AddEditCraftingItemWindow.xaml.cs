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

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for AddEditCraftingItemWindow.xaml
    /// </summary>
    public partial class AddEditCraftingItemWindow : Window
    {
        private List<CraftingMaterial> materialItems = new List<CraftingMaterial>();
        private List<CraftingItem> craftingItems = new List<CraftingItem>();

        public IEnumerable<string> ValidItemNames => materialItems
            .Cast<ICraftingMaterial>()
            .Concat(craftingItems)
            .Select(x => x.Name);

        public AddEditCraftingItemWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        public void AddItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
