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

	public async Task<IActionResult> ViewAll()
	{
		// Fetch events from Google Calendar and store them in the database
		var events = await _googleCalendarService.GetEventsAsync();
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
			return RedirectToAction("ViewAll");
		}

		return View(model);
	}
}