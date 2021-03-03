using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using glytics.Logic.Application.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shyjus.BrowserDetection;

namespace glytics.Controllers
{
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IBrowserDetector _browserDetector;
        private readonly Analytic _analytic;
        private readonly UnitOfWork _unitOfWork;

        public StatisticController(IBrowserDetector browserDetector, Analytic analytic, UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _browserDetector = browserDetector;
            _analytic = analytic;
        }
        
        [HttpPost("app/web")]
        public async Task<ActionResult> HandleStatisticRequest(StatisticRequest request)
        {
            request.Sent = DateTime.Now.ToUniversalTime();

            if (request.Type != "view")
                return new BadRequestResult();

            Application site = _unitOfWork.Application.GetWithStatistics(request.Id);

            if (site == null || site.Active == false)
                return new BadRequestResult();

            await _analytic.New(site, request, _browserDetector.Browser, Request);
            
            return new OkResult();
        }
    }
}