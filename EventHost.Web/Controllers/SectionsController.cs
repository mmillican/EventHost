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
using EventHost.Web.Models.Sections;
using EventHost.Web.Entities.Events;
using EventHost.Web.Mappers;

namespace EventHost.Web.Controllers
{
    [Authorize]
    [Route("~/events/{eventId}/sections")]
    public class SectionsController : Controller
    {
        private readonly EventsDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public SectionsController(EventsDbContext dbContext,
            UserManager<User> userManager,
            ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger<SectionsController>();
        }
        
        [HttpGet("new")]
        public async Task<IActionResult> Create(int eventId)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            var model = new EditSectionViewModel();
            model.EventId = evt.Id;
            model.EventName = evt.Name;
            model.StartOn = evt.StartOn;
            model.EndOn = evt.StartOn;

            return View(model);
        }

        [HttpPost("new")]
        public async Task<IActionResult> Create(int eventId, EditSectionViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            if (model.StartOn < evt.StartOn || model.StartOn > evt.EndOn)
            {
                ModelState.AddModelError(nameof(model.StartOn), "Must be during event times");
            }

            if (model.EndOn < evt.StartOn || model.EndOn > evt.EndOn)
            {
                ModelState.AddModelError(nameof(model.EndOn), "Must be during event times");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var section = new Section
                {
                    Event = evt,
                    Name = model.Name,
                    Description = model.Description,
                    StartOn = model.StartOn,
                    EndOn = model.EndOn
                };

                _dbContext.Sections.Add(section);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(2, ex, "Error creating section");

                ModelState.AddModelError("", "There was an error creating the section.  Try again.");
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

            var section = await _dbContext.Sections.FindAsync(id);
            if (section == null || section.EventId != eventId)
            {
                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }

            var model = section.ToEditModel();

            return View(model);
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int eventId, int id, EditSectionViewModel model)
        {
            var evt = await _dbContext.Events.FindAsync(eventId);
            if (evt == null)
            {
                // TODO: Diplay error
                return RedirectToAction("Index", "Events");
            }

            var section = await _dbContext.Sections.FindAsync(id);
            if (section == null || section.EventId != eventId)
            {
                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }

            if (model.StartOn < evt.StartOn || model.StartOn > evt.EndOn)
            {
                ModelState.AddModelError(nameof(model.StartOn), "Must be during event times");
            }

            if (model.EndOn < evt.StartOn || model.EndOn > evt.EndOn)
            {
                ModelState.AddModelError(nameof(model.EndOn), "Must be during event times");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                section.Name = model.Name;
                section.Description = model.Description;
                section.StartOn = model.StartOn;
                section.EndOn = model.EndOn;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Details", "Events", new { slug = evt.Slug });
            }
            catch (Exception ex)
            {
                _logger.LogError(3, ex, "Error updating section");

                ModelState.AddModelError("", "There was an error updating the section.  Try again.");
                return View(model);
            }
        }
    }
}