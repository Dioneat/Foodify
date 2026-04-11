using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class AlertService : IAlertService
    {
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            return Shell.Current.DisplayAlertAsync(title, message, cancel);
        }
        public Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                return Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
            }

            return Task.FromResult(false);
        }
    }
}