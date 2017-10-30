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
            var currentUser = await _userManager.GetUserAsync(User);

            var userEventIds = await _dbContext.EventMembers
                    .Where(x => x.UserId == currentUser.Id)
                    .Select(x => x.EventId)
                    .ToListAsync();

            var events = await _dbContext.Events.Include(x => x.Members)
                .Where(x => !x.HideFromPublicLists
                    || (userEventIds.Contains(x.Id)
                    || x.OwnerUserId == currentUser.Id))
                .OrderBy(x => x.StartOn)
                .ThenBy(x => x.EndOn)
                .ProjectTo<EventModel>()
                .ToListAsync();

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

            model.UserIsMember = await _dbContext.EventMembers.AnyAsync(x => x.EventId == evt.Id && x.UserId == currentUser.Id);

            if (evt.RequirePassword && !model.UserIsMember && evt.OwnerUserId != currentUser.Id)
            {
                _logger.LogInformation($"Event ID {evt.Id} ({evt.Name}) requires password to join and user ID {currentUser.Id} is not member");
                return RedirectToAction(nameof(JoinEvent), new { id = evt.Id });
            }

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

        [HttpGet("{id}/join")]
        public async Task<IActionResult> JoinEvent(int id)
        {
            var evt = await _dbContext.Events.FindAsync(id);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction(nameof(Index));
            }

            // TODO: Handle guest users (remove auth attribute)
            var currentUser = await _userManager.GetUserAsync(User);

            var model = new JoinEventViewModel();
            model.Event = evt.ToModel();
            model.IsCurrentUserMember = await _dbContext.EventMembers.AnyAsync(x => x.EventId == evt.Id && x.UserId == currentUser.Id);
            if (model.IsCurrentUserMember)
            {
                return RedirectToAction(nameof(Details), new { slug = evt.Slug });
            }

            return View(model);
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinEvent(int id, JoinEventViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(id);
            if (evt == null)
            {
                _logger.LogInformation($"Event ID {id} not found");

                // TODO: Display error
                return RedirectToAction(nameof(Index));
            }

            model.Event = evt.ToModel();

            var currentUser = await _userManager.GetUserAsync(User);
            var userIsMember = await _dbContext.EventMembers.AnyAsync(x => x.EventId == evt.Id && x.UserId == currentUser.Id);
            if (userIsMember)
            {
                _logger.LogInformation($"User ID {currentUser.Id} is already member of Event ID {evt.Id}");

                return RedirectToAction(nameof(Details), new { slug = evt.Slug });
            }

            if (evt.RequirePassword && model.Password != evt.JoinPassword)
            {
                ModelState.AddModelError(nameof(model.Password), "Event password is invalid");
                return View(model);
            }

            try
            {
                var member = new EventMember
                {
                    EventId = evt.Id,
                    UserId = currentUser.Id,
                    JoinDate = DateTime.UtcNow,
                    JoinMethod = EventMemberJoinMethod.Password
                };

                _dbContext.EventMembers.Add(member);
                await _dbContext.SaveChangesAsync();

                // TODO: show success message
                return RedirectToAction(nameof(Details), new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining user ID {currentUser.Id} to event ID {evt.Id}");
                return View(model);
            }
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
                    HideFromPublicLists = model.HideFromPublicLists,
                    RequirePassword = model.RequirePassword,
                    JoinPassword = model.JoinPassword,
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
                evt.HideFromPublicLists = model.HideFromPublicLists;
                evt.RequirePassword = model.RequirePassword;
                evt.JoinPassword = model.JoinPassword;

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
