using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ffxiv_crafter.Services
{
    public interface INotificationService
    {
        void ShowMessage(string message);
    }

    public class NotificationService : INotificationService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
