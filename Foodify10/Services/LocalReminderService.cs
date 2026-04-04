using Foodify10.Services.Interfaces;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace Foodify10.Services
{
    public class LocalReminderService : ILocalReminderService
    {
        public async Task<bool> EnsureNotificationsEnabledAsync()
        {
            var areEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();

            if (!areEnabled)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
                areEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            }

            return areEnabled;
        }

        public async Task ScheduleReminderAsync(int id, string title, string description, DateTime notifyTime)
        {
            var notification = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = description,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };

            await LocalNotificationCenter.Current.Show(notification);
        }
    }
}