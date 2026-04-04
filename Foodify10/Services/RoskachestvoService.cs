using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Net.Http.Json;
using static Foodify10.Models.JsonModels.RskrfModels;

namespace Foodify10.Services
{
    public class RoskachestvoService : IRoskachestvoService
    {
        private readonly HttpClient _httpClient;

        public RoskachestvoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<QualityProduct>> GetQualityProductsAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<QualityProductResponse>("products/quality/");
                return result?.Response?.Take(5).ToList() ?? new List<QualityProduct>();
            }
            catch
            {
                return new List<QualityProduct>();
            }
        }

        public async Task<List<RoskachestvoArticle>> GetTipsArticlesAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<ArticleListResponse>("article/tips/");
                return result?.Response?.Articles ?? new List<RoskachestvoArticle>();
            }
            catch
            {
                return new List<RoskachestvoArticle>();
            }
        }

        public async Task<List<RoskachestvoArticle>> GetTipsArticlesForMainPageAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<ArticleListResponse>("article/tips/");
                return result?.Response?.Articles?.Take(5).ToList() ?? new List<RoskachestvoArticle>();
            }
            catch
            {
                return new List<RoskachestvoArticle>();
            }
        }

        public async Task<RoskachestvoArticleDetail?> GetArticleDetailAsync(int id)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<ArticleDetailResponse>($"article/{id}/");
                return result?.Response;
            }
            catch
            {
                return null;
            }
        }
    }
}