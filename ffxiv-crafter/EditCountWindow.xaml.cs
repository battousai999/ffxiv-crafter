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
    /// Interaction logic for EditCountWindow.xaml
    /// </summary>
    public partial class EditCountWindow : Window
    {
        public string Count { get; set; }
        public int CountValue => (Int32.TryParse(Count, out var value) ? (value < 1 ? 1 : value) : 1);

        public EditCountWindow(int count)
        {
            Count = count.ToString();
            DataContext = this;

            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
