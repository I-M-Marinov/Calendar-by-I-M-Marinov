using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Calendar.v3.Data;
using Calendar_by_I_M_Marinov.Services.Contracts;
using Calendar_by_I_M_Marinov.Models;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc.Rendering;
using Calendar_by_I_M_Marinov.Common;

public class CalendarController : Controller
{
    private readonly IGoogleCalendarService _googleCalendarService;

	public CalendarController(IGoogleCalendarService googleCalendarService)
    {
        _googleCalendarService = googleCalendarService;
	}


    public async Task<IActionResult> ListCalendars()
    {
        var calendars = await _googleCalendarService.GetAllCalendarsAsync();

        var calendarViewModels = new List<CalendarViewModel>();

        foreach (var calendar in calendars)
        {
            var events = await _googleCalendarService.GetEventsForCalendarAsync(calendar.Id);

            var calendarViewModel = new CalendarViewModel
            {
                CalendarName = calendar.Summary,
                CalendarId = calendar.Id,
                Description = calendar.Description,
                AccessRole = calendar.AccessRole,
                EventsCount = events.Count,
                Primary = calendar.Primary
            };

            calendarViewModels.Add(calendarViewModel);
        }

        calendarViewModels = calendarViewModels
            .OrderBy(c => c.AccessRole)
            .ToList();

        return View("ListAllCalendars", calendarViewModels);
    }

    [HttpGet]
    public async Task<IActionResult> ListCalendarsAndEvents(string selectedCalendarId)
    {
        try
        {
            var calendars = await _googleCalendarService.GetAllCalendarsAsync();

            var calendarViewModels = calendars.Select(c => new CalendarViewModel
            {
                CalendarName = c.Summary,
                CalendarId = c.Id,
                AccessRole = c.AccessRole
            }).ToList();

            List<Event> events = new List<Event>();
            string selectedCalendarName = "";
            string accessRole = "";
            int eventsCount = 0;
            var eventCalendarMap = new Dictionary<string, string>();

            if (selectedCalendarId == "all")
            {
                var allEvents = new List<Event>();

                foreach (var calendar in calendars)
                {
                    var calendarEvents = await _googleCalendarService.GetEventsAsync(calendar.Id);
                    allEvents.AddRange(calendarEvents);

                    foreach (var evt in calendarEvents)
                    {
                        eventCalendarMap[evt.Id] = calendar.Id;
                    }
                }

                events = allEvents.OrderBy(e => e.Start.DateTimeDateTimeOffset).ToList();
                selectedCalendarName = "All Calendars";
            }
            else if (!string.IsNullOrEmpty(selectedCalendarId))
            {
                events = (await _googleCalendarService.GetEventsAsync(selectedCalendarId)).ToList();
                eventsCount = events.Count;

                var selectedCalendar = calendarViewModels.FirstOrDefault(c => c.CalendarId == selectedCalendarId);
                if (selectedCalendar != null)
                {
                    selectedCalendarName = selectedCalendar.CalendarName;
                    accessRole = selectedCalendar.AccessRole;
                }

                foreach (var evt in events)
                {
                    eventCalendarMap[evt.Id] = selectedCalendarId;
                }
            }

            var viewModel = new CalendarEventsViewModel
            {
                Calendars = calendarViewModels,
                SelectedCalendarId = selectedCalendarId,
                SelectedCalendarName = selectedCalendarName,
                AccessRole = accessRole,
                Events = events, 
                EventsCount = eventsCount,
                EventCalendarMap = eventCalendarMap
            };

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.SuccessEventId = TempData["SuccessEventId"];

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while loading events: {ex.Message}");
            return View("Error");
        }
    }


    public async Task<IActionResult> ViewNewEventAdded()
    {
        var events = await _googleCalendarService.GetEventsAsync("primary");
        var lastAdded = events.OrderByDescending(e => e.CreatedDateTimeOffset ?? e.UpdatedDateTimeOffset)
            .FirstOrDefault();

        if (lastAdded == null)
        {
            return NotFound("No events found.");
        }


        return View(lastAdded);
    }

    public async Task<IActionResult> ViewNewEventUpdated(string calendarId, string eventId)
    {
        // Retrieve the specific event by its ID
        var eventToView = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId);

        if (eventToView == null)
        {
            return NotFound("Event not found.");
        }

        // Pass the calendarId to the view via ViewBag if needed
        ViewBag.CalendarId = calendarId;

        return View(eventToView);
    }

	[HttpGet]
	public async Task<IActionResult> CreateEvent(string calendarId, string? eventId)
	{
		// Initialize the model
		var model = new EventViewModel
		{
			Attendants = new List<string>(),
            CalendarId = calendarId
		};

		model.VisibilityOptions = new List<SelectListItem>
	{
		new SelectListItem { Value = "public", Text = "Public" },
		new SelectListItem { Value = "private", Text = "Private" }
    };

		model.EventTypeOptions = new List<SelectListItem>
	{
		new SelectListItem { Value = "single", Text = "Single" },
		new SelectListItem { Value = "annual", Text = "Annual" }
    };

		if (!string.IsNullOrEmpty(eventId))
		{
			try
			{
				var existingEvent = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId); // Get the Event details

				model.EventId = existingEvent.Id;
				model.Summary = existingEvent.Summary;
				model.Description = existingEvent.Description;
				model.Location = existingEvent.Location;
				model.IsAllDayEvent = existingEvent.Start.Date != null && existingEvent.End.Date != null;
                model.RecurringEventId = existingEvent.RecurringEventId;

				bool isAllDayEvent = model.IsAllDayEvent;

				
				if (existingEvent.RecurringEventId != null)
				{
					model.EventType = "annual";
					if (isAllDayEvent)
					{
						model.Start = DateTime.Parse(existingEvent.Start.Date);
					}
					else
					{
						model.Start = existingEvent.Start.DateTime;
						model.End = existingEvent.End.DateTime;
					}
				}
				else
				{
					model.EventType = "single";
					if (isAllDayEvent)
					{
						model.Start = DateTime.Parse(existingEvent.Start.Date);
					}
					else
					{
						model.Start = existingEvent.Start.DateTime;
						model.End = existingEvent.End.DateTime;
					}
				}

				if (existingEvent.Attendees != null)
				{
					model.Attendants = existingEvent.Attendees.Select(a => a.Email).ToList();
				}

				model.Visibility = existingEvent.Visibility;

				model.SendUpdates = existingEvent.ExtendedProperties?.Shared != null &&
				                    existingEvent.ExtendedProperties.Shared.ContainsKey("SendUpdates")
					? existingEvent.ExtendedProperties.Shared["SendUpdates"]
					: "None";
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", $"Error retrieving event: {ex.Message}");
				return View(model); 
			}
		}

		// Return the view with the model for editing or creating
		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> CreateEvent(EventViewModel model)
	{
		
		var calendarId = model.CalendarId != null ? model.CalendarId : "primary";

		var timeZone = "Europe/Sofia";
		EventDateTime startEventDateTime;
		EventDateTime endEventDateTime;

		if (model.EventType == "annual" || model.RecurringEventId != null)
		{
			// Annual event
			if (model.IsAllDayEvent) 
			{
				startEventDateTime = new EventDateTime
				{
					Date = model.Start?.ToString("yyyy-MM-dd"),
					TimeZone = timeZone
				};

				endEventDateTime = new EventDateTime
				{
					Date = model.Start?.AddDays(1).ToString("yyyy-MM-dd"), // End date is one day after start date
					TimeZone = timeZone
				};
			}
			else
			{
				startEventDateTime = new EventDateTime
				{
					DateTime = model.Start.Value.ToUniversalTime(),
					TimeZone = timeZone
				};

				endEventDateTime = new EventDateTime
				{
					DateTime = model.End.Value.ToUniversalTime(),
					TimeZone = timeZone
				};
			}
		}
		else
		{
			// Non-annual event
			if (model.IsAllDayEvent)
			{
				startEventDateTime = new EventDateTime
				{
					Date = model.Start?.ToString("yyyy-MM-dd"),
					TimeZone = timeZone
				};

				endEventDateTime = new EventDateTime
				{
					Date = model.Start?.AddDays(1).ToString("yyyy-MM-dd"), // End date is one day after start date
					TimeZone = timeZone
				};
			}
			else
			{
				startEventDateTime = new EventDateTime
				{
					DateTime = model.Start.Value.ToUniversalTime(),
					TimeZone = timeZone
				};

				endEventDateTime = new EventDateTime
				{
					DateTime = model.End.Value.ToUniversalTime(),
					TimeZone = timeZone
				};
			}
		}

		var attendees = model.Attendants?.Any() == true
			? model.Attendants.Select(email => new EventAttendee { Email = email.Trim() }).ToList()
			: null; 

		var newEvent = new Event
		{
            Id = model.EventId,
			Summary = model.Summary,
			Description = model.Description,
			Location = model.Location,
			Visibility = model.Visibility,
			Start = startEventDateTime,
			End = endEventDateTime,
			Recurrence = model.EventType == "annual"
				? new List<string>
				{
			$"RRULE:FREQ=YEARLY;BYMONTH={model.Start?.Month};BYMONTHDAY={model.Start?.Day}"
				}
				: null,
            RecurringEventId = model.RecurringEventId,
			Attendees = attendees
		};

		try
		{
			if (string.IsNullOrEmpty(model.EventId))
			{
				// Creating a new event
				EventsResource.InsertRequest.SendUpdatesEnum sendUpdatesInsert;
				switch (model.SendUpdates)
				{
					case "All":
						sendUpdatesInsert = EventsResource.InsertRequest.SendUpdatesEnum.All;
						break;
					case "ExternalOnly":
						sendUpdatesInsert = EventsResource.InsertRequest.SendUpdatesEnum.ExternalOnly;
						break;
					case "None":
					default:
						sendUpdatesInsert = EventsResource.InsertRequest.SendUpdatesEnum.None;
						break;
				}

				await _googleCalendarService.AddEventAsync(calendarId, newEvent, sendUpdatesInsert);
				return RedirectToAction("ViewNewEventAdded", new { message = "Event created successfully." });
			}
			else
			{
				// Updating an existing event
				EventsResource.UpdateRequest.SendUpdatesEnum sendUpdatesUpdate;
				switch (model.SendUpdates)
				{
					case "All":
						sendUpdatesUpdate = EventsResource.UpdateRequest.SendUpdatesEnum.All;
						break;
					case "ExternalOnly":
						sendUpdatesUpdate = EventsResource.UpdateRequest.SendUpdatesEnum.ExternalOnly;
						break;
					case "None":
					default:
						sendUpdatesUpdate = EventsResource.UpdateRequest.SendUpdatesEnum.None;
						break;
				}

				await _googleCalendarService.UpdateEventAsync(calendarId, model.EventId, newEvent, sendUpdatesUpdate);

				return RedirectToAction("ViewNewEventUpdated", new { calendarId, eventId = model.EventId, message = "Event updated successfully." });
			}
		}
		catch (Google.GoogleApiException ex)
		{
			ModelState.AddModelError("", $"Error from Google API: {ex.Message}");
			return View(model);
		}
		catch (Exception ex)
		{
			ModelState.AddModelError("", $"Error creating/updating event: {ex.Message}");
			return View(model);
		}
	}

	[HttpPost]
    public async Task<IActionResult> DeletePrimaryEvent(DeleteEventViewModel model)
    {
        if (string.IsNullOrEmpty(model.EventId))
        {
            TempData["ErrorMessage"] = "Event ID is missing.";
            TempData["IsSuccess"] = false;
            return RedirectToAction("ConfirmDelete", new { calendarId = model.CalendarId, eventId = model.EventId });
        }

        try
        {
            int deletedInstancesCount =
                await _googleCalendarService.DeleteEventAsync("primary", model.EventId, model.DeleteSeries);

            TempData["IsSuccess"] = true;
            TempData["SuccessMessage"] = model.DeleteSeries
                ? "The recurring event series was deleted successfully."
                : "The event was deleted successfully.";
            TempData["DeletedInstancesCount"] = deletedInstancesCount;
            return RedirectToAction("DeletionConfirmed");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to delete event. Error: {ex.Message}";
            TempData["IsSuccess"] = false;
            return RedirectToAction("ConfirmDelete", new { calendarId = model.CalendarId, eventId = model.EventId });
        }
    }

    public IActionResult DeletionConfirmed(string eventId)
    {
        var successMessage = TempData["SuccessMessage"];
        var deletedInstancesCount =
            TempData["DeletedInstancesCount"] != null ? (int)TempData["DeletedInstancesCount"] : 0;

        if (successMessage != null)
        {
            ViewBag.SuccessMessage = successMessage;
            ViewBag.DeletedInstancesCount = deletedInstancesCount;
            return View();
        }

        return RedirectToAction("ListCalendarsAndEvents");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmDelete(string calendarId, string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            TempData["ErrorMessage"] = "Event ID is missing.";
            return RedirectToAction("ListCalendarsAndEvents");
        }

        try
        {
            var eventToDelete = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId);

            if (eventToDelete == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction("ListCalendarsAndEvents");
            }

            // Store the necessary data in TempData
            TempData["EventId"] = eventToDelete.Id;
            TempData["Summary"] = eventToDelete.Summary;
            TempData["Start"] = eventToDelete.Start.DateTime;
            TempData["End"] = eventToDelete.End.DateTime;
            TempData["Location"] = eventToDelete.Location;

            // Redirect to ConfirmDelete view
            return View(new DeleteEventViewModel
            {
                EventId = eventToDelete.Id,
                Summary = eventToDelete.Summary,
                Start = eventToDelete.Start.DateTime,
                End = eventToDelete.End.DateTime,
                Location = eventToDelete.Location,
                DeleteSeries = false
            });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while retrieving the event details: {ex.Message}";
            return RedirectToAction("ListCalendarsAndEvents");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DuplicateEvent(string calendarId, string eventId)
    {
        try
        {
            // Retrieve the existing event from the specified calendar
            var existingEvent = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId);

            if (existingEvent == null)
            {
                return NotFound($"Event with ID {eventId} not found in calendar {calendarId}.");
            }

            // Create a new event model based on the existing event
            var model = new EventViewModel
            {
                Summary = existingEvent.Summary + " " + "<duplicate>",
                Description = existingEvent.Description,
                Location = existingEvent.Location,
                Visibility = existingEvent.Visibility,
                Start = existingEvent.Start?.DateTimeDateTimeOffset?.DateTime,
                End = existingEvent.End?.DateTimeDateTimeOffset?.DateTime,
                EventType = existingEvent.Start?.Date != null ? "allDay" : "single" // Determine event type
            };

            // Call the method to create the duplicated event in the primary calendar
            var result = await CreateEvent(model);

            return RedirectToAction("ViewNewEventAdded", new { message = "Event duplicated successfully." });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error duplicating event: {ex.Message}");
            return RedirectToAction("Error"); // Redirect to an error page or view if something goes wrong
        }
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

        // for debugging purposes only
        foreach (var evt in matchingEvents)
        {
            Console.WriteLine($"Event ID: {evt.Id}");
            Console.WriteLine($"Creator.Self: {evt.Creator.Self}");
            Console.WriteLine($"GuestsCanModify: {evt.GuestsCanModify}");
            Console.WriteLine($"Source: {evt.Source}");
            Console.WriteLine($"Locked: {evt.Locked}");
            Console.WriteLine($"Transparency: {evt.Transparency}");
        }

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
            Start = e.Start.DateTimeDateTimeOffset?.ToDateTime(),
            End = e.End.DateTimeDateTimeOffset?.ToDateTime(),
            Location = e.Location,
            CalendarId = e.Organizer?.Email!,
            IsCreator = e.Creator?.Self ?? false,
            GuestsCanModify = e.GuestsCanModify ?? false,
            Status = e.Status,
            Source = e.Source?.ToString() ?? string.Empty,
            Locked = e.Locked ?? false,
            Transparency = e.Transparency ?? string.Empty
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
    public async Task<IActionResult> EditPrimaryEvent(string eventId)
    {
        var eventToEdit = await _googleCalendarService.GetEventByIdAsync("primary", eventId);

        if (eventToEdit == null)
        {
            return NotFound($"Event with ID {eventId} not found.");
        }

        var viewModel = new EventViewModel
        {
            Summary = eventToEdit.Summary,
            Description = eventToEdit.Description,
            Location = eventToEdit.Location,
            Start = eventToEdit.Start.DateTimeDateTimeOffset?.ToDateTime(),
            End = eventToEdit.End.DateTimeDateTimeOffset?.ToDateTime()
        };

        ViewBag.PageTitle = "Edit Event";
        ViewBag.FormAction = "EditPrimaryEvent";
        ViewBag.ButtonText = "Save Changes";
        ViewBag.EventId = eventId;

        return View("CreateEvent", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditPrimaryEvent(EventViewModel model, string eventId)
    {
        if (ModelState.IsValid)
        {
            var updatedEvent = new Event
            {
                Id = eventId, // Ensure the ID is set for updating
                Summary = model.Summary,
                Description = model.Description,
                Location = model.Location,
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = model.Start,
                    TimeZone = "Europe/Sofia"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = model.End,
                    TimeZone = "Europe/Sofia"
                }
            };

            try
            {
                await _googleCalendarService.AddEventAsync("primary", eventId, updatedEvent);
                return RedirectToAction("ViewNewEventAdded");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating event: {ex.Message}");
            }
        }

        // Redisplay form with errors
        ViewBag.PageTitle = "Edit Event";
        ViewBag.FormAction = "EditPrimaryEvent";
        ViewBag.ButtonText = "Save Changes";
        ViewBag.EventId = eventId;

        return View("CreateEvent", model);
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(string calendarId, string eventId)
    {
        // Retrieve the event from the Google Calendar
        var eventToEdit = await _googleCalendarService.GetEventByIdAsync(calendarId, eventId);

        if (eventToEdit == null)
        {
            return NotFound($"Event with ID {eventId} not found.");
        }

        // Initialize variables for Start and End
        DateTime? startDateTime = eventToEdit.Start.DateTimeDateTimeOffset?.ToDateTime();
        DateTime? endDateTime = eventToEdit.End.DateTimeDateTimeOffset?.ToDateTime();

        DateTime? startDate = !string.IsNullOrEmpty(eventToEdit.Start.Date)
            ? DateTime.Parse(eventToEdit.Start.Date)
            : (DateTime?)null;

        DateTime? endDate = !string.IsNullOrEmpty(eventToEdit.End.Date)
            ? DateTime.Parse(eventToEdit.End.Date)
            : (DateTime?)null;

        string eventType;
        if (startDateTime.HasValue && endDateTime.HasValue)
        {
            eventType = eventToEdit.EventType;
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            eventType = "allDay";
            startDateTime = startDate;
            endDateTime = endDate;
        }
        else
        {
            eventType = "unknown";
        }

        // Map the retrieved event to the EditEventViewModel
        var viewModel = new EditEventViewModel
        {
            EventId = eventToEdit.Id,
            CalendarId = calendarId,
            Summary = eventToEdit.Summary,
            Location = eventToEdit.Location,
            Start = startDateTime,
            End = endDateTime,
            IsCreator = eventToEdit.Creator?.Email == "lcfrrr@gmail.com",
            GuestsCanModify = eventToEdit.GuestsCanModify ?? false,
            Status = eventToEdit.Status,
            Source = eventToEdit.Source?.ToString(),
            Locked = eventToEdit.Locked,
            Transparency = eventToEdit.Transparency,
            Visibility = eventToEdit.Visibility,
            EventType = eventType
        };

        // Set the ViewBag properties for the view
        ViewBag.PageTitle = "Edit Event";
        ViewBag.FormAction = "UpdateEvent";
        ViewBag.ButtonText = "Save Changes";

        // Return the view for editing the event
        return View("UpdateEvent", viewModel);
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
                    // Prepare the event to update
                    var eventToUpdate = new Event
                    {
                        Id = eventModel.EventId,
                        Summary = eventModel.Summary,
                        Start = new EventDateTime { DateTime = eventModel.Start },
                        End = new EventDateTime { DateTime = eventModel.End },
                        Location = eventModel.Location
                    };

                    // Update event in the correct calendar
                    await _googleCalendarService.UpdateEventAsync(eventModel.CalendarId, eventModel.EventId,
                        eventToUpdate);
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

        // Retrieve the list of events again to redisplay with errors
        // Assuming you need to show events for the same calendar that was being edited
        // or show the list of all events for a summary search
        var calendarId = events.Values.FirstOrDefault()?.CalendarId;
        var matchingEvents = string.IsNullOrEmpty(calendarId)
            ? await _googleCalendarService.GetEventByIdAcrossAllCalendarsAsync(events.Values.First().Summary)
            : await _googleCalendarService.GetEventsAsync(calendarId);

        var viewModel = matchingEvents.Select(e => new EditEventViewModel
        {
            EventId = e.Id,
            Summary = e.Summary,
            Start = e.Start.DateTime,
            End = e.End.DateTime,
            Location = e.Location,
            CalendarId = e.Organizer?.Email // Adjust this based on how you determine CalendarId
        }).ToList();

        return View("EditEvents", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEvents(Dictionary<string, EditEventViewModel> events)
    {
        if (events == null || !events.Any())
        {
            ModelState.AddModelError("", "No events to update.");
            return View("EditEvents", events.Values); // Return to the same view with the current model
        }

        var updatedEvents = new List<UpdatedEventViewModel>();

        foreach (var eventEntry in events)
        {
            var eventId = eventEntry.Key;
            var eventModel = eventEntry.Value;
            var updatedEventViewModel = new UpdatedEventViewModel
            {
                EventId = eventId,
                CalendarId = eventModel.CalendarId,
                Summary = eventModel.Summary,
                Start = eventModel.Start,
                End = eventModel.End,
                Location = eventModel.Location
            };

            try
            {
                var updatedEvent = new Event
                {
                    Id = eventId,
                    Summary = eventModel.Summary,
                    Start = new EventDateTime { DateTimeDateTimeOffset = eventModel.Start },
                    End = new EventDateTime { DateTimeDateTimeOffset = eventModel.End },
                    Location = eventModel.Location
                };

                await _googleCalendarService.UpdateEventAsync(eventModel.CalendarId, eventId, updatedEvent);

                updatedEventViewModel.IsUpdated = true;
            }
            catch (Exception ex)
            {
                // Handle exceptions for each individual event
                Console.WriteLine(
                    $"Error updating event with ID {eventId}: {ex.Message}"); // input in the console just in case 
                updatedEventViewModel.IsUpdated = false;
                updatedEventViewModel.ErrorMessage = $"Error updating event with ID {eventId}: {ex.Message}";
            }

            updatedEvents.Add(updatedEventViewModel);
        }

        // Pass the list of updated events to the view
        return View("UpdateEvents", updatedEvents);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEvent(EditEventViewModel model)
    {
        // Initialize the Event object
        var updatedEvent = new Event
        {
            Id = model.EventId,
            Summary = model.Summary,
            Description = model.Description,
            Location = model.Location,
            EventType = model.EventType

        };

        // Check if the event is an all-day event
        if (model.EventType == "allDay")
        {
            // For all-day events, set Start.Date and End.Date to the same date
            updatedEvent.Start = new EventDateTime
            {
                Date = model.Start?.ToString("yyyy-MM-dd"),
                TimeZone = "Europe/Sofia"
            };
            updatedEvent.End = new EventDateTime
            {
                Date = model.Start?.ToString("yyyy-MM-dd"), // Use Start date as End date for all-day events
                TimeZone = "Europe/Sofia"
            };
        }
        else
        {
            // For timed events, set Start.DateTime and End.DateTime
            updatedEvent.Start = new EventDateTime
            {
                DateTime = model.Start.HasValue ? model.Start.Value.ToUniversalTime() : DateTime.UtcNow,
                TimeZone = "Europe/Sofia"
            };
            updatedEvent.End = new EventDateTime
            {
                DateTime = model.End.HasValue ? model.End.Value.ToUniversalTime() : DateTime.UtcNow,
                TimeZone = "Europe/Sofia"
            };
        }

        try
        {
            // Update the event using the Google Calendar service
            await _googleCalendarService.UpdateEventAsync(model.CalendarId, model.EventId, updatedEvent);

            // Redirect to a confirmation page after successful update
            return RedirectToAction("ViewNewEventUpdated",
                new { message = "Event updated successfully.", model.CalendarId, model.EventId });
        }
        catch (Exception ex)
        {
            // Handle errors during the update process
            ModelState.AddModelError("", $"Error updating event: {ex.Message}");
        }

        // Redisplay the form with errors if the update fails
        ViewBag.PageTitle = "Edit Event";
        ViewBag.FormAction = "UpdateEvent";
        ViewBag.ButtonText = "Save Changes";

        return View("UpdateEvent", model);
    }

    public async Task<IList<Event>> GetTodaysEventsAsync()
    {
        var calendarList = await _googleCalendarService.GetAllCalendarsAsync();
        var allEvents = new List<Event>();
        var now = DateTime.UtcNow;
        var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        // Retrieve the primary calendar's time zone
        var primaryCalendarTimeZone = await _googleCalendarService.GetPrimaryCalendarTimeZoneAsync();
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(primaryCalendarTimeZone);

        foreach (var calendar in calendarList)
        {
            var todayEvents = await _googleCalendarService.GetEventsAsync(calendar.Id);

            var filteredEvents = todayEvents.Where(e =>
            {
                if (e.Start.DateTime.HasValue)
                {
                    // Filter events with specific start and end times
                    var eventStart = e.Start.DateTime.Value;
                    var eventEnd = e.End.DateTime.HasValue ? e.End.DateTime.Value : eventStart;

                    // Ensure DateTime is in UTC only if necessary
                    if (eventStart.Kind != DateTimeKind.Utc)
                    {
                        eventStart = DateTime.SpecifyKind(eventStart, DateTimeKind.Utc);
                    }
                    if (eventEnd.Kind != DateTimeKind.Utc)
                    {
                        eventEnd = DateTime.SpecifyKind(eventEnd, DateTimeKind.Utc);
                    }

                    return eventStart < endOfDay && eventEnd >= startOfDay;
                }
                else if (!string.IsNullOrEmpty(e.Start.Date))
                {
                    // Handle all-day events
                    var eventStartDate = DateTime.Parse(e.Start.Date).Date;
                    var eventEndDate = e.End.Date != null ? DateTime.Parse(e.End.Date).Date : eventStartDate.AddDays(1);

                    return eventStartDate <= endOfDay && eventEndDate > startOfDay;
                }

                return false;
            });

            // Convert event times to local time zone if needed
            foreach (var e in filteredEvents)
            {
                if (e.Start.DateTimeDateTimeOffset.HasValue)
                {
                    
                    if (e.Start.DateTime.Value.Kind == DateTimeKind.Utc) // if the returned from the API times are UTC convert them to local time 
                    {
                        e.Start.DateTimeDateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(e.Start.DateTime.Value, timeZoneInfo);
                    }

                    if (e.End.DateTimeDateTimeOffset.HasValue && e.End.DateTime.Value.Kind == DateTimeKind.Utc) // if the returned from the API times are UTC convert them to local time 
                    {
                        e.End.DateTimeDateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(e.End.DateTime.Value, timeZoneInfo);
                    }
                }
            }

            allEvents.AddRange(filteredEvents);
        }

        return allEvents.OrderBy(e => e.Start.DateTimeDateTimeOffset ?? DateTime.Parse(e.Start.Date)).ToList();
    }

    public async Task<IList<Event>> GetNextWeeksEventsAsync()
    {
        var calendarList = await _googleCalendarService.GetAllCalendarsAsync();
        var allEvents = new List<Event>();
        var now = DateTime.UtcNow;

        // Calculate the start of next week (assumed to be Monday) and the end of the week (Sunday)
        var daysUntilNextMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7; // Find how many days from today until next Monday
        DateTime startOfNextWeek;

		if (daysUntilNextMonday == 0)
        {
	        startOfNextWeek = now.AddDays(7).Date; // Next Monday (start of next week)

		}
        else
        {
	        startOfNextWeek = now.AddDays(daysUntilNextMonday).Date; 
		}

		DateTime endOfNextWeek = startOfNextWeek.AddDays(7).AddTicks(-1); // End of next week (Sunday 23:59:59)

        // Retrieve the primary calendar's time zone
        var primaryCalendarTimeZone = await _googleCalendarService.GetPrimaryCalendarTimeZoneAsync();
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(primaryCalendarTimeZone);

        foreach (var calendar in calendarList)
        {
            var nextWeekEvents = await _googleCalendarService.GetEventsAsync(calendar.Id);

            var filteredEvents = nextWeekEvents.Where(e =>
            {
                if (e.Start.DateTime.HasValue)
                {
                    // Filter events with specific start and end times
                    var eventStart = e.Start.DateTime.Value;
                    var eventEnd = e.End.DateTime.HasValue ? e.End.DateTime.Value : eventStart;

                    // Ensure DateTime is in UTC only if necessary
                    if (eventStart.Kind != DateTimeKind.Utc)
                    {
                        eventStart = DateTime.SpecifyKind(eventStart, DateTimeKind.Utc);
                    }
                    if (eventEnd.Kind != DateTimeKind.Utc)
                    {
                        eventEnd = DateTime.SpecifyKind(eventEnd, DateTimeKind.Utc);
                    }

                    return eventStart < endOfNextWeek && eventEnd >= startOfNextWeek;
                }
                else if (!string.IsNullOrEmpty(e.Start.Date))
                {
                    // Handle all-day events
                    var eventStartDate = DateTime.Parse(e.Start.Date).Date;
                    var eventEndDate = e.End.Date != null ? DateTime.Parse(e.End.Date).Date : eventStartDate.AddDays(1);

                    return eventStartDate <= endOfNextWeek && eventEndDate > startOfNextWeek;
                }

                return false;
            });

            // Convert event times to local time zone if needed
            foreach (var e in filteredEvents)
            {
                if (e.Start.DateTime.HasValue)
                {
                    if (e.Start.DateTime.Value.Kind == DateTimeKind.Utc) // Convert to local time if needed
                    {
                        e.Start.DateTimeDateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(e.Start.DateTime.Value, timeZoneInfo);
                    }

                    if (e.End.DateTime.HasValue && e.End.DateTime.Value.Kind == DateTimeKind.Utc) // Convert to local time if needed
                    {
                        e.End.DateTimeDateTimeOffset = TimeZoneInfo.ConvertTimeFromUtc(e.End.DateTime.Value, timeZoneInfo);
                    }
                }
            }

            allEvents.AddRange(filteredEvents);
        }

        return allEvents.OrderBy(e => e.Start.DateTimeDateTimeOffset ?? DateTime.Parse(e.Start.Date)).ToList();
    }

	public async Task<IActionResult> ViewTodaysAndNextWeeksEvents()
	{
		var todaysEvents = await GetTodaysEventsAsync();
		var nextWeekEvents = await GetNextWeeksEventsAsync();
		var nextWeeksEvents = nextWeekEvents.ToList();
		var nextWeeksAttendees = new List<EventAttendee>();

		foreach (var ev in nextWeeksEvents)
		{
			if (ev.Attendees != null)
			{
				nextWeeksAttendees.AddRange(ev.Attendees);
			}

		}

		ViewBag.NextWeekEvents = nextWeeksEvents;
		ViewBag.Attendees = nextWeeksAttendees;

		return View(todaysEvents);
	}

	[HttpGet]
    public IActionResult CreateNewCalendar()
    {
        var localTimeZoneId = TimeZoneInfo.Local.Id;

        // Populate the time zones
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new SelectListItem
            {
                Value = tz.Id,
                Text = tz.DisplayName,
                Selected = tz.Id == localTimeZoneId // Pre-select the local time zone
            })
            .ToList();

        ViewBag.TimeZones = timeZones;
        ViewBag.SelectedTimeZone = localTimeZoneId; 

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewCalendar(string calendarName, string timeZone, string? description = null)
    {
        var localTimeZoneId = TimeZoneInfo.Local.Id;


        if (string.IsNullOrWhiteSpace(calendarName) || string.IsNullOrWhiteSpace(timeZone))
        {
            ViewBag.Message = "Calendar Name and Time Zone are required.";
            ViewBag.MessageClass = "alert-danger"; // Set the error message style
            ViewBag.CalendarNameClass = string.IsNullOrWhiteSpace(calendarName) ? "is-invalid" : "";
            ViewBag.TimeZoneClass = string.IsNullOrWhiteSpace(timeZone) ? "is-invalid" : "";

            // Re-populate time zones
            ViewBag.TimeZones = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new SelectListItem
                {
                    Value = tz.Id,
                    Text = tz.DisplayName,
                    Selected = tz.Id == localTimeZoneId
                })
                .ToList();

            return View();
        }

        try
        {
            
            string ianaTimeZone = TimeZoneConverter.ConvertToIanaTimeZone(timeZone);

            
            var newCalendar = await _googleCalendarService.CreateCalendarAsync(calendarName, ianaTimeZone, description);

            // Set success message and calendar details
            ViewBag.Message = $"New Calendar '{newCalendar.Summary}' Created Successfully!";
            ViewBag.MessageClass = "alert-success"; // Success message style
            ViewBag.ShowSuccessMessage = true; // Show success message

            // Set the newly created calendar details
            ViewBag.NewCalendarName = newCalendar.Summary;
            ViewBag.NewCalendarTimeZone = newCalendar.TimeZone;
            ViewBag.NewCalendarDescription = string.IsNullOrWhiteSpace(newCalendar.Description) ? "N/A" : newCalendar.Description;
        }
        catch (Exception ex)
        {
            ViewBag.Message = "Error creating calendar: " + ex.Message;
            ViewBag.MessageClass = "alert-danger"; // Error message style
        }


        // Re-populate time zones
        var timeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new SelectListItem
            {
                Value = tz.Id,
                Text = tz.DisplayName,
                Selected = tz.Id == localTimeZoneId // Pre-select the local time zone
            })
            .ToList();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCalendar(string calendarId)
    {
	    try
	    {
		    if (string.IsNullOrEmpty(calendarId))
		    {
				TempData["DeleteSuccessMessage"] = "Invalid calendar ID.";
				TempData["MessageClass"] = "alert-danger";

				return RedirectToAction("ListCalendars");
		    }

		    var calendar = await _googleCalendarService.GetCalendarByIdAsync(calendarId);
		    var isDeleted = await _googleCalendarService.DeleteCalendarAsync(calendarId);

			if (isDeleted)
			{
				TempData["MessageClass"] = "alert-success";
				TempData["DeleteSuccessMessage"] = $"{calendar.Summary} deleted successfully!";
			}
			else
			{
				TempData["MessageClass"] = "alert-danger";
				TempData["DeleteSuccessMessage"] = $"Failed to delete calendar {calendar.Summary}.";
			}

		}
	    catch (Exception ex)
	    {
		    Console.WriteLine(ex.Message); // for debugging purposes 
			TempData["MessageClass"] = "alert-danger";
			TempData["DeleteSuccessMessage"] = $"Error deleting calendar: {ex.Message}";
	    }

	    return RedirectToAction("ListCalendars");

    }
}