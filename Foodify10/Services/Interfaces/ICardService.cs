using Foodify10.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodify10.Services.Interfaces
{
    public interface ICardService
    {
        Task<List<LoyaltyCard>> GetCardsAsync();
        Task SaveCardAsync(LoyaltyCard card);
        Task DeleteCardAsync(Guid id);
        Task<string> SaveCardImageAsync(FileResult photoResult); 
    }
}
