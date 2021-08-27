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
    public class ConfigureCraftingItemsWindowTests
    {
        private const string TestMaterialName = "test-material-name-";
        private const string TestMaterialLocation = "test-material-location-";
        private const string TestCraftingItemName = "test-crafting-item-name-";

        private readonly ConfigureCraftingItemsWindow window;
        private readonly IChildWindowProvider childWindowProvider = Substitute.For<IChildWindowProvider>();
        private readonly List<CraftingItem> definedCraftingItems;
        private readonly List<CraftingMaterial> definedMaterials;
        private readonly List<CraftingMaterial> registeredMaterials = new List<CraftingMaterial>();
        private readonly List<CraftingItem> deletedItems = new List<CraftingItem>();

        public ConfigureCraftingItemsWindowTests()
        {
            definedCraftingItems = new List<CraftingItem>
            {
                new CraftingItem { Name = $"{TestCraftingItemName}1", SourceType = SourceType.Alchemy },
                new CraftingItem { Name = $"{TestCraftingItemName}2", SourceType = SourceType.Alchemy },
                new CraftingItem { Name = $"{TestCraftingItemName}3", SourceType = SourceType.Alchemy },
            };

            definedMaterials = new List<CraftingMaterial>
            {
                new CraftingMaterial { Name = $"{TestMaterialName}1", SourceType = SourceType.None, Location = $"{TestMaterialLocation}1" },
                new CraftingMaterial { Name = $"{TestMaterialName}2", SourceType = SourceType.None, Location = $"{TestMaterialLocation}2" },
                new CraftingMaterial { Name = $"{TestMaterialName}3", SourceType = SourceType.None, Location = $"{TestMaterialLocation}3" }
            };

            window = new ConfigureCraftingItemsWindow(
                childWindowProvider,
                definedCraftingItems,
                definedMaterials,
                item => registeredMaterials.Add(item),
                item => deletedItems.Add(item));
        }

        [UIFact]
        public void DefinedDataPopulated_WhenPassedToConstructor()
        {
            window.CraftingItems.ShouldBe(definedCraftingItems);
        }

        [UIFact]
        public void CraftingItemAddedToList_IfNewItemClickedWithDialogThatReturns()
        {
            var newItemName = $"{TestCraftingItemName}99";
            var newSourceType = SourceType.Weaving;

            var newMaterials = new List<SpecifiedCraftingMaterial>
            {
                new SpecifiedCraftingMaterial
                {
                    Count = 1,
                    Material = new CraftingMaterial
                    {
                        Name = $"{TestMaterialName}101",
                        SourceType = SourceType.None,
                        Location = $"{TestMaterialLocation}201"
                    }
                },
                new SpecifiedCraftingMaterial
                {
                    Count = 1,
                    Material = new CraftingMaterial
                    {
                        Name = $"{TestMaterialName}102",
                        SourceType = SourceType.None,
                        Location = $"{TestMaterialLocation}202"
                    }
                }
            };

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns((newItemName, newSourceType, newMaterials));

            window.NewItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, newItemName));

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(newItemName);
            expectedItem.SourceType.ShouldBe(newSourceType);
            expectedItem.Materials.ShouldBe(newMaterials);
        }

        [UIFact]
        public void CraftingItemNotAddedToList_IfNewItemClickedWithDialogThatDoesNotReturn()
        {
            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns(((string, SourceType, IEnumerable<SpecifiedCraftingMaterial>)?)null);

            window.NewItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(definedCraftingItems);
        }

        [UIFact]
        public void CraftingItemUpdated_IfEditItemClickedWithDialogThatReturns()
        {
            var updatedItemName = $"{TestCraftingItemName}99";
            var updatedSourceType = SourceType.Weaving;

            var updatedMaterials = new List<SpecifiedCraftingMaterial>
            {
                new SpecifiedCraftingMaterial
                {
                    Count = 1,
                    Material = new CraftingMaterial
                    {
                        Name = $"{TestMaterialName}101",
                        SourceType = SourceType.None,
                        Location = $"{TestMaterialLocation}201"
                    }
                },
                new SpecifiedCraftingMaterial
                {
                    Count = 1,
                    Material = new CraftingMaterial
                    {
                        Name = $"{TestMaterialName}102",
                        SourceType = SourceType.None,
                        Location = $"{TestMaterialLocation}202"
                    }
                }
            };

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns((updatedItemName, updatedSourceType, updatedMaterials));

            window.SelectedCraftingItem = window.CraftingItems.First();

            window.EditItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, updatedItemName));

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(updatedItemName);
            expectedItem.SourceType.ShouldBe(updatedSourceType);
            expectedItem.Materials.ShouldBe(updatedMaterials);
        }

        [UIFact]
        public void CraftingItemNotUpdated_IfEditItemClickedWithDialogThatDoesNotReturn()
        {
            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns(((string, SourceType, IEnumerable<SpecifiedCraftingMaterial>)?)null);

            window.SelectedCraftingItem = window.CraftingItems.First();

            window.EditItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(definedCraftingItems);
        }

        [UIFact]
        public void CraftingItemNotUpdated_IfEditItemClickedWithoutItemSelected()
        {
            window.SelectedCraftingItem = null;

            window.EditItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(definedCraftingItems);
        }

        [UIFact]
        public void CraftingItemRemovedFromList_IfDeleteItemClickedWithItemSelected()
        {
            window.SelectedCraftingItem = window.CraftingItems.First();
            var selectedItemName = window.SelectedCraftingItem.Name;

            window.DeleteItem_Click(window, new RoutedEventArgs());

            var itemToHaveBeenDeleted = window.CraftingItems.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, selectedItemName));

            itemToHaveBeenDeleted.ShouldBeNull();
        }

        [UIFact]
        public void CraftingItemNotRemovedFromList_IfDeleteItemClickedWithoutItemSelected()
        {
            window.SelectedCraftingItem = null;

            window.DeleteItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(definedCraftingItems);
        }

        [UIFact]
        public void CraftingItemWillBeNotifiedAsDeleted_IfDeleteItemClickedWithItemSelected()
        {
            window.SelectedCraftingItem = window.CraftingItems.First();
            var selectedItemName = window.SelectedCraftingItem.Name;

            window.DeleteItem_Click(window, new RoutedEventArgs());

            deletedItems.ShouldContain(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, selectedItemName));
        }
    }
}
