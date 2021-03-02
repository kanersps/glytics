﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Auth;
using glytics.Data.Persistence;
using Isopoh.Cryptography.Argon2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace glytics.Logic.Account
{
    public class AccountService
    {
        private readonly GlyticsDbContext _dbContext;
        
        public AccountService(GlyticsDbContext _db)
        {
            _dbContext = _db;
        }
        
        private string GenerateJwtToken(Common.Models.Account account)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            
            byte[] key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("GLYTICS_SECRET") ?? "This should probably not be your key");
            
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<Common.Models.Account> GetAccountById(string id)
        {
            return await _dbContext.Account.FirstOrDefaultAsync(acc => acc.Id.ToString() == id);
        }

        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        public async Task<AuthenticationResponse> Authenticate(LoginAccount loginAccount)
        {
            Common.Models.Account account = await _dbContext.Account.FirstOrDefaultAsync(acc => acc.Username == loginAccount.Username);

            if (account != null)
            {
                if (Argon2.Verify(account.Password, loginAccount.Password))
                {
                    return new AuthenticationResponse()
                    {
                        Success = true,
                        Token = GenerateJwtToken(account)
                    };
                }
            }
            
            return new AuthenticationResponse()
            {
                Success = false,
                Message = "Invalid username and/or password"
            };
        }

        public async Task<AccountMessage> Register(RegisterAccount _account)
        {
            Common.Models.Account accountUsername = await _dbContext.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);
            Common.Models.Account accountEmail = await _dbContext.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);

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

            await _dbContext.Account.AddAsync(new Common.Models.Account()
            {
                Username = _account.Username,
                Password = Argon2.Hash(_account.Password)
            });
            
            await _dbContext.SaveChangesAsync();
            
            return new AccountMessage()
            {
                Success = true,
                Message = ""
            };
        }
    }
}