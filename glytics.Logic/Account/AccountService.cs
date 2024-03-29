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
        private readonly IUnitOfWorkAccount _unitOfWorkAccount;
        private readonly IUnitOfWorkAccountSearch _unitOfWorkAccountSearch;
        
        public AccountService(IUnitOfWorkAccountSearch unitOfWork, IUnitOfWorkAccount unitOfWorkAccount)
        {
            _unitOfWorkAccount = unitOfWorkAccount;
            _unitOfWorkAccountSearch = unitOfWork;
        }

        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        public async Task<AuthenticationResponse> Authenticate(LoginAccount loginAccount)
        {
            Common.Models.Account account = _unitOfWorkAccountSearch.Account.GetByUsername(loginAccount.Username);

            if (account != null)
            {
                if (account.VerifyPassword(loginAccount.Password))
                {
                    return new AuthenticationResponse()
                    {
                        Success = true,
                        Token = account.GenerateJwt()
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
            Common.Models.Account accountUsername = _unitOfWorkAccountSearch.Account.GetByUsername(_account.Username);
            //Common.Models.Account accountEmail = await _dbContext.Account.FirstOrDefaultAsync(a => a.Username == _account.Username);

            if (accountUsername != null)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Username is already in use"
                };
            }

            Common.Models.Account acc = new Common.Models.Account()
            {
                Username = _account.Username,
                Password = Argon2.Hash(_account.Password)
            };
            
            _unitOfWorkAccount.Account.Add(acc);
            
            _unitOfWorkAccount.Save();
            
            return new AccountMessage()
            {
                Success = true,
                Message = acc.GenerateJwt()
            };
        }
    }
}