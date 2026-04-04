using Foodify10.Models;
using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class ComparisonNavigationService : IComparisonNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ComparisonNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OpenComparisonDetailsAsync(ComparisonGroup group)
        {
            var page = _serviceProvider.GetRequiredService<ComparisonDetailsPage>();
            page.Initialize(group);
            await Shell.Current.Navigation.PushAsync(page);
        }
    }
}