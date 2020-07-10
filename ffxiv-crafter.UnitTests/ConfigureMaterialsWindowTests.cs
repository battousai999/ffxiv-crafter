using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NSubstitute;
using Shouldly;
using ffxiv_crafter.Services;
using ffxiv_crafter.Models;
using System.Windows;
using System.Linq;

namespace ffxiv_crafter.UnitTests
{
    public class ConfigureMaterialsWindowTests
    {
        private const string TestItemName = "test-item-name-";
        private const string TestLocation = "test-location-";

        private readonly ConfigureMaterialsWindow window;
        private readonly IChildWindowProvider childWindowProvider = Substitute.For<IChildWindowProvider>();
        private readonly List<CraftingMaterial> definedCraftingMaterials;

        public ConfigureMaterialsWindowTests()
        {
            definedCraftingMaterials = new List<CraftingMaterial>
            {
                new CraftingMaterial { Name = $"{TestItemName}1", SourceType = SourceType.None, Location = $"{TestLocation}1" },
                new CraftingMaterial { Name = $"{TestItemName}2", SourceType = SourceType.None, Location = $"{TestLocation}2" },
                new CraftingMaterial { Name = $"{TestItemName}3", SourceType = SourceType.None, Location = $"{TestLocation}3" }
            };

            window = new ConfigureMaterialsWindow(childWindowProvider, definedCraftingMaterials);
        }

        [UIFact]
        public void DefinedDataPopulated_WhenPassedToConstructor()
        {
            window.MaterialItems.ShouldBe(definedCraftingMaterials);
        }

        [UIFact]
        public void MaterialItemAddedToList_IfNewItemClickedWithDialogThatReturns()
        {
            var newItemName = $"{TestItemName}99";
            var newSourceType = SourceType.Botany;
            var newLocation = $"{TestLocation}99";

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns((newItemName, newSourceType, newLocation));

            window.NewItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, newItemName));

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(newItemName);
            expectedItem.SourceType.ShouldBe(newSourceType);
            expectedItem.Location.ShouldBe(newLocation);
        }

        [UIFact]
        public void MaterialItemNotAddedToList_IfNewItemClickedWithDialogThatDoesNotReturn()
        {
            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns(((string MaterialType, SourceType SourceType, string Location)?)null);

            window.NewItem_Click(window, new RoutedEventArgs());

            window.MaterialItems.ShouldBe(definedCraftingMaterials);
        }

        [UIFact]
        public void MaterialItemUpdated_IfEditItemClickedWithDialogThatReturns()
        {
            var updatedItemName = $"{TestItemName}99";
            var updatedSourceType = SourceType.Botany;
            var updatedLocation = $"{TestLocation}99";

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns((updatedItemName, updatedSourceType, updatedLocation));

            window.SelectedMaterialItem = window.MaterialItems.First();

            window.EditItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, updatedItemName));

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(updatedItemName);
            expectedItem.SourceType.ShouldBe(updatedSourceType);
            expectedItem.Location.ShouldBe(updatedLocation);
        }

        [UIFact]
        public void MaterialItemNotUpdated_IfEditItemClickedWithDialogThatDoesNotReturn()
        {
            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns(((string MaterialName, SourceType SourceType, string Location)?)null);

            window.SelectedMaterialItem = window.MaterialItems.First();

            window.EditItem_Click(window, new RoutedEventArgs());

            window.MaterialItems.ShouldBe(definedCraftingMaterials);
        }

        [UIFact]
        public void MaterialItemNotUpdated_IfEditItemClickedWithoutItemSelected()
        {
            window.SelectedMaterialItem = null;

            window.EditItem_Click(window, new RoutedEventArgs());

            window.MaterialItems.ShouldBe(definedCraftingMaterials);
        }

        [UIFact]
        public void MaterialItemRemovedFromList_IfDeleteItemClickedWithItemSelected()
        {
            window.SelectedMaterialItem = window.MaterialItems.First();
            var selectedItemName = window.SelectedMaterialItem.Name;

            window.DeleteItem_Click(window, new RoutedEventArgs());

            var itemToHaveBeenDeleted = window.MaterialItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, selectedItemName));

            itemToHaveBeenDeleted.ShouldBeNull();
        }

        [UIFact]
        public void MaterialItemNotRemovedFromList_IfDeleteItemClickedWithoutItemSelected()
        {
            window.SelectedMaterialItem = null;

            window.DeleteItem_Click(window, new RoutedEventArgs());

            window.MaterialItems.ShouldBe(definedCraftingMaterials);
        }
    }
}
