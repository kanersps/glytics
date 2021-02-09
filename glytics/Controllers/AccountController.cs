using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using glytics.Models;
using glytics.Persistence;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace glytics.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        private readonly Authentication _apiHandler;
        public AccountController(GlyticsDbContext dbContext, Authentication apiHandler)
        {
            _db = dbContext;
            _apiHandler = apiHandler;
        }

        [HttpGet("account")]
        public async Task<ActionResult<LoginAccount>> Account()
        {
            APIKey key = null;
            
            if (!string.IsNullOrEmpty(Request.Headers["key"]))
            {
                key = await _apiHandler.Authorized(Request.Headers["key"]);
            }

            if (key == null)
            {
                return new UnauthorizedResult();
            }

            return new LoginAccount()
            {
                Username = key.Account.Username,
                Password = key.Account.Password,
            };
        }
        
        [HttpGet("account/authenticated")]
        public async Task<ActionResult<AccountMessage>> AccountAuthenticated()
        {
            APIKey key = null;
            
            if (!string.IsNullOrEmpty(Request.Headers["key"]))
            {
                key = await _apiHandler.Authorized(Request.Headers["key"]);
            }

            if (key == null)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "This key is not authenticated"
                };
            }

            return new AccountMessage()
            {
                Success = true,
                Message = ""
            };
        }
        
        [HttpPost("account/login")]
        public async Task<ActionResult<AccountMessage>> AccountLogin(LoginAccount _account)
        {
            Account account = await _db.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);

            if (account != null)
            {
                if (Argon2.Verify(account.Password, _account.Password))
                {
                    Guid key = await _apiHandler.Get(account.Id);
                    
                    return new AccountMessage()
                    {
                        Success = true,
                        Message = key.ToString()
                    };
                }
                else
                {
                    return new AccountMessage()
                    {
                        Success = false,
                        Message = "Invalid username and/or password"
                    };
                }
            }
            
            return new AccountMessage()
            {
                Success = false,
                Message = "Invalid username and/or password"
            };
        }
        
        [HttpPost("account/register")]
        public async Task<ActionResult<AccountMessage>> AccountRegister(RegisterAccount _account)
        {
            if (!ModelState.IsValid)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Unknown validation error"
                };
            }
            
            Account accountUsername = await _db.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);
            Account accountEmail = await _db.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);

            if (accountUsername != null)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Username is already in use"
                };
            }
            
            if (accountEmail != null)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "E-Mail is already in use"
                };
            }

            _db.Account.Add(new Account()
            {
                Username = _account.Username,
                Password = Argon2.Hash(_account.Password)
            });
            
            await _db.SaveChangesAsync();
            
            return new AccountMessage()
            {
                Success = true,
                Message = ""
            };
        }
    }
}