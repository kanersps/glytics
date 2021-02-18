using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Data.Persistence;

namespace glytics.Logic.Account
{
    public class GetAccount
    {
        private Authentication _authentication;
        
        public GetAccount(Authentication authentication)
        {
            _authentication = authentication;
        }

        public async Task<APIKey> Verify(string key)
        {
            return await _authentication.Authorized(key);
        }
    }
}