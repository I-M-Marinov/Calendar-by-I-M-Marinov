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

		// Default to an empty list of events
		IList<Event> events = new List<Event>();

		// Determine the selected calendar name for display
		var selectedCalendarName = "";

		if (selectedCalendarId == "all")
		{
			// Fetch events from all calendars if "All Calendars" is selected
			var allEvents = new List<Event>();

			foreach (var calendar in calendars)
			{
				var calendarEvents = await _googleCalendarService.GetEventsAsync(calendar.Id);
				allEvents.AddRange(calendarEvents);
			}

			events = allEvents
				.OrderBy(e => e.Start.DateTimeDateTimeOffset)
				.ToList();

			// Order events by start time

			selectedCalendarName = "All Calendars";
		}
		else if (!string.IsNullOrEmpty(selectedCalendarId))
		{
			// Fetch events from the selected calendar if a valid ID is provided
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
		// Fetch events from Google Calendar and store them in the database
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
}