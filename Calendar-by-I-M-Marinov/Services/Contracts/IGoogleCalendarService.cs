using Google.Apis.Calendar.v3.Data;

namespace Calendar_by_I_M_Marinov.Services.Contracts
{
    public interface IGoogleCalendarService
    {
        public Task<IList<CalendarListEntry>> GetCalendarsAsync();
        public Task<IList<Event>> GetEventsAsync(string calendarId);
	    public Task<Event> AddEventAsync(Event newEvent);

    }


}
