using Foodify10.Models.JsonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodify10.Models
{
    public class ProductFlowResult
    {
       
        public ProductData Data { get; set; }

        
        public CompositionExplanation Explanation { get; set; }

        
        public ReviewData? Reviews { get; set; }

        public ProductFlowResult(ProductData data, CompositionExplanation explanation, ReviewData? reviews)
        {
            Data = data;
            Explanation = explanation;
            Reviews = reviews;
        }
    }
}
