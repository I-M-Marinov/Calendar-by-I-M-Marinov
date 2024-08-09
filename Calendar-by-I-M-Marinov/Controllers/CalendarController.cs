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
        var calendarViewModels = new List<CalendarViewModel>();

        foreach (var calendar in calendars)
        {
            calendarViewModels.Add(new CalendarViewModel
            {
                CalendarName = calendar.Summary,
                CalendarId = calendar.Id
            });
        }

        var events = new List<Event>();
        if (!string.IsNullOrEmpty(selectedCalendarId))
        {
            events = (await _googleCalendarService.GetEventsAsync(selectedCalendarId)).ToList();
        }

        var viewModel = new CalendarEventsViewModel
        {
            Calendars = calendarViewModels,
            SelectedCalendarId = selectedCalendarId,
            Events = events
        };

        return View(viewModel);
    }

    public async Task<IActionResult> ViewAllPrimary()
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
					DateTime = model.Start,
                    TimeZone = "Europe/Sofia"
				},
				End = new EventDateTime()
				{
					DateTime = model.End,
                    TimeZone = "Europe/Sofia"
                },
			};

			await _googleCalendarService.AddEventAsync(newEvent);
			return RedirectToAction("ViewAllPrimary");
		}

		return View(model);
	}
}