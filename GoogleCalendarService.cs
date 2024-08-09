using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

public class GoogleCalendarService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GoogleCalendarService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IList<Google.Apis.Calendar.v3.Data.Event>> GetEventsAsync()
    {
        var user = _httpContextAccessor.HttpContext.User;
        var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

        var credential = GoogleCredential.FromAccessToken(token);

        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Caledar-by-I-M-Marinov"
        });

        var request = service.Events.List("primary");
        var events = await request.ExecuteAsync();

        return events.Items;
    }
}