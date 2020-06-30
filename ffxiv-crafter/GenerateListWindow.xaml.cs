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
using System.Windows.Shapes;

namespace ffxiv_crafter
{
    /// <summary>
    /// Interaction logic for GenerateListWindow.xaml
    /// </summary>
    public partial class GenerateListWindow : Window
    {
        public GenerateListWindow(List<SpecifiedCraftingItem> itemsToBuild)
        {
            InitializeComponent();

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

            foreach (var group in groupedMaterials)
            {
                var groupName = String.IsNullOrWhiteSpace(group.Key) ? "No Location" : group.Key;

                tableRowGroup.Rows.Add(createGroupHeader($"[{groupName}]"));

                foreach (var material in group)
                {
                    tableRowGroup.Rows.Add(createRow(material.Name, material.Count));
                }

                tableRowGroup.Rows.Add(createBlankRow());
            }

            return table;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
