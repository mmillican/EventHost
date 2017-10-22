using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHost.Web.Data;
using EventHost.Web.Models.Events;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using EventHost.Web.Entities.Events;
using Microsoft.AspNetCore.Identity;
using EventHost.Web.Entities.Users;
using Microsoft.Extensions.Logging;
using EventHost.Web.Mappers;
using EventHost.Web.Models.Sections;
using System.Linq;
using EventHost.Web.Models.Sessions;
using EventHost.Web.Models.Registrations;
using EventHost.Web.Helpers;

namespace EventHost.Web.Controllers
{
    [Authorize]
    [Route("~/events")]
    public class EventsController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public EventsController(EventsDbContext dbContext,
            UserManager<User> userManager,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<EventsController>();
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var events = await _dbContext.Events.ProjectTo<EventModel>().ToListAsync();

            var model = new EventListViewModel
            {
                Events = events
            };

            return View(model);
        }

        [HttpGet("new")]
        public IActionResult Create()
        {
            var model = new EditEventViewModel();
            model.StartOn = DateTime.Now;
            model.EndOn = DateTime.Now.AddDays(1);

            return View(model);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var evt = await _dbContext.Events.SingleOrDefaultAsync(x => x.Slug == slug);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var model = new EventDetailsViewModel();
            model.Event = evt.ToModel();
            model.CurrentUserId = currentUser.Id;

            model.UserCanEdit = evt.OwnerUserId == currentUser.Id;
            model.RegistrationIsOpen = evt.IsEventRegistrationOpen(DateTime.UtcNow);

            model.Sections = await _dbContext.Sections
                .Where(x => x.EventId == evt.Id)
                .OrderBy(x => x.StartOn)
                .ProjectTo<SectionModel>()
                .ToListAsync();

            model.Sessions = await _dbContext.Sessions
                .Where(x => x.EventId == evt.Id)
                .ProjectTo<SessionModel>()
                .ToListAsync();
            
            model.Registrations = await _dbContext.Registrations
                .Where(x => x.EventId == evt.Id)
                .OrderBy(x => x.CreatedOn)
                .ProjectTo<RegistrationModel>()
                .ToListAsync();

            return View(model);
        }

        [HttpPost("new")]
        public async Task<IActionResult> Create(EditEventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            try
            {
                var evt = new Event
                {
                    Name = model.Name,
                    Description = model.Description,
                    StartOn = model.StartOn,
                    EndOn = model.EndOn,
                    RegistrationStartOn = model.RegistrationStartOn,
                    RegistrationEndOn = model.RegistrationEndOn,
                    EnableWaitLists = model.EnableWaitLists,
                    EnableAutomaticApproval = model.EnableAutomaticApproval,
                    Owner = currentUser
                };

                evt.Slug = await GenerateEventSlug(evt);

                _dbContext.Events.Add(evt);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { slug = evt.Slug });
            }
            catch(Exception ex)
            {
                _logger.LogError(2, ex, "Error creating event");

                ModelState.AddModelError("", "There was an error creating the event.  Try again.");
                return View(model);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var evt = await _dbContext.Events.FindAsync(id);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction(nameof(Index));
            }

            var model = evt.ToEditModel();

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, EditEventViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(id);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            try
            {
                evt.Name = model.Name;
                evt.Description = model.Description;
                evt.StartOn = model.StartOn;
                evt.EndOn = model.EndOn;
                evt.RegistrationStartOn = model.RegistrationStartOn;
                evt.RegistrationEndOn = model.RegistrationEndOn;
                evt.EnableWaitLists = model.EnableWaitLists;
                evt.EnableAutomaticApproval = model.EnableAutomaticApproval;

                if (string.IsNullOrEmpty(evt.Slug))
                    evt.Slug = await GenerateEventSlug(evt);

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(3, ex, "Error updating event");

                ModelState.AddModelError("", "There was an error updating the event.  Try again.");
                return View(model);
            }
        }

        public async Task<string> GenerateEventSlug(Event evt)
        {
            if (string.IsNullOrEmpty(evt.Name))
            {
                throw new Exception("Event name must have a value");
            }

            try
            {
                var slug = $"{evt.StartOn:yyyy-MM-dd}-{evt.Name.Clean()}";
                if (!await DoesSlugExist(slug, evt.Id))
                {
                    return slug;
                }

                var appendIdx = 1;
                var modSlug = $"{slug}-{appendIdx})";
                while (await DoesSlugExist(modSlug, evt.Id))
                {
                    appendIdx++;
                    modSlug = $"{slug}-{appendIdx})";
                }

                return modSlug;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not generate slug for event ID {evt.Id} ({evt.Name})");
            }
        }

        private async Task<bool> DoesSlugExist(string slug, int eventId)
        {
            bool exists;
            if (eventId != 0)
            {
                exists = await _dbContext.Events.AnyAsync(x => x.Id != eventId && x.Slug == slug);
            }
            else
            {
                exists = await _dbContext.Events.AnyAsync(x => x.Slug == slug);
            }

            return exists;
        }
    }
}
