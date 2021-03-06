﻿using System;
using System.Threading.Tasks;
using glytics.Attributes;
using glytics.Common.Models;
using glytics.Common.Models.Auth;
using glytics.Data;
using glytics.Data.Persistence;
using glytics.Logic;
using glytics.Logic.Account;
using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        private readonly AccountService _accountService;
        public AccountController(GlyticsDbContext dbContext, AccountService accountService)
        {
            _db = dbContext;
            _accountService = accountService;
        }

        [HttpGet("account")]
        [Authenticated]
        public ActionResult<LoginAccount> Account()
        {
            Account account = (Account) HttpContext.Items["Account"];

            return new LoginAccount()
            {
                Username = account?.Username,
                Password = account?.Password,
            };
        }
        
        [HttpGet("account/authenticated")]
        [Authenticated]
        public ActionResult<AccountMessage> AccountAuthenticated()
        {
            return new AccountMessage()
            {
                Success = true,
                Message = ""
            };
        }
        
        [HttpPost("account/login")]
        public async Task<ActionResult<AuthenticationResponse>> AccountLogin(LoginAccount account)
        {
            CaptchaCheck captcha = new CaptchaCheck(account.RecaptchaToken);
            if(!captcha.Verify())
                return new AuthenticationResponse()
                {
                    Success = false,
                    Message = "Invalid captcha! Please try again."
                };

            return await _accountService.Authenticate(account);
        }
        
        [HttpPost("account/register")]
        public async Task<ActionResult<AccountMessage>> AccountRegister(RegisterAccount account)
        {
            if (!ModelState.IsValid)
            {
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Unknown validation error"
                };
            }

            CaptchaCheck captcha = new CaptchaCheck(account.RecaptchaToken);
            if(!captcha.Verify())
                return new AccountMessage()
                {
                    Success = false,
                    Message = "Invalid captcha! Please refresh the page and try again."
                };

            return await _accountService.Register(account);
        }
    }
}