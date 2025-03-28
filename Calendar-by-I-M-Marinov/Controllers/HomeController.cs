﻿using Calendar_by_I_M_Marinov.Models.Calendar;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Calendar_by_I_M_Marinov.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Error(string message)
		{
			ViewBag.ErrorMessage = message;
			return View();
		}
	}
}