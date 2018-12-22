using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Widget;
using RollingOutTools.Storage;
using S2A.AppPart.AppEnvironment;
using System.Threading.Tasks;

namespace ItRollingOut.Xamarin.Droid
{
    /// <summary>
    /// Главное требование к метода в этом классе - они должны быть доступны в любой момент.
    /// Если у вас есть код, который только на конкретной странице выведет сообщение, то его не стоит писать сюда.
    /// </summary>
    public class MessagesService:IMessagesService
    {
        /// <summary>
        /// Уведомление в статус баре.
        /// </summary>
        public void ShowNotification(string msg, string title = null, bool soundEnabled = true)
        {
            _ShowNotification(msg, title, soundEnabled);

        }

        static  async Task _ShowNotification(string msg, string title = null, bool soundEnabled = true)
        {
            var ctx = Application.Context;

            title = title ?? CustomEnv.Current.AppName;
            NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(ctx);
            mBuilder
               .SetDefaults(NotificationCompat.DefaultAll)
               .SetAutoCancel(true)
               .SetContentTitle(title)
               .SetContentText(msg)
               .SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 })
               .SetSmallIcon(
                    AndroidResourceHelper.GetId("Mipmap.ic_launcher")
               );

            if (soundEnabled)
            {
                mBuilder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
            }

            Intent resultIntent = new Intent(ctx, CustomEnv.Current.MainActivityType);
            var stackBuilder = Android.App.TaskStackBuilder.Create(ctx);
            stackBuilder.AddNextIntent(resultIntent);
            PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(
                0,
                PendingIntentFlags.UpdateCurrent
            );
            mBuilder.SetContentIntent(resultPendingIntent);

            NotificationManager mNotificationManager =
                (NotificationManager)(ctx.GetSystemService(Context.NotificationService));
            //int notificationNum = new Random().Next();
            int notificationNum = await StorageHardDrive.Get<int>("notification_number");
            StorageHardDrive.Set("notification_number", ++notificationNum);
            mNotificationManager.Notify(notificationNum, mBuilder.Build());

        }

        /// <summary>
        /// Очищает все уведомления в статус баре
        /// </summary>
        public void ClearNotifications()
        {
            NotificationManager mNotificationManager =
                (NotificationManager)(Application.Context.GetSystemService(Context.NotificationService));
            mNotificationManager.CancelAll();

        }

        /// <summary>
        /// Всплывающее сообщение.
        /// </summary>
        public void ShowToast(string msg)
        {
            Srv.GuiContext.Invoke(() =>
            {
                Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Long).Show();
            });
        }
    }
}