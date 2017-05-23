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
using EventHost.Web.Services;
using System.Net;

namespace EventHost.Web.Controllers
{
    [Authorize]
    public class ItineraryController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IViewRenderer _viewRenderer;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public ItineraryController(EventsDbContext dbContext,
            UserManager<User> userManager,
            IViewRenderer viewRenderer,
            IEmailSender emailSender,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _viewRenderer = viewRenderer;
            _emailSender = emailSender;
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

        [HttpPost("~/events/{eventId}/itinerary/email")]
        public async Task<IActionResult> EmailItinerary(int eventId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            try
            {
                var model = new EventItineraryEmailModel();
                model.Event = evt.ToModel();
                model.User = user.ToModel();

                model.Sections = await _dbContext.Sections
                    .Where(x => x.EventId == evt.Id)
                    .OrderBy(x => x.StartOn)
                    .ProjectTo<SectionModel>()
                    .ToListAsync();

                var userSessionIds = await _dbContext.Registrations
                    .Where(x => x.EventId == eventId
                        && x.UserId == user.Id)
                    .Select(x => x.SessionId)
                    .ToListAsync();
                model.RegisteredSessions = await _dbContext.Sessions
                    .Where(x => userSessionIds.Contains(x.Id))
                    .ProjectTo<SessionModel>()
                    .ToListAsync();

                model.ItineraryUrl = Url.Action("EventItinerary", "Itinerary", new { eventId }, Request.Scheme);
                model.EventUrl = Url.Action("Details", "Events", new { slug = evt.Slug }, Request.Scheme);

                var subject = $"Your {evt.Name} itinerary";
                var body = await _viewRenderer.RenderViewToStringAsync("Itinerary/ItineraryEmail", model);

                await _emailSender.SendEmailAsync(user.Email, subject, body);

                return Json(null);
            }
            catch(Exception ex)
            {
                _logger.LogError(0, ex, $"Error sending itinerary email to '{user.Email}' for event '{evt.Name}'");

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error sending itinerary email");
            }
        }
    }
}