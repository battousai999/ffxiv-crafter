using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using ffxiv_crafter.Models;

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

        public static IEnumerable<T> ToSingleton<T>(this T item)
        {
            yield return item;
        }

        public static IEnumerable<SpecifiedCraftingMaterial> GetAllMaterials(this IEnumerable<SpecifiedCraftingItem> items)
        {
            return SpecifiedCraftingItem.GetMaterialsFor(items);
        }
    }
}
