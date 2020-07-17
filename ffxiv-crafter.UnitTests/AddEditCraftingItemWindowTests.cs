using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using NSubstitute;
using ffxiv_crafter.Models;
using System.Linq;
using ffxiv_crafter.Services;
using System.Windows;
using System.Security.RightsManagement;

namespace ffxiv_crafter.UnitTests
{
    public class AddEditCraftingItemWindowTests
    {
        private const string TestNewItemName = "test-new-item-name-1";
        private const string TestEditedItemName = "test-edited-item-name-1";
        private const string TestCraftingMaterialName = "test-crafting-material-name-1";
        private const string TestCraftingItemName = "test-crafting-item-name-1";
        private const string TestLocation = "test-location-1";

        private readonly List<CraftingMaterial> registeredCraftingMaterials = new List<CraftingMaterial>();
        private readonly List<CraftingItem> registeredCraftingItems = new List<CraftingItem>();
        private readonly CraftingItem editedCraftingItem;
        private readonly List<CraftingMaterial> definedCraftingMaterials;
        private readonly List<CraftingItem> definedCraftingItems;
        private readonly INotificationService notificationService = Substitute.For<INotificationService>();
        private readonly IChildWindowProvider childWindowProvider = Substitute.For<IChildWindowProvider>();

        public AddEditCraftingItemWindowTests()
        {
            definedCraftingMaterials = new List<CraftingMaterial>
            {
                new CraftingMaterial { Name = TestCraftingMaterialName, SourceType = SourceType.Mining, Location = TestLocation }
            };

            var craftingItem = BuildCraftingItem(
                TestCraftingItemName, 
                SourceType.Cooking, 
                (1, definedCraftingMaterials.First()));

            editedCraftingItem = BuildCraftingItem(
                TestEditedItemName, 
                SourceType.Leatherworking, 
                (1, definedCraftingMaterials.First()),
                (2, craftingItem));

            definedCraftingItems = new List<CraftingItem>
            {
                craftingItem,
                BuildCraftingItem(TestCraftingItemName + "2", SourceType.Alchemy, (3, definedCraftingMaterials.First())),
                editedCraftingItem
            };
        }

        private CraftingItem BuildCraftingItem(string name, SourceType sourceType, params (int count, ICraftingMaterial material)[] materials)
        {
            var item = new CraftingItem
            {
                Name = name,
                SourceType = sourceType
            };

            item.SetMaterials(materials.Select(x => BuildSpecifiedCraftingMaterial(x.count, x.material)));

            return item;
        }

        private SpecifiedCraftingMaterial BuildSpecifiedCraftingMaterial(int count, ICraftingMaterial material)
        {
            return new SpecifiedCraftingMaterial { Material = material, Count = count };
        }

        private AddEditCraftingItemWindow GetSubject(string suggestedItemName, CraftingItem editItem = null)
        {
            return new AddEditCraftingItemWindow(
                notificationService,
                childWindowProvider,
                definedCraftingMaterials,
                definedCraftingItems,
                x => registeredCraftingMaterials.Add(x),
                x => registeredCraftingItems.Add(x),
                suggestedItemName,
                editItem);
        }

        [UIFact]
        public void NoDataDisplayed_IfWindowOpenedInNewItemMode()
        {
            var window = GetSubject(TestNewItemName);

            window.ItemName.ShouldBe(TestNewItemName);
            window.SelectedSourceType.ShouldBe(window.SourceTypes.First());
            window.MaterialsList.Count().ShouldBe(0);
        }

        [UIFact]
        public void DataDisplayed_IfWindowOpenedInEditItemMode()
        {
            var window = GetSubject(null, editedCraftingItem);

            var expectedSourceTypeItem = window.SourceTypes.FirstOrDefault(x => x.Value == editedCraftingItem.SourceType);

            expectedSourceTypeItem.ShouldNotBeNull();
            window.ItemName.ShouldBe(editedCraftingItem.Name);
            window.SelectedSourceType.ShouldBe(expectedSourceTypeItem);
            window.MaterialsList.Count().ShouldBe(editedCraftingItem.Materials.Count);
        }

        [UIFact]
        public void DefinedDataPopulated_WhenPassedToConstructor()
        {
            var window = GetSubject(TestNewItemName);

            definedCraftingMaterials.Select(x => x.Name).ShouldBeSubsetOf(window.ValidItemNames);
            definedCraftingItems.Select(x => x.Name).ShouldBeSubsetOf(window.ValidItemNames);
        }

        [UIFact]
        public void TitleIndicatesNewItem_IfWindowOpenedInNewItemMode()
        {
            var window = GetSubject(TestNewItemName);

            window.Title.ShouldContain("new", Case.Insensitive);
        }

        [UIFact]
        public void TitleIndicatesEditingItem_IfWindowOpenedInEditItemMode()
        {
            var window = GetSubject(null, editedCraftingItem);

            window.Title.ShouldContain("edit", Case.Insensitive);
        }

        [UIFact]
        public void NotificationToUser_IfAddItemClickedWithEmptyItemName()
        {
            var window = GetSubject(TestNewItemName);

            window.AddItemName = "";

            window.AddItem_Click(window, new RoutedEventArgs());

            notificationService.Received(1).ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void MaterialAddedToList_IfAddItemClickedWithPreviouslyDefinedMaterial()
        {
            var newItemName = definedCraftingItems.First().Name;

            var window = GetSubject(TestNewItemName);

            window.AddItemName = newItemName;
            window.AddItemCount = "1";

            window.AddItem_Click(window, new RoutedEventArgs());

            window.MaterialsList.Count().ShouldBe(1);
            window.MaterialsList.First().Name.ShouldBe(newItemName);
        }

        [UIFact]
        public void MaterialAddedToListWithCorrectCount_IfAddItemClickedWithGreaterThanOneCount()
        {
            var newItemName = definedCraftingItems.First().Name;
            var newItemCount = 3;

            var window = GetSubject(TestNewItemName);

            window.AddItemName = newItemName;
            window.AddItemCount = newItemCount.ToString();

            window.AddItem_Click(window, new RoutedEventArgs());

            window.MaterialsList.Count().ShouldBe(1);
            window.MaterialsList.First().Name.ShouldBe(newItemName);
            window.MaterialsList.First().Count.ShouldBe(newItemCount);
        }

        [UIFact]
        public void MaterialAddedToList_IfAddItemClickedWithDialogThatReturnsCraftingMaterial()
        {
            var materialName = "test-material-name-1";
            var materialSourceType = SourceType.Mining;
            var materialLocation = "test-material-location-1";
            var itemCount = 8;

            childWindowProvider
                .ShowChooseMaterialTypeWindow(Arg.Any<Window>())
                .Returns(ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingMaterial);

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns((materialName, materialSourceType, materialLocation));

            var window = GetSubject(TestNewItemName);

            window.AddItemName = materialName;
            window.AddItemCount = itemCount.ToString();

            window.AddItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == materialName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(materialName);
            expectedItem.Material.SourceType.ShouldBe(materialSourceType);
            expectedItem.Material.Location.ShouldBe(materialLocation);
            expectedItem.Count.ShouldBe(itemCount);
        }

        [UIFact]
        public void MaterialNotAddedToList_IfAddItemClickedWithDialogThatDoesNotReturn()
        {
            var materialName = "test-material-name-1";

            childWindowProvider
                .ShowChooseMaterialTypeWindow(Arg.Any<Window>())
                .Returns(ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingMaterial);

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns(((string MaterialName, SourceType SourceType, string Location)?)null);

            var window = GetSubject(TestNewItemName);

            window.AddItemName = materialName;

            window.AddItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == materialName);

            expectedItem.ShouldBeNull();
        }

        [UIFact]
        public void CraftingItemAddedToList_IfAddItemClickedWithDialogThatReturnsCraftingItem()
        {
            var craftingItem = BuildCraftingItem("crafting-item-name-1", SourceType.Weaving, (1, definedCraftingMaterials.First()));
            var itemCount = 9;

            childWindowProvider
                .ShowChooseMaterialTypeWindow(Arg.Any<Window>())
                .Returns(ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingItem);

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns((craftingItem.Name, craftingItem.SourceType, craftingItem.Materials));

            var window = GetSubject(TestNewItemName);

            window.AddItemName = craftingItem.Name;
            window.AddItemCount = itemCount.ToString();

            window.AddItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == craftingItem.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Material.ShouldBeOfType<CraftingItem>();
            expectedItem.Name.ShouldBe(craftingItem.Name);
            expectedItem.Material.SourceType.ShouldBe(craftingItem.SourceType);
            ((CraftingItem)expectedItem.Material).Materials.ShouldBe(craftingItem.Materials);
            expectedItem.Count.ShouldBe(itemCount);
        }

        [UIFact]
        public void RegisterFunctionCalled_IfAddItemClickedWithDialogThatReturnsCraftingMaterial()
        {
            var materialName = "test-material-name-1";
            var materialSourceType = SourceType.Mining;
            var materialLocation = "test-material-location-1";
            var itemCount = 8;

            childWindowProvider
                .ShowChooseMaterialTypeWindow(Arg.Any<Window>())
                .Returns(ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingMaterial);

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns((materialName, materialSourceType, materialLocation));

            var window = GetSubject(TestNewItemName);

            window.AddItemName = materialName;
            window.AddItemCount = itemCount.ToString();

            window.AddItem_Click(window, new RoutedEventArgs());

            registeredCraftingMaterials.ShouldContain(x => x.Name == materialName &&
                x.SourceType == materialSourceType &&
                x.Location == materialLocation);
        }

        [UIFact]
        public void RegisterFunctionCalled_IfAddItemClickedWithDialogThatReturnsCraftingItem()
        {
            var craftingItem = BuildCraftingItem("crafting-item-name-1", SourceType.Weaving, (1, definedCraftingMaterials.First()));
            var itemCount = 9;

            childWindowProvider
                .ShowChooseMaterialTypeWindow(Arg.Any<Window>())
                .Returns(ChooseMaterialTypeWindow.MaterialTypeChoice.CraftingItem);

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns((craftingItem.Name, craftingItem.SourceType, craftingItem.Materials));

            var window = GetSubject(TestNewItemName);

            window.AddItemName = craftingItem.Name;
            window.AddItemCount = itemCount.ToString();

            window.AddItem_Click(window, new RoutedEventArgs());

            registeredCraftingItems.ShouldContain(x => x.Name == craftingItem.Name);
        }

        [UIFact]
        public void MaterialCountUpdated_IfEditCountClickedWithItemSelectedAndWithDialogThatReturns()
        {
            var material = editedCraftingItem.Materials.First();
            var newItemCount = 99;

            childWindowProvider
                .ShowEditCountWindow(Arg.Any<Window>(), Arg.Any<int>())
                .Returns(newItemCount);

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(newItemCount);
        }

        [UIFact]
        public void MaterialCountNotUpdated_IfEditCountClickedWithItemSelectedAndWithDialogThatDoesNotReturn()
        {
            var material = editedCraftingItem.Materials.First();

            childWindowProvider
                .ShowEditCountWindow(Arg.Any<Window>(), Arg.Any<int>())
                .Returns((int?)null);

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(material.Count);
        }

        [UIFact]
        public void MaterialCountNotUpdated_IfEditCountClickedWithoutItemSelected()
        {
            var material = editedCraftingItem.Materials.First();

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = null;

            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(material.Count);
        }

        [UIFact]
        public void MaterialDeleted_IfDeleteItemClickedWithItemSelected()
        {
            var material = editedCraftingItem.Materials.First();

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            window.DeleteMaterial_Click(window, new RoutedEventArgs());

            var checkForItem = window.MaterialsList.FirstOrDefault(x => x.Name == material.Name);

            checkForItem.ShouldBeNull();
        }

        [UIFact]
        public void MaterialNotDeleted_IfDelteItemClickedWithoutItemSelected()
        {
            var materials = editedCraftingItem.Materials;

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = null;

            window.DeleteMaterial_Click(window, new RoutedEventArgs());

            window.MaterialsList.ShouldBe(materials);
        }

        [UIFact]
        public void DisplayNotification_IfOkClickedWhileInNewItemModeAndForAlreadyExistingItem()
        {
            var window = GetSubject(TestNewItemName);

            window.ItemName = definedCraftingItems.First().Name;

            window.Ok_Click(window, new RoutedEventArgs());

            notificationService.Received(1).ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void DoNotDisplayNotification_IfOkClickedWhileInEditMode()
        {
            var window = GetSubject(null, editedCraftingItem);

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() => { window.Ok_Click(window, new RoutedEventArgs()); });

            notificationService.DidNotReceive().ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void DoNotDisplayNotification_IfOkClickedWhileInNewItemModeForNotAlreadyExistingItem()
        {
            var window = GetSubject(TestNewItemName);

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() => { window.Ok_Click(window, new RoutedEventArgs()); });

            notificationService.DidNotReceive().ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void CraftingItemUpdated_IfEditItemClickedWithCraftingItemSelectedAndDialogThatReturns()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingItem);
            var craftingItem = (CraftingItem)material.Material;
            var newItemName = craftingItem.Name + "***";
            var newSourceType = (craftingItem.SourceType == SourceType.Alchemy ? SourceType.Armorcraft : SourceType.Alchemy);
            var newMaterials = craftingItem.Materials.Skip(1).ToList();

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

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == craftingItem.Name);

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == newItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(newItemName);
            expectedItem.Material.SourceType.ShouldBe(newSourceType);
            ((CraftingItem)expectedItem.Material).Materials.ShouldBe(newMaterials);
        }

        [UIFact]
        public void CraftingItemNotUpdated_IfEditItemClickedWithCraftingItemSelectedAndDialogThatDoesNotReturn()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingItem);
            var craftingItem = ((CraftingItem)material.Material).Clone();

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns(((string ItemName, SourceType SourceType, IEnumerable<SpecifiedCraftingMaterial> Materials)?)null);

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == craftingItem.Name);

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == craftingItem.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(craftingItem.Name);
            expectedItem.Material.SourceType.ShouldBe(craftingItem.SourceType);
            ((CraftingItem)expectedItem.Material).Materials.ShouldBe(craftingItem.Materials);
        }

        [UIFact]
        public void CraftingItemNotUpdated_IfEditItemClickedWithoutItemSelected()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingItem);
            var craftingItem = ((CraftingItem)material.Material).Clone();

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = null;

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == craftingItem.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(craftingItem.Name);
            expectedItem.Material.SourceType.ShouldBe(craftingItem.SourceType);
            ((CraftingItem)expectedItem.Material).Materials.ShouldBe(craftingItem.Materials);
        }

        [UIFact]
        public void CraftingMaterialUpdated_IfEditItemClickedWithCraftingMaterialSelectedAndDialogThatReturns()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingMaterial);
            var craftingMaterial = (CraftingMaterial)material.Material;
            var newItemName = craftingMaterial.Name + "***";
            var newSourceType = (craftingMaterial.SourceType == SourceType.Botany ? SourceType.Mining : SourceType.Botany);
            var newLocation = craftingMaterial.Location + "***";

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns((newItemName, newSourceType, newLocation));

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == craftingMaterial.Name);

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == newItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(newItemName);
            expectedItem.Material.SourceType.ShouldBe(newSourceType);
            expectedItem.Material.Location.ShouldBe(newLocation);
        }

        [UIFact]
        public void CraftingMaterialNotUpdated_IfEditItemClickedWithCraftingMaterialSelectedAndDialogThatDoesNotReturn()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingMaterial);
            var craftingMaterial = (CraftingMaterial)material.Material;

            childWindowProvider
                .ShowAddEditCraftingMaterialWindow(
                    Arg.Any<Window>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingMaterial>(),
                    Arg.Any<Func<string, bool>>())
                .Returns(((string MaterialName, SourceType SourceType, string Location)?)null);

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = window.MaterialsList.FirstOrDefault(x => x.Name == craftingMaterial.Name);

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == craftingMaterial.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(craftingMaterial.Name);
            expectedItem.Material.SourceType.ShouldBe(craftingMaterial.SourceType);
            expectedItem.Material.Location.ShouldBe(craftingMaterial.Location);
        }

        [UIFact]
        public void CraftingMaterialNotUpdated_IfEditItemClickedWithoutItemSelected()
        {
            var material = editedCraftingItem.Materials.First(x => x.Material is CraftingMaterial);
            var craftingMaterial = (CraftingMaterial)material.Material;

            var window = GetSubject(null, editedCraftingItem);

            window.SelectedMaterial = null;

            window.Edit_Click(window, new RoutedEventArgs());

            var expectedItem = window.MaterialsList.FirstOrDefault(x => x.Name == craftingMaterial.Name);

            expectedItem.ShouldNotBeNull();
            expectedItem.Name.ShouldBe(craftingMaterial.Name);
            expectedItem.Material.SourceType.ShouldBe(craftingMaterial.SourceType);
            expectedItem.Material.Location.ShouldBe(craftingMaterial.Location);
        }
    }
}
