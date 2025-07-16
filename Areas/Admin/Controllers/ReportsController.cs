using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskTracker.Data;
using SmartTaskTracker.Models;

namespace SmartTaskTracker.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ReportsController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<AppUser> _userManager;

		public ReportsController(ApplicationDbContext context, UserManager<AppUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<IActionResult> Statistics()
		{
			var vm = new StatisticsViewModel
			{
				TotalUsers = await _userManager.Users.CountAsync(),
				TotalTasks = await _context.TaskItems.CountAsync(),
				// Замените 'Done' на то значение enum, которое у вас означает завершено
				CompletedTasks = await _context.TaskItems.CountAsync(t => t.Status == Models.TaskStatus.Done)
			};
			return View(vm);
		}
	}

	public class StatisticsViewModel
	{
		public int TotalUsers { get; set; }
		public int TotalTasks { get; set; }
		public int CompletedTasks { get; set; }
	}
}
