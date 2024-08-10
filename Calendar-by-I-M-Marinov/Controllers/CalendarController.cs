using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using Calendar_by_I_M_Marinov.Services.Contracts;
using Calendar_by_I_M_Marinov.Models;

public class CalendarController : Controller
{
    private readonly IGoogleCalendarService _googleCalendarService;

    public CalendarController(IGoogleCalendarService googleCalendarService)
    {
        _googleCalendarService = googleCalendarService;
    }

    public async Task<IActionResult> ListCalendars()
    {
        var calendars = await _googleCalendarService.GetCalendarsAsync();
        var calendarViewModels = new List<CalendarViewModel>();

        foreach (var calendar in calendars)
        {
            calendarViewModels.Add(new CalendarViewModel
            {
                CalendarName = calendar.Summary,
                CalendarId = calendar.Id
            });
        }

        return View(calendarViewModels);
    }

    public async Task<IActionResult> ListCalendarsAndEvents(string selectedCalendarId)
    {
        var calendars = await _googleCalendarService.GetCalendarsAsync();
        var calendarViewModels = calendars.Select(c => new CalendarViewModel
        {
            CalendarName = c.Summary,
            CalendarId = c.Id
        }).ToList();

        IList<Event> events = new List<Event>();

        var selectedCalendarName = "";

        if (selectedCalendarId == "all")
        {
            var allEvents = new List<Event>();

            foreach (var calendar in calendars)
            {
                var calendarEvents = await _googleCalendarService.GetEventsAsync(calendar.Id);
                allEvents.AddRange(calendarEvents);
            }

            events = allEvents
                .OrderBy(e => e.Start.DateTimeDateTimeOffset)
                .ToList();

            selectedCalendarName = "All Calendars";
        }
        else if (!string.IsNullOrEmpty(selectedCalendarId))
        {
            events = (await _googleCalendarService.GetEventsAsync(selectedCalendarId)).ToList();
            selectedCalendarName = calendarViewModels
                .FirstOrDefault(c => c.CalendarId == selectedCalendarId)?.CalendarName ?? "Unknown Calendar";
        }

        var viewModel = new CalendarEventsViewModel
        {
            Calendars = calendarViewModels,
            SelectedCalendarId = selectedCalendarId,
            SelectedCalendarName = selectedCalendarName,
            Events = events
        };

        return View(viewModel);
    }

    public async Task<IActionResult> ViewNewEventAdded()
    {
        var events = await _googleCalendarService.GetEventsAsync("primary");
        return View(events);
    }

    public IActionResult CreateEvent()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventViewModel model)
    {
        if (ModelState.IsValid)
        {
            var newEvent = new Event()
            {
                Summary = model.Summary,
                Location = model.Location,
                Description = model.Description,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = model.Start,
                    TimeZone = "Europe/Sofia"
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = model.End,
                    TimeZone = "Europe/Sofia"
                },
            };

            await _googleCalendarService.AddEventAsync(newEvent);
            return RedirectToAction("ViewNewEventAdded");
        }

        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> SearchEventByName(SearchEventViewModel model)
    {
        if (string.IsNullOrEmpty(model.EventName))
        {
            ModelState.AddModelError("", "Please provide an event name.");
            return View(model);
        }

        string eventNameString = model.EventName;

        var matchingEvents = await _googleCalendarService.GetEventByIdAcrossAllCalendarsAsync(eventNameString);

        if (matchingEvents == null || !matchingEvents.Any())
        {
            ModelState.AddModelError("", "No events found with the provided name.");
            return View(model);
        }

        var eventToEdit = matchingEvents.First();

        return RedirectToAction("EditEvents", new { eventId = eventToEdit.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {

        var eventToEdit = await _googleCalendarService.GetEventsAsync(id);

        if (eventToEdit == null)
        {
            return NotFound();
        }

        return View(eventToEdit);
    }

    [HttpGet]
    public async Task<IActionResult> EditEvents(string eventId)
    {
        var eventItems = await _googleCalendarService.GetEventByIdAcrossAllCalendarsAsync(eventId);

        // Create a list of view models for each matching event
        var eventViewModels = eventItems.Select(eventItem => new EditEventViewModel
        {
            EventId = eventItem.Id,
            Summary = eventItem.Summary,
            Start = eventItem.Start.DateTime ?? DateTime.Now,  // Default to DateTime.Now if null
            End = eventItem.End.DateTime ?? DateTime.Now,      // Default to DateTime.Now if null
            Location = eventItem.Location
        }).ToList();

        // Return a view that displays all matching events
        return View("EditEvent", eventViewModels);
    }
}