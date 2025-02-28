using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications;
using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using System.Windows;

namespace DocStack.Utils
{
    public static class Toaster
    {
        static Notifier notifier;
        static Toaster()
        {
            notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });

        }
        public static void ShowInformation(string msg) => notifier.ShowInformation(msg);
        public static void ShowSuccess(string msg) => notifier.ShowSuccess(msg);
        public static void ShowError(string msg) => notifier.ShowError(msg);
        public static void ShowWarning(string msg) => notifier.ShowWarning(msg);
    }
}
