using Foodify10.Models;

namespace Foodify10.Services.Interfaces
{
    public interface IComparisonNavigationService
    {
        Task OpenComparisonDetailsAsync(ComparisonGroup group);
    }
}