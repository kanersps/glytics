using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Data.Persistence;
using glytics.Logic.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace glytics.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UnitOfWork _unitOfWork)
        {
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
                await AttachAccount(context, _unitOfWork, token);
            
            await _next(context);
        }

        private async Task AttachAccount(HttpContext context, UnitOfWork _unitOfWork, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("GLYTICS_SECRET") ?? "This should probably not be your key");
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

                context.Items["Account"] = _unitOfWork.Account.GetById(userId);
            }
            catch
            {
                // ignored
            }
        }
    }
}