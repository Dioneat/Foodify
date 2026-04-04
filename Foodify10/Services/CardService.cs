using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Foodify10.Services
{
    public class CardService : ICardService
    {
        private const string CardsKey = "user_loyalty_cards";

        public async Task<List<LoyaltyCard>> GetCardsAsync()
        {
            string json = Preferences.Default.Get(CardsKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return new List<LoyaltyCard>();
            return JsonSerializer.Deserialize<List<LoyaltyCard>>(json) ?? new List<LoyaltyCard>();
        }

        public async Task SaveCardAsync(LoyaltyCard card)
        {
            var cards = await GetCardsAsync();
            cards.Add(card);
            string json = JsonSerializer.Serialize(cards);
            Preferences.Default.Set(CardsKey, json);
        }

        public async Task DeleteCardAsync(Guid id)
        {
            var cards = await GetCardsAsync();
            var cardToRemove = cards.FirstOrDefault(x => x.Id == id);
            if (cardToRemove != null)
            {
                
                if (!string.IsNullOrEmpty(cardToRemove.ImagePath) && File.Exists(cardToRemove.ImagePath))
                {
                    File.Delete(cardToRemove.ImagePath);
                }
                cards.Remove(cardToRemove);
                string json = JsonSerializer.Serialize(cards);
                Preferences.Default.Set(CardsKey, json);
            }
        }

        public async Task<string> SaveCardImageAsync(FileResult photoResult)
        {
            if (photoResult == null) return null;

           
            string targetDir = Path.Combine(FileSystem.AppDataDirectory, "cards");
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoResult.FileName)}";
            string targetPath = Path.Combine(targetDir, fileName);

            
            using (var stream = await photoResult.OpenReadAsync())
            using (var newStream = File.OpenWrite(targetPath))
            {
                await stream.CopyToAsync(newStream);
            }

            return targetPath;
        }
    }
}
