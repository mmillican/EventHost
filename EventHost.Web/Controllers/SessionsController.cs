using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EventHost.Web.Data;
using Microsoft.AspNetCore.Identity;
using EventHost.Web.Entities.Users;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using EventHost.Web.Models.Sessions;
using EventHost.Web.Entities.Events;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventHost.Web.Mappers;

namespace EventHost.Web.Controllers
{
    [Authorize]
    [Route("~/events/{eventId}/sessions")]
    public class SessionsController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public SessionsController(EventsDbContext dbContext,
            UserManager<User> userManager,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<SessionsController>();
        }

        [HttpGet("new")]
        public async Task<IActionResult> Create(int eventId, int? sectionId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            //Section section = null;
            //if (sectionId.HasValue)
            //{
            //    section = await _dbContext.Sections.FindAsync(sectionId.Value);
            //    if (section == null || section.EventId != eventId)
            //    {
            //        return RedirectToAction("Details", "Events", new { id = eventId });
            //    }
            //}

            var model = new EditSessionViewModel();
            model.EventId = evt.Id;
            model.EventName = evt.Name;
            model.SectionId = sectionId ?? 0;
            model.SectionOptions = await GetEventSectionOptions(eventId);
            model.UserOptions = await GetUserOptions();

            // Defaults
            model.AllowRegistrations = true;
            model.EnableWaitList = evt.EnableWaitLists;

            return View(model);
        }

        [HttpPost("new")]
        public async Task<IActionResult> Create(int eventId, int? sectionId, EditSessionViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            model.SectionOptions = await GetEventSectionOptions(eventId);
            model.UserOptions = await GetUserOptions();
            model.EnableWaitList = evt.EnableWaitLists;

            var section = await _dbContext.Sections.FindAsync(model.SectionId);
            if (section == null)
            {
                ModelState.AddModelError(nameof(model.SectionId), "A valid section is required");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var session = new Session
                {
                    Event = evt,
                    Section = section,
                    Name = model.Name,
                    Description = model.Description,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    PostalCode = model.PostalCode,
                    AllowRegistrations = model.AllowRegistrations,
                    MaxSpots = model.MaxSpots,
                    ReservedSpots = model.ReservedSpots,
                    AllowWaitList = evt.EnableWaitLists && model.AllowWaitList,
                    HostUserId = model.HostUserId,
                    LastUpdatedOn = DateTime.Now
                };

                if(!session.HostUserId.HasValue)
                {
                    session.HostName = model.HostName;
                }

                _dbContext.Sessions.Add(session);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(2, ex, "Error creating session");

                ModelState.AddModelError("", "There was an error creating the session.  Try again.");
                return View(model);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int eventId, int id)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            var session = await _dbContext.Sessions.FindAsync(id);
            if (session == null || session.EventId != eventId)
            {
                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }

            var model = session.ToEditModel();
            model.SectionOptions = await GetEventSectionOptions(eventId);
            model.UserOptions = await GetUserOptions();
            model.EnableWaitList = evt.EnableWaitLists;

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int eventId, int id, EditSessionViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            model.SectionOptions = await GetEventSectionOptions(eventId);
            model.UserOptions = await GetUserOptions();
            model.EnableWaitList = evt.EnableWaitLists;

            var session = await _dbContext.Sessions.FindAsync(id);
            if (session == null || session.EventId != eventId)
            {
                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (model.SectionId != session.SectionId)
                {
                    var section = await _dbContext.Sections.FindAsync(model.SectionId);
                    if (section == null)
                    {
                        ModelState.AddModelError(nameof(model.SectionId), "A valid section is required");
                    }
                    else
                    {
                        session.Section = section;
                    }
                }
                
                session.Name = model.Name;
                session.Description = model.Description;
                session.Address = model.Address;
                session.City = model.City;
                session.State = model.State;
                session.PostalCode = model.PostalCode;
                session.AllowRegistrations = model.AllowRegistrations;
                session.MaxSpots = model.MaxSpots;
                session.ReservedSpots = model.ReservedSpots;
                session.AllowWaitList = evt.EnableWaitLists && model.AllowWaitList;
                session.HostUserId = model.HostUserId;
                if (!session.HostUserId.HasValue)
                {
                    session.HostName = model.HostName;
                }
                session.LastUpdatedOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(3, ex, "Error updating session");

                ModelState.AddModelError("", "There was an error updating the session.  Try again.");
                return View(model);
            }
        }

        private async Task<IEnumerable<SelectListItem>> GetEventSectionOptions(int eventId)
        {
            var sections = await _dbContext.Sections
                .Where(x => x.EventId == eventId)
                .OrderBy(x => x.StartOn)
                .Select(x => new SelectListItem {
                    Value = x.Id.ToString(),
                    Text = $"{x.Name} ({x.StartOn:ddd, h:mm tt} - {x.EndOn:ddd, h:mm tt})"
                })
                .ToListAsync();

            return sections;
        }

        private async Task<IEnumerable<SelectListItem>> GetUserOptions()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = $"{x.LastName}, {x.FirstName}" })
                .ToListAsync();

            return users;
        }
    }
}