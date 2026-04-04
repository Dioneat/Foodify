using System;
using System.Collections.Generic;
using System.Text;

namespace Foodify10.Models
{

    public class ArticleListResponse { public ArticleListContent Response { get; set; } }
    public class ArticleListContent { public List<RoskachestvoArticle> Articles { get; set; } }

    public class RoskachestvoArticle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public RoskachestvoSection Section { get; set; }
    }

    public class RoskachestvoSection { public string Title { get; set; } }

    // Деталка статьи
    public class ArticleDetailResponse { public RoskachestvoArticleDetail Response { get; set; } }

    public class RoskachestvoArticleDetail
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public List<ContentBlock> Content_Blocks { get; set; }
    }

    public class ContentBlock
    {
        public string Type { get; set; } // "paragraph", "image", "header"
        public string Content { get; set; }
    }
}
