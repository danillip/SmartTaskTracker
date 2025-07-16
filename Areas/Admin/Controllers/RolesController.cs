using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SmartTaskTracker.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class RolesController : Controller
	{
		private readonly RoleManager<IdentityRole<int>> _roleManager;

		public RolesController(RoleManager<IdentityRole<int>> roleManager)
		{
			_roleManager = roleManager;
		}

		// GET: Admin/Roles
		public IActionResult Index()
		{
			var roles = _roleManager.Roles.ToList();
			return View(roles);
		}

		// GET: Admin/Roles/Create
		public IActionResult Create() => View();

		// POST: Admin/Roles/Create
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(string roleName)
		{
			if (!string.IsNullOrWhiteSpace(roleName))
			{
				// создаём именно IdentityRole<int>
				await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
				return RedirectToAction(nameof(Index));
			}

			ModelState.AddModelError("", "Имя роли не может быть пустым");
			return View();
		}
	}
}
