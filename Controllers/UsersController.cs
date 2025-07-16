using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartTaskTracker.Models;

namespace SmartTaskTracker.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public UsersController(UserManager<AppUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var list = new List<UserRoleViewModel>();
        foreach (var u in _userManager.Users)
        {
            var role = (await _userManager.GetRolesAsync(u)).FirstOrDefault() ?? string.Empty;
            list.Add(new UserRoleViewModel { Id = u.Id, UserName = u.UserName ?? string.Empty, Role = role });
        }
        return View(list);
    }

    public IActionResult Create()
    {
        ViewData["Roles"] = new SelectList(_roleManager.Roles.Select(r => r.Name));
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new AppUser { UserName = model.UserName };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction(nameof(Index));
            }
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
        }
        ViewData["Roles"] = new SelectList(_roleManager.Roles.Select(r => r.Name), model.Role);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
        var vm = new UserRoleViewModel { Id = user.Id, UserName = user.UserName ?? string.Empty, Role = role };
        ViewData["Roles"] = new SelectList(_roleManager.Roles.Select(r => r.Name), vm.Role);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null) return NotFound();
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        return RedirectToAction(nameof(Index));
    }
}
