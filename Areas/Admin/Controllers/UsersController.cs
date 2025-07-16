using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTaskTracker.Models;

namespace SmartTaskTracker.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class UsersController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole<int>> _roleManager;

		public UsersController(
			UserManager<AppUser> userManager,
			RoleManager<IdentityRole<int>> roleManager)        // <-- IdentityRole<int>
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public IActionResult Index()
		{
			var users = _userManager.Users.ToList();
			return View(users);
		}

		public async Task<IActionResult> EditRoles(int id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null) return NotFound();

			var model = new EditRolesViewModel
			{
				UserId = user.Id,
				Email = user.Email,
				Roles = _roleManager.Roles
						   .Select(r => new RoleCheckbox
						   {
							   RoleName = r.Name,
							   IsSelected = _userManager.IsInRoleAsync(user, r.Name).Result
						   }).ToList()
			};
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> EditRoles(EditRolesViewModel model)
		{
			var user = await _userManager.FindByIdAsync(model.UserId.ToString());
			if (user == null) return NotFound();

			var current = await _userManager.GetRolesAsync(user);
			var selected = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);

			await _userManager.AddToRolesAsync(user, selected.Except(current));
			await _userManager.RemoveFromRolesAsync(user, current.Except(selected));

			return RedirectToAction(nameof(Index));
		}
	}

	public class EditRolesViewModel
	{
		public int UserId { get; set; }
		public string? Email { get; set; }
		public List<RoleCheckbox> Roles { get; set; } = new();
	}

	public class RoleCheckbox
	{
		public string RoleName { get; set; } = "";
		public bool IsSelected { get; set; }
	}
}
