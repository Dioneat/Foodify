using Foodify10.Models;
using static Foodify10.Models.JsonModels.RskrfModels;

namespace Foodify10.Services.Interfaces
{
    public interface IRoskachestvoService
    {
        Task<List<QualityProduct>> GetQualityProductsAsync();
        Task<List<RoskachestvoArticle>> GetTipsArticlesAsync();
        Task<List<RoskachestvoArticle>> GetTipsArticlesForMainPageAsync();
        Task<RoskachestvoArticleDetail?> GetArticleDetailAsync(int id);
    }
}