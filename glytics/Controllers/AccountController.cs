using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using glytics.Models;
using glytics.Persistence;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace glytics.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        public AccountController(GlyticsDbContext dbContext)
        {
            _db = dbContext;
        }
        
        [HttpPost("account/login")]
        public ActionResult<AccountMessage> AccountLogin(LoginAccount _account)
        {
            Account account = _db.Account.FirstOrDefault(a => a.Username == _account.Username);

            if (account != null)
            {
                if (Argon2.Verify(account.Password, _account.Password))
                {
                    return new AccountMessage()
                    {
                        Success = true,
                        Message = account.Username
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
        public ActionResult<AccountMessage> AccountRegister(RegisterAccount _account)
        {
            if (!ModelState.IsValid)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Unknown validation error"
                };
            }
            
            Account accountUsername = _db.Account.FirstOrDefault(a => a.Username == _account.Username);
            Account accountEmail = _db.Account.FirstOrDefault(a => a.Username == _account.Username);

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
            _db.SaveChanges();
            
            return new AccountMessage()
            {
                Success = true,
                Message = ""
            };
        }
    }
}