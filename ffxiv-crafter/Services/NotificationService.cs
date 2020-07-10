using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ffxiv_crafter.Services
{
    public interface INotificationService
    {
        void ShowMessage(string message);
        bool ShowConfirmation(string message, string title);
    }

    public class NotificationService : INotificationService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        public bool ShowConfirmation(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
        }
    }
}
