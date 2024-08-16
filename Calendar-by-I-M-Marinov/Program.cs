using Calendar_by_I_M_Marinov.Services.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc().AddRazorPagesOptions(o =>
{
	o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Google authentication
builder.Services.AddAuthentication(options =>
	{
		options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
		options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	})
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
	})
	.AddGoogle(options =>
	{
		var googleAuthNSection = builder.Configuration.GetSection("Google");
		options.ClientId = googleAuthNSection["ClientId"];
		options.ClientSecret = googleAuthNSection["ClientSecret"];
		options.CallbackPath = "/signin-google"; // This is the default callback path used by Google
	});


builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();