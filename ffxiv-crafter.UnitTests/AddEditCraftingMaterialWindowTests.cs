using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using NSubstitute;
using ffxiv_crafter.Services;
using ffxiv_crafter.Models;
using System.Printing;
using System.Security.Principal;
using System.Linq;
using System.Windows;

namespace ffxiv_crafter.UnitTests
{
    public class AddEditCraftingMaterialWindowTests
    {
        private const string TestCraftingMaterialName = "test-crafting-material-name-1";
        private const string TestNewMaterialName = "test-new-material-name";
        private const string TestLocationName = "test-location-name-1";
        private const string TestAlreadyUsedMaterialName = "test-already-used-material-name-1";

        private readonly INotificationService notificationService = Substitute.For<INotificationService>();
        private readonly Func<string, bool> existingNamePredicate;

        public AddEditCraftingMaterialWindowTests()
        {
            existingNamePredicate = name => StringComparer.OrdinalIgnoreCase.Equals(name, TestAlreadyUsedMaterialName);
        }

        private AddEditCraftingMaterialWindow GetSubject(string suggestedName, CraftingMaterial editMaterial = null)
        {
            return new AddEditCraftingMaterialWindow(
                notificationService,
                suggestedName,
                editMaterial,
                existingNamePredicate);
        }

        private CraftingMaterial BuildCraftingMaterial(string name, SourceType sourceType, string location)
        {
            return new CraftingMaterial { Name = name, SourceType = sourceType, Location = location };
        }

        [UIFact]
        public void DataDisplayed_IfWindowOpenedInEditItemMode()
        {
            var material = BuildCraftingMaterial(TestCraftingMaterialName, SourceType.Botany, TestLocationName);

            var window = GetSubject(null, material);

            window.MaterialName.ShouldBe(material.Name);
            window.SourceType.ShouldBe(material.SourceType);
            window.Location.ShouldBe(material.Location);
        }

        [UIFact]
        public void NoDataDisplayed_IfWindowOpenedInNewItemMode()
        {
            var window = GetSubject(TestNewMaterialName);

            window.MaterialName.ShouldBe(TestNewMaterialName);
            window.SourceType.ShouldBe(window.SourceTypes.First().Value);
            window.Location.ShouldBeEmpty();
        }

        [UIFact]
        public void TitleIndicatesEditingItem_IfWindowOpenedInEditItemMode()
        {
            var material = BuildCraftingMaterial(TestCraftingMaterialName, SourceType.Botany, TestLocationName);

            var window = GetSubject(null, material);

            window.Title.ShouldContain("edit", Case.Insensitive);
        }

        [UIFact]
        public void TitleIndicatesNewItem_IfWindowOpenedInNewItemMode()
        {
            var window = GetSubject(TestNewMaterialName);

            window.Title.ShouldContain("new", Case.Insensitive);
        }

        [UIFact]
        public void DisplayNotification_IfOkClickedWithEmptyName()
        {
            var window = GetSubject(null);

            window.MaterialName = "";

            window.Ok_Click(window, new RoutedEventArgs());

            notificationService.Received(1).ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void DisplayNotification_IfOkClickedWithAlreadyUsedName()
        {
            var window = GetSubject(null);

            window.MaterialName = TestAlreadyUsedMaterialName;

            window.Ok_Click(window, new RoutedEventArgs());

            notificationService.Received(1).ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void DoNotDisplayNotification_IfOkClickedWithNonEmptyName()
        {
            var window = GetSubject(null);

            window.MaterialName = TestNewMaterialName;

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() => { window.Ok_Click(window, new RoutedEventArgs()); });

            notificationService.DidNotReceive().ShowMessage(Arg.Any<string>());
        }

        [UIFact]
        public void DoNotDisplayNotification_IfOkClickedWithNotAlreadyUsedName()
        {
            var window = GetSubject(null);

            window.MaterialName = TestNewMaterialName;

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() => { window.Ok_Click(window, new RoutedEventArgs()); });

            notificationService.DidNotReceive().ShowMessage(Arg.Any<string>());
        }
    }
}
