using Foodify10.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foodify10.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;

        async Task Init()
        {
            if (_db != null) return;
            var path = Path.Combine(FileSystem.AppDataDirectory, "ShoppingDb.db3");
            _db = new SQLiteAsyncConnection(path);
            await _db.CreateTableAsync<ShoppingListModel>();
        }

        public async Task SaveList(ShoppingListModel list)
        {
            await Init();
            if (await _db.FindAsync<ShoppingListModel>(list.Id) != null)
                await _db.UpdateAsync(list);
            else
                await _db.InsertAsync(list);
        }

        public async Task<List<ShoppingListModel>> GetLists()
        {
            await Init();
            return await _db.Table<ShoppingListModel>().ToListAsync();
        }
    }
}
