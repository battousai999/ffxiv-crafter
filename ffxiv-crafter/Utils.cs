using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace ffxiv_crafter
{
    public static class Utils
    {
        public static void ResizeGridViewColumn(GridViewColumn column)
        {
            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }
    }
}
