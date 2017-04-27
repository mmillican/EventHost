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
using AutoMapper;
using EventHost.Web.Models.Sessions;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Users;
using Newtonsoft.Json;

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

        [HttpGet("~/events/{eventId}/registrations")]
        public async Task<IActionResult> EventRegistrations(int eventId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                return NotFound();
            }

            var model = new EventRegistrationsViewModel();
            model.Event = evt.ToModel();

            model.Sections = await _dbContext.Sections
                .Where(x => x.EventId == evt.Id)
                .OrderBy(x => x.StartOn)
                .ProjectTo<SectionModel>()
                .ToListAsync();

            model.Registrations = await _dbContext.Registrations
                .Where(x => x.EventId == evt.Id)
                .ProjectTo<RegistrationModel>()
                .ToListAsync();

            var registeredUserIds = model.Registrations.Select(x => x.UserId).Distinct();
            model.RegisteredUsers = await _dbContext.Users
                .Where(x => registeredUserIds.Contains(x.Id))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ProjectTo<UserModel>()
                .ToListAsync();

            return View(model);
        }

        [HttpGet("~/events/{eventId}/registrations/user/{userId}")]
        public async Task<IActionResult> EventUserRegistrations(int eventId, int userId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (evt.OwnerUserId != currentUser.Id && userId != currentUser.Id)
            {
                return NotFound();
            }

            UserModel viewingUser = null;
            if (currentUser.Id != userId)
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                    viewingUser = user.ToModel();
            }
            else
            {
                viewingUser = currentUser.ToModel();
            }

            var model = new EventUserRegistrationsViewModel();
            model.Event = evt.ToModel();
            model.User = viewingUser;

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
        
        [HttpGet("partial")]
        public async Task<IActionResult> SessionRegistrations(int sessionId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var session = await _dbContext.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var evt = await _dbContext.Events.FindAsync(session.EventId);
            if (evt == null)
            {
                return NotFound();
            }
            
            var model = new SessionRegistrationViewModel
            {
                Session = Mapper.Map<SessionModel>(session),
                UserCanManageRegistrations = currentUser.Id == evt.OwnerUserId,
                RequiresApproval = !evt.EnableAutomaticApproval
            };

            model.Registrations = await _dbContext.Registrations
                .Where(x => x.SessionId == sessionId)
                .OrderBy(x => x.CreatedOn)
                .ProjectTo<RegistrationModel>()
                .ToListAsync();
            model.CurrentUserIsRegistered = model.Registrations.Select(x => x.UserId).Contains(currentUser.Id);

            return PartialView("_SessionRegistrations", model);
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

                var hasSameSectionReg = await _dbContext.Registrations
                    .AnyAsync(x => x.Session.SectionId == session.SectionId
                        && x.UserId == currentUser.Id);

                if (hasSameSectionReg)
                {
                    return BadRequest("User is already registered for session in the time section");
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

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var registration = await _dbContext.Registrations.FindAsync(id);
            if (registration == null)
                return NotFound();

            var evt = await _dbContext.Events.FindAsync(registration.EventId);
            if (evt == null)
                return NotFound();

            if (currentUser.Id != evt.OwnerUserId)
                return Unauthorized();

            if (evt.EnableAutomaticApproval)
                return BadRequest("Event does not require approvals");

            if (registration.ApprovedOn.HasValue)
                return BadRequest("Registration already approved");

            try
            {
                registration.ApprovedOn = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                var result = registration.ToModel();
                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(3, ex, "Error approving registration", registration.Id, evt.Id);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error approving registration");
            }
        }

        [HttpPost("unapprove/{id}")]
        public async Task<IActionResult> Unapprove(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var registration = await _dbContext.Registrations.FindAsync(id);
            if (registration == null)
                return NotFound();

            var evt = await _dbContext.Events.FindAsync(registration.EventId);
            if (evt == null)
                return NotFound();

            if (currentUser.Id != evt.OwnerUserId)
                return Unauthorized();

            if (evt.EnableAutomaticApproval)
                return BadRequest("Event does not require approvals");

            if (!registration.ApprovedOn.HasValue)
                return BadRequest("Registration not approved");

            try
            {
                registration.ApprovedOn = null;
                await _dbContext.SaveChangesAsync();

                var result = registration.ToModel();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(4, ex, "Error unapproving registration", registration.Id, evt.Id);

                return StatusCode((int)HttpStatusCode.InternalServerError, "Error unapproving registration");
            }
        }
    }
}