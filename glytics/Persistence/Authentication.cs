using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using glytics.Models;
using Microsoft.EntityFrameworkCore;

namespace glytics.Persistence
{
    public class Authentication
    {
        private readonly GlyticsDbContext _db;
        
        public Authentication(GlyticsDbContext context)
        {
            _db = context;
        }
        
        public async Task<Guid> Get(Guid ownerId, string description = "")
        {
            Account account = await _db.Account.Include(a => a.ApiKeys).FirstOrDefaultAsync(a => a.Id == ownerId);

            if (account.ApiKeys.Count == 0)
            {
                APIKey newKey = new APIKey()
                {
                    Description = description
                };
                
                account.ApiKeys.Add(newKey);

                await _db.SaveChangesAsync();
            }
            
            return account.ApiKeys[0].Key;
        }

        public async Task<APIKey> Authorized(string _key)
        {
            return await _db.ApiKey.Include(key => key.Account).ThenInclude(acc => acc.Applications).FirstOrDefaultAsync(key => key.Key.ToString() == _key);
        }
    }
}