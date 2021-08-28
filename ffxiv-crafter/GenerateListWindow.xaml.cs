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
using System.Windows.Shapes;
using ffxiv_crafter.Models;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for GenerateListWindow.xaml
    /// </summary>
    public partial class GenerateListWindow : Window
    {
        private readonly List<SpecifiedCraftingItem> itemsToBuild;

        public GenerateListWindow(List<SpecifiedCraftingItem> itemsToBuild)
        {
            this.itemsToBuild = itemsToBuild;

            InitializeComponent();

            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            var results = GenerateResults(itemsToBuild);

            flowDocument.Blocks.Add(results);
        }

        private Table GenerateResults(List<SpecifiedCraftingItem> itemsToBuild)
        {
            var materials = itemsToBuild.GetAllMaterials();
            var groupedMaterials = materials.GroupBy(x => x.Material.Location).ToList();

            var table = new Table();

            table.FontFamily = new FontFamily("Arial");

            table.Columns.Add(new TableColumn());
            table.Columns.Add(new TableColumn());

            table.RowGroups.Add(new TableRowGroup());

            var tableRowGroup = table.RowGroups[0];

            Func<string, int, TableRow> createRow = (name, count) =>
            {
                var row = new TableRow();

                row.Cells.Add(new TableCell(new Paragraph(new Run(name))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(count.ToString()))));

                row.FontSize = 20;

                return row;
            };

            Func<string, TableRow> createGroupHeader = location =>
            {
                var row = new TableRow();

                row.Cells.Add(new TableCell(new Paragraph(new Run(location))));
                row.Cells[0].ColumnSpan = 2;

                row.FontWeight = FontWeights.Bold;
                row.FontSize = 30;

                return row;
            };

            Func<TableRow> createBlankRow = () =>
            {
                var row = new TableRow();

                row.Cells.Add(new TableCell());
                row.Cells[0].ColumnSpan = 2;

                row.FontSize = 20;

                return row;
            };

            var sortedGroups = groupedMaterials.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var group in sortedGroups)
            {
                var groupName = String.IsNullOrWhiteSpace(group.Key) ? "No Location" : group.Key;

                tableRowGroup.Rows.Add(createGroupHeader($"{groupName}"));

                var sortedItems = group
                    .OrderBy(x => x.Material.SourceType.ToString(), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var material in sortedItems)
                {
                    var annotation = material.Material.SourceType == SourceType.None ? "" : $" ({material.Material.SourceType})";
                    tableRowGroup.Rows.Add(createRow($"{material.Name}{annotation}", material.Count));
                }

                tableRowGroup.Rows.Add(createBlankRow());
            }

            return table;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyToClickboard_Click(object sender, RoutedEventArgs e)
        {
            var builder = new StringBuilder();
            var materials = itemsToBuild.GetAllMaterials();
            var groupedMaterials = materials.GroupBy(x => x.Material.Location).ToList();
            var sortedGroups = groupedMaterials.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase);
            var isFirstGroup = true;

            foreach (var group in sortedGroups)
            {
                if (isFirstGroup)
                    isFirstGroup = false;
                else
                    builder.AppendLine();

                var groupName = String.IsNullOrWhiteSpace(group.Key) ? "No Location" : group.Key;

                builder.AppendLine(groupName);

                var sortedItems = group
                    .OrderBy(x => x.Material.SourceType.ToString(), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var material in sortedItems)
                {
                    var annotation = material.Material.SourceType == SourceType.None ? "" : $" ({material.Material.SourceType})";
                    builder.AppendLine($"{material.Name}{annotation}\t{material.Count}");
                }
            }

            TextCopy.ClipboardService.SetText(builder.ToString());
        }
    }
}
