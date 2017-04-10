using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventHost.Web.Data;
using Microsoft.AspNetCore.Identity;
using EventHost.Web.Entities.Users;
using Microsoft.Extensions.Logging;
using EventHost.Web.Models.Registrations;
using EventHost.Web.Entities.Events;
using EventHost.Web.Mappers;
using System.Net;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace EventHost.Web.Controllers
{
    [Authorize]
    [Route("~/registrations")]
    public class RegistrationsController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public RegistrationsController(EventsDbContext dbContext,
            UserManager<User> userManager,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<RegistrationsController>();
        }

        [HttpGet("partial-list")]
        public async Task<IActionResult> PartialList(int eventId, int? sessionId = null)
        {
            var registrations = await _dbContext.Registrations
                .Where(x => x.EventId == eventId
                    && (!sessionId.HasValue || x.SessionId == sessionId.Value))
                .OrderBy(x => x.CreatedOn)
                .ProjectTo<RegistrationModel>()
                .ToListAsync();

            return PartialView("_RegistrationList", registrations);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create(CreateRegistrationModel model)
        {
            var evt = await _dbContext.Events.FindAsync(model.EventId);
            if (evt == null)
            {
                return BadRequest("Invalid event");
            }

            var session = await _dbContext.Sessions.FindAsync(model.SessionId);
            if (session == null || session.EventId != evt.Id)
            {
                return BadRequest("Invalid session");
            }

            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var existing = await _dbContext.Registrations
                    .AnyAsync(x => x.EventId == model.EventId
                        && x.SessionId == model.SessionId
                        && x.UserId == currentUser.Id);

                if (existing)
                {
                    return BadRequest("User is already registered for session");
                }

                var registration = new Registration
                {
                    Event = evt,
                    Session = session,
                    User = currentUser,
                    CreatedOn = DateTime.UtcNow
                };

                _dbContext.Registrations.Add(registration);
                await _dbContext.SaveChangesAsync();

                var result = registration.ToModel();
                return Created("", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(2, ex, "Error creating registration", model.EventId, model.SessionId, model.UserId);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error creating registration");
            }
        }

        [HttpDelete("")]
        public async Task<IActionResult> Delete(int eventId, int sessionId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                return BadRequest("Invalid event");
            }

            var session = await _dbContext.Sessions.FindAsync(sessionId);
            if (session == null || session.EventId != evt.Id)
            {
                return BadRequest("Invalid session");
            }

            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var registration = await _dbContext.Registrations
                    .SingleOrDefaultAsync(x => x.EventId == eventId
                        && x.SessionId == sessionId
                        && x.UserId == currentUser.Id);

                if (registration == null)
                {
                    return BadRequest("User is not registered for session");
                }

                _dbContext.Registrations.Remove(registration);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(2, ex, "Error deleting registration", eventId, sessionId);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error deleting registration");
            }
        }
    }
}