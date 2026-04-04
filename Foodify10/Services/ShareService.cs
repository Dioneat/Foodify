using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class ShareService : IShareService
    {
        public Task ShareTextAsync(string title, string text)
        {
            return Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = title,
                Text = text
            });
        }
    }
}