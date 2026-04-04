namespace Foodify10.Services.Interfaces
{
    public interface ILocalReminderService
    {
        Task<bool> EnsureNotificationsEnabledAsync();
        Task ScheduleReminderAsync(int id, string title, string description, DateTime notifyTime);
    }
}