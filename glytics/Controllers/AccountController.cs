using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using glytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace glytics.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("account/login")]
        public ActionResult<Object> AccountLogin(LoginAccount account)
        {
            return new
            {
                Success = true,
                Message = account.Username
            };
        }
    }
}