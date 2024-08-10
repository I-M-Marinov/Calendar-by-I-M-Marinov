using Microsoft.AspNetCore.Mvc;
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

        var matchingEvents = await _googleCalendarService.GetEventByIdAcrossAllCalendarsAsync(model.EventName);

        if (matchingEvents == null || !matchingEvents.Any())
        {
            ModelState.AddModelError("", "No events found with the provided name.");
            return View(model);
        }

        // Map the matching events to the EditEventViewModel and add them to a List
        var eventViewModels = matchingEvents.Select(e => new EditEventViewModel
        {
            EventId = e.Id,
            Summary = e.Summary,
            Start = e.Start.DateTime,
            End = e.End.DateTime,
            Location = e.Location,
            CalendarId = e.Organizer?.Email 
        }).ToList();

        return View("EditEvents", eventViewModels);
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
    public async Task<IActionResult> EditEvent(string eventId, string calendarId)
    {
        var eventToEdit = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId);

        if (eventToEdit == null)
        {
            return NotFound($"Event with ID {eventId} not found.");
        }

        var viewModel = new EditEventViewModel
        {
            EventId = eventToEdit.Id,
            Summary = eventToEdit.Summary,
            Start = eventToEdit.Start.DateTime,
            End = eventToEdit.End.DateTime,
            Location = eventToEdit.Location,
            CalendarId = calendarId
        };

        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> EditEvents(Dictionary<string, EditEventViewModel> events)
    {
        // Validate and process each event
        foreach (var eventModel in events.Values)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var eventToUpdate = new Event
                    {
                        Id = eventModel.EventId,
                        Summary = eventModel.Summary,
                        Start = new EventDateTime { DateTime = eventModel.Start },
                        End = new EventDateTime { DateTime = eventModel.End },
                        Location = eventModel.Location
                    };

                    await _googleCalendarService.AddEventAsync(eventModel.CalendarId, eventModel.EventId, eventToUpdate);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating event {eventModel.EventId}: {ex.Message}");
                }
            }
        }

        if (ModelState.IsValid)
        {
            return RedirectToAction("SearchEventByName");
        }

        // Redisplay the form with validation errors

        var matchingEvents = await _googleCalendarService.GetEventByIdAcrossAllCalendarsAsync(events.Values.First().Summary);
        return View("EditEvents", matchingEvents.Select(e => new EditEventViewModel
        {
            EventId = e.Id,
            Summary = e.Summary,
            Start = e.Start.DateTime,
            End = e.End.DateTime,
            Location = e.Location,
            CalendarId = e.Organizer?.Email
        }).ToList());
    }

}