using CommunityToolkit.Mvvm.ComponentModel;
using Foodify10.Services.Interfaces;
using System.Text;
using System.Net;

namespace Foodify10.ViewModels
{
    public partial class ArticlePageViewModel : ObservableObject
    {
        private readonly IRoskachestvoService _roskachestvoService;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private string htmlContent = string.Empty;

        [ObservableProperty]
        private string loadingText = "Загружаем статью...";

        public bool IsContentVisible => !IsLoading;

        private int? _loadedArticleId;

        public ArticlePageViewModel(IRoskachestvoService roskachestvoService)
        {
            _roskachestvoService = roskachestvoService;
        }

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsContentVisible));
        }

        public async Task LoadAsync(int articleId)
        {
            if (_loadedArticleId == articleId && !string.IsNullOrWhiteSpace(HtmlContent))
                return;

            IsLoading = true;
            _loadedArticleId = articleId;

            try
            {
                var detail = await _roskachestvoService.GetArticleDetailAsync(articleId);

                if (detail == null)
                {
                    HtmlContent = BuildHtml("Статья не найдена", "", "<p>Не удалось загрузить статью.</p>");
                    return;
                }

                var bodyHtml = new StringBuilder();

                if (detail.Content_Blocks != null)
                {
                    foreach (var block in detail.Content_Blocks)
                    {
                        if (string.IsNullOrWhiteSpace(block?.Content))
                            continue;

                        switch (block.Type?.ToLowerInvariant())
                        {
                            case "header":
                                bodyHtml.Append($"<h2>{block.Content}</h2>");
                                break;

                            case "image":
                                bodyHtml.Append($@"<img src=""{block.Content}"" alt=""image"" loading=""lazy"" />");
                                break;

                            case "paragraph":
                            default:
                                bodyHtml.Append(block.Content);
                                break;
                        }
                    }
                }

                HtmlContent = BuildHtml(
                    detail.Title ?? "Статья",
                    detail.Thumbnail ?? string.Empty,
                    bodyHtml.ToString());
            }
            catch (Exception ex)
            {
                HtmlContent = BuildHtml(
                    "Ошибка",
                    "",
                    $"<p>{WebUtility.HtmlEncode(ex.Message)}</p>");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static string BuildHtml(string title, string imageUrl, string body)
        {
            var imageBlock = string.IsNullOrWhiteSpace(imageUrl)
                ? ""
                : $@"<img class=""hero"" src=""{imageUrl}"" alt=""article image"" />";

            return $@"
<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, viewport-fit=cover"" />
    <style>
        html, body {{
            margin: 0;
            padding: 0;
            background: #ffffff;
            color: #1f2937;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
            font-size: 16px;
            line-height: 1.65;
            word-wrap: break-word;
            overflow-wrap: break-word;
        }}

        .page {{
            padding: 20px 20px 28px 20px;
        }}

        .hero {{
            width: 100%;
            height: auto;
            display: block;
            border-radius: 18px;
            margin: 0 0 18px 0;
            object-fit: cover;
        }}

        h1 {{
            margin: 0 0 18px 0;
            color: #15803d;
            font-size: 28px;
            line-height: 1.25;
        }}

        h2, h3 {{
            color: #15803d;
            margin: 22px 0 12px 0;
            line-height: 1.3;
        }}

        p {{
            margin: 0 0 14px 0;
        }}

        img {{
            max-width: 100%;
            height: auto;
            display: block;
            border-radius: 14px;
            margin: 14px 0;
        }}

        ul, ol {{
            padding-left: 22px;
            margin: 0 0 14px 0;
        }}

        li {{
            margin-bottom: 6px;
        }}

        table {{
            width: 100%;
            border-collapse: collapse;
            display: block;
            overflow-x: auto;
            white-space: nowrap;
            margin-bottom: 14px;
        }}

        td, th {{
            border: 1px solid #e5e7eb;
            padding: 8px;
        }}

        a {{
            color: #16a34a;
            text-decoration: none;
        }}

        blockquote {{
            margin: 14px 0;
            padding: 12px 16px;
            background: #f9fafb;
            border-left: 4px solid #22c55e;
            border-radius: 8px;
        }}
    </style>
</head>
<body>
    <div class=""page"">
        {imageBlock}
        <h1>{WebUtility.HtmlEncode(title)}</h1>
        {body}
    </div>
</body>
</html>";
        }
    }
}