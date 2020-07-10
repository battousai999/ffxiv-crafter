using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NSubstitute;
using Shouldly;
using System.Windows.Input;
using static ffxiv_crafter.ChooseMaterialTypeWindow;
using System.Windows;

namespace ffxiv_crafter.UnitTests
{
    public class ChooseMaterialTypeWindowTests
    {
        private readonly PresentationSource inputSource = Substitute.For<PresentationSource>();

        [UIFact]
        public void ReturnsCraftingMaterialType_IfMKeyIsPressed()
        {
            var window = new ChooseMaterialTypeWindow();

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() =>
            {
                window.ChooseMaterialTypeWindow_KeyUp(window, new KeyEventArgs(Keyboard.PrimaryDevice, inputSource, 0, Key.M));
            });

            window.MaterialType.ShouldBe(MaterialTypeChoice.CraftingMaterial);
        }

        [UIFact]
        public void ReturnsCraftingItemType_IfIKeyIsPressed()
        {
            var window = new ChooseMaterialTypeWindow();

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() =>
            {
                window.ChooseMaterialTypeWindow_KeyUp(window, new KeyEventArgs(Keyboard.PrimaryDevice, inputSource, 0, Key.I));
            });

            window.MaterialType.ShouldBe(MaterialTypeChoice.CraftingItem);
        }

        [UIFact]
        public void ReturnsCraftingMaterialType_IfMaterialButtonClicked()
        {
            var window = new ChooseMaterialTypeWindow();

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() =>
            {
                window.CraftingMaterial_Click(window, new RoutedEventArgs());
            });

            window.MaterialType.ShouldBe(MaterialTypeChoice.CraftingMaterial);
        }

        [UIFact]
        public void ReturnsCraftingItemType_IfItemButtonClicked()
        {
            var window = new ChooseMaterialTypeWindow();

            // Since the OK button sets the DialogResult and these tests never call window.ShowDialog() (since
            // that call blocks), an exception is expected here.
            Should.Throw<InvalidOperationException>(() =>
            {
                window.CraftingItem_Click(window, new RoutedEventArgs());
            });

            window.MaterialType.ShouldBe(MaterialTypeChoice.CraftingItem);
        }
    }
}
