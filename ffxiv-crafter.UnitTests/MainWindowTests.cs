using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using NSubstitute;
using ffxiv_crafter.Models;
using ffxiv_crafter.Services;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Security.RightsManagement;

namespace ffxiv_crafter.UnitTests
{
    public class MainWindowTests
    {
        private const string TestCraftingItemName = "test-crafting-item-1";

        private readonly List<CraftingItem> craftingItems = new List<CraftingItem>();
        private readonly List<CraftingMaterial> craftingMaterials = new List<CraftingMaterial>();
        private readonly IInitialDataService initialDataService = Substitute.For<IInitialDataService>();
        private readonly IChildWindowProvider childWindowProvider = Substitute.For<IChildWindowProvider>();
        private readonly IFileSystemService fileSystemService = Substitute.For<IFileSystemService>();
        private readonly INotificationService notificationService = Substitute.For<INotificationService>();
        private readonly ICraftingMaterial craftingMaterial;

        public MainWindowTests()
        {
            craftingMaterial = new CraftingMaterial { Name = "test-crafting-material-1", SourceType = SourceType.Mining, Location = "location-1" };
            craftingMaterials.Add(craftingMaterial as CraftingMaterial);

            craftingItems.Add(BuildCraftingItem(TestCraftingItemName));

            initialDataService.GetCraftingItems().Returns(craftingItems);
            initialDataService.GetCraftingMaterials().Returns(craftingMaterials);
        }

        private SpecifiedCraftingMaterial BuildSpecifiedCraftingMaterial(int count, ICraftingMaterial material)
        {
            return new SpecifiedCraftingMaterial { Material = material, Count = count };
        }

        private CraftingItem BuildCraftingItem(string name, SourceType sourceType = SourceType.Blacksmith)
        {
            var item = new CraftingItem { Name = name, SourceType = sourceType };

            item.SetMaterials(BuildSpecifiedCraftingMaterial(2, craftingMaterial).ToSingleton());

            return item;
        }

        private string GetTestJson()
        {
            return @"
            {
                ""AllCraftingItems"": [
                    {
                        ""Id"": ""bfed2dc7-896e-4b3e-9fdf-c393dbdbeaad"",
                        ""Name"": ""bronze ingot"",
                        ""SourceType"": 10,
                        ""Material"": [
                            {
                                ""ItemId"": ""242770af-dde8-4281-909b-c1d959991e6d"",
                                ""Count"": 2,
                                ""IsMaterial"": true
                            }
                        ]
                    }
                ],
                ""AllCraftingMaterials"": [
                    {
                        ""Id"": ""242770af-dde8-4281-909b-c1d959991e6d"",
                        ""Name"": ""adamantoise shell"",
                        ""SourceType"": 2,
                        ""Location"": ""northern thanalan (bluefog)""
                    }
                ],
                ""ListedItems"": [
                    {
                        ""ItemId"": ""bfed2dc7-896e-4b3e-9fdf-c393dbdbeaad"",
                        ""Count"": 1,
                        ""IsMaterial"": false
                    },
                ]
            }";
        }

        private MainWindow GetSubject()
        {
            return new MainWindow(initialDataService, childWindowProvider, fileSystemService, notificationService);
        }

        [UIFact]
        public void CraftingItemAddedToList_IfAddItemClickWithPreviouslyDefinedItemNameEntered()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = "1";

            window.AddItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe(TestCraftingItemName);
            window.CraftingItems.First().Count.ShouldBe(1);
        }

        [UIFact]
        public void CraftingItemAddedToList_IfAddItemClickWithEmptyStringForCountEntered()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = "";

            window.AddItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe(TestCraftingItemName);
            window.CraftingItems.First().Count.ShouldBe(1);
        }

        [UIFact]
        public void CraftingItemAddedToListWithCount_IfAddItemClickWithCountHigherThanOne()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = "2";

            window.AddItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe(TestCraftingItemName);
            window.CraftingItems.First().Count.ShouldBe(2);
        }

        [UIFact]
        public void CraftingItemAddedToList_IfAddItemClickedWithDialogThatReturns()
        {
            var newItemName = TestCraftingItemName + "2";

            childWindowProvider
                .ShowAddEditCraftingItemWindow(
                    Arg.Any<Window>(),
                    Arg.Any<IEnumerable<CraftingMaterial>>(),
                    Arg.Any<IEnumerable<CraftingItem>>(),
                    Arg.Any<Action<CraftingMaterial>>(),
                    Arg.Any<Action<CraftingItem>>(),
                    Arg.Any<string>(),
                    Arg.Any<CraftingItem>())
                .Returns((newItemName, SourceType.Armorcraft, Enumerable.Empty<SpecifiedCraftingMaterial>()));

            var window = GetSubject();

            window.ItemName = newItemName;

            window.AddItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe(newItemName);
            window.CraftingItems.First().Count.ShouldBe(1);
        }

        [UIFact]
        public void CraftingItemNotAddedToList_IfAddItemClickedWithDialogThatDoesNotReturn()
        {
            var newItemName = TestCraftingItemName + "2";

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

            var window = GetSubject();

            window.ItemName = newItemName;

            window.AddItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(0);
        }

        [UIFact]
        public void NotificationToUser_IfAddItemClickedWithEmptyItemName()
        {
            var window = GetSubject();

            window.ItemName = "";

            window.AddItem_Click(window, new RoutedEventArgs());

            notificationService.Received(1).ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void CraftingMaterialsUpdated_IfConfigureMaterialsClickedAndReturns()
        {
            var newMaterials = new List<CraftingMaterial>
            {
                new CraftingMaterial { Name = "test-crafting-material-1", SourceType = SourceType.Mining, Location = "location-1" },
                new CraftingMaterial { Name = "test-crafting-material-2", SourceType = SourceType.Mining, Location = "location-2" },
                new CraftingMaterial { Name = "test-crafting-material-3", SourceType = SourceType.Mining, Location = "location-3" }
            };

            childWindowProvider
                .ShowConfigureMaterialsWindow(Arg.Any<Window>(), Arg.Any<List<CraftingMaterial>>())
                .Returns(newMaterials);

            var window = GetSubject();

            window.ConfigureItems_Click(window, new RoutedEventArgs());

            var expectedValidItemNames = newMaterials.Select(x => x.Name);

            window.MaterialItemNames.ShouldBe(expectedValidItemNames);
        }

        [UIFact]
        public void CraftingMaterialsNotUpdated_IfConfigureMaterialsClickedButCancelled()
        {
            childWindowProvider
                .ShowConfigureMaterialsWindow(Arg.Any<Window>(), Arg.Any<List<CraftingMaterial>>())
                .Returns((IEnumerable<CraftingMaterial>)null);

            var window = GetSubject();

            var expectedValidItemNames = window.MaterialItemNames.ToList();

            window.ConfigureItems_Click(window, new RoutedEventArgs());

            window.MaterialItemNames.ShouldBe(expectedValidItemNames);
        }

        [UIFact]
        public void CraftingItemsUpdated_IfDeleteItemClickedWhileItemSelected()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = window.CraftingItems.First();
            window.DeleteItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(0);
        }

        [UIFact]
        public void CraftingItemsNotUpdated_IfDeleteItemClickedWithoutItemSelected()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = null;
            window.DeleteItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
        }

        [UIFact]
        public void CraftingItemsUpdated_IfEditItemClickedWithItemSelectedAndReturns()
        {
            var newItemName = TestCraftingItemName + "2";
            var newSourceType = SourceType.Goldsmith;
            var newMaterials = Enumerable.Empty<SpecifiedCraftingMaterial>();

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

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = window.CraftingItems.First();
            window.EditItem_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => x.Name == newItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.SourceType.ShouldBe(newSourceType);
            expectedItem.Materials.ShouldBe(newMaterials);
        }

        [UIFact]
        public void CraftingItemsNotUpdated_IfEditItemClickedWithItemSelectedAndDoesNotReturn()
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
            .Returns(((string ItemName, SourceType SourceType, IEnumerable<SpecifiedCraftingMaterial> Materials)?)null);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            var expectedCraftingItems = window.CraftingItems.ToList();

            window.SelectedCraftingItem = window.CraftingItems.First();
            window.EditItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(expectedCraftingItems);
        }

        [UIFact]
        public void CraftingItemsNotUpdated_IfEditItemClickedWithoutItemSelected()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            var expectedCraftingItems = window.CraftingItems.ToList();

            window.SelectedCraftingItem = null;
            window.EditItem_Click(window, new RoutedEventArgs());

            window.CraftingItems.ShouldBe(expectedCraftingItems);
        }

        [UIFact]
        public void CraftingItemsUpdate_IfEditCountClickedWithItemSelectedAndReturns()
        {
            var initialCount = 1;

            childWindowProvider
                .ShowEditCountWindow(Arg.Any<Window>(), Arg.Any<int>())
                .Returns(initialCount + 1);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = initialCount.ToString();
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = window.CraftingItems.First();
            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => x.Name == TestCraftingItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(initialCount + 1);
        }

        [UIFact]
        public void CraftingItemsNotUpdated_IfEditCountClickedWithItemSelectedAndDoesNotReturn()
        {
            var initialCount = 8;

            childWindowProvider
                .ShowEditCountWindow(Arg.Any<Window>(), Arg.Any<int>())
                .Returns((int?)null);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = initialCount.ToString();
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = window.CraftingItems.First();
            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => x.Name == TestCraftingItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(initialCount);
        }

        [UIFact]
        public void CraftingItemsNotUpdated_IfEditCountClickedWithoutItemSelected()
        {
            var initialCount = 8;

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.ItemCount = initialCount.ToString();
            window.AddItem_Click(window, new RoutedEventArgs());

            window.SelectedCraftingItem = null;
            window.EditCount_Click(window, new RoutedEventArgs());

            var expectedItem = window.CraftingItems.FirstOrDefault(x => x.Name == TestCraftingItemName);

            expectedItem.ShouldNotBeNull();
            expectedItem.Count.ShouldBe(initialCount);
        }

        [UIFact]
        public void DialogDisplayed_IfGenerateListClicked()
        {
            var window = GetSubject();

            window.GenerateList_Click(window, new RoutedEventArgs());

            childWindowProvider.Received(1).ShowGenerateListWindow(Arg.Any<Window>(), Arg.Any<List<SpecifiedCraftingItem>>());
        }

        [UIFact]
        public void FilenameIsQueried_IfLoadClicked()
        {
            var json = GetTestJson();

            fileSystemService.ReadAllText(Arg.Any<string>()).Returns(json);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Load_Click(window, new RoutedEventArgs());

            fileSystemService.Received(1).GetOpenFilename();
        }

        [UIFact]
        public void ContentsReadFromFile_IfLoadClickedAndFilenameGiven()
        {
            var json = GetTestJson();

            fileSystemService.ReadAllText(Arg.Any<string>()).Returns(json);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Load_Click(window, new RoutedEventArgs());

            fileSystemService.Received(1).ReadAllText(Arg.Any<string>());
        }

        [UIFact]
        public void CraftingDataUpdated_IfLoadClickedAndFilenameGiven()
        {
            var json = GetTestJson();

            fileSystemService.ReadAllText(Arg.Any<string>()).Returns(json);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Load_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe("bronze ingot");
            window.MaterialItemNames.Count().ShouldBe(1);
            window.MaterialItemNames.First().ShouldBe("adamantoise shell");
            window.ValidItemNames.Count().ShouldBe(1);
            window.ValidItemNames.First().ShouldBe("bronze ingot");
        }

        [UIFact]
        public void CraftingDataNotUpdated_IfLoadClickedAndFilenameNotGiven()
        {
            fileSystemService.GetOpenFilename().Returns((string)null);

            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Load_Click(window, new RoutedEventArgs());

            window.CraftingItems.Count().ShouldBe(1);
            window.CraftingItems.First().Name.ShouldBe(TestCraftingItemName);
        }

        [UIFact]
        public void FilenameIsQueried_IfSaveClicked()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Save_Click(window, new RoutedEventArgs());

            fileSystemService.Received(1).GetSaveFilename();
        }

        [UIFact]
        public void ContentsWrittenToFile_IfSaveClickedAndFilenameGiven()
        {
            var window = GetSubject();

            window.ItemName = TestCraftingItemName;
            window.AddItem_Click(window, new RoutedEventArgs());

            window.Save_Click(window, new RoutedEventArgs());

            fileSystemService.Received(1).WriteAllText(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
