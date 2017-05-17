using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventHost.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using EventHost.Web.Entities.Users;
using EventHost.Web.Models.Users;
using EventHost.Web.Mappers;
using EventHost.Web.Models.Itinerary;
using EventHost.Web.Models.Sections;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using EventHost.Web.Models.Sessions;
using Newtonsoft.Json;

namespace EventHost.Web.Controllers
{
    [Authorize]
    public class ItineraryController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public ItineraryController(EventsDbContext dbContext,
            UserManager<User> userManager,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<ItineraryController>();
        }


        [HttpGet("~/events/{eventId}/itinerary")]
        public async Task<IActionResult> EventItinerary(int eventId, int? userId = null)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (!userId.HasValue)
                userId = currentUser.Id;
            else if (userId.HasValue && userId != currentUser.Id && evt.OwnerUserId != currentUser.Id)
                return NotFound();

            var user = await _dbContext.Users.FindAsync(userId);
            
            var model = new EventItineraryViewModel();
            model.Event = evt.ToModel();
            model.User = user.ToModel();

            model.Sections = await _dbContext.Sections
                .Where(x => x.EventId == evt.Id)
                .OrderBy(x => x.StartOn)
                .ProjectTo<SectionModel>()
                .ToListAsync();

            var userSessionIds = await _dbContext.Registrations
                .Where(x => x.EventId == eventId
                    && x.UserId == userId)
                .Select(x => x.SessionId)
                .ToListAsync();
            model.RegisteredSessions = await _dbContext.Sessions
                .Where(x => userSessionIds.Contains(x.Id))
                .ProjectTo<SessionModel>()
                .ToListAsync();

            model.SerializedSessions = JsonConvert.SerializeObject(model.RegisteredSessions);

            return View(model);
        }
    }
}