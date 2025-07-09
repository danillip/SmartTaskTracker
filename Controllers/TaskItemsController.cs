using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskTracker.Data;
using SmartTaskTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace SmartTaskTracker.Controllers;

[Authorize]
public class TaskItemsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public TaskItemsController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, Models.TaskStatus? status, string? sort)
    {
        var q = _context.TaskItems
            .Include(t => t.Event)
            .Include(t => t.Executor)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            var uid = int.Parse(_userManager.GetUserId(User)!);
            q = q.Where(t => t.ExecutorId == uid);
        }

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(t => t.Title.Contains(search));

        if (status.HasValue)
            q = q.Where(t => t.Status == status);

        q = sort switch
        {
            "deadline" => q.OrderBy(t => t.DeadlineUtc),
            "executor" => q.OrderBy(t => t.Executor.UserName),
            _ => q.OrderBy(t => t.Id)
        };

        return View(await q.ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title");
        ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "UserName");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,DeadlineUtc,Report,Status,ExecutorId,EventId")] TaskItem item)
    {
        if (ModelState.IsValid)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", item.EventId);
        ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "UserName", item.ExecutorId);
        return View(item);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.TaskItems.FindAsync(id);
        if (item == null) return NotFound();
        ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", item.EventId);
        ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "UserName", item.ExecutorId);
        return View(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,DeadlineUtc,Report,Status,ExecutorId,EventId")] TaskItem item)
    {
        if (id != item.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", item.EventId);
        ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "UserName", item.ExecutorId);
        return View(item);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.TaskItems
            .Include(t => t.Event)
            .Include(t => t.Executor)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.TaskItems.FindAsync(id);
        if (item != null)
        {
            _context.TaskItems.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Report(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.TaskItems
            .Include(t => t.Event)
            .Include(t => t.Executor)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (item == null) return NotFound();
        if (!User.IsInRole("Admin"))
        {
            var uid = int.Parse(_userManager.GetUserId(User)!);
            if (item.ExecutorId != uid) return Forbid();
        }
        return View(item);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Report(int id, [Bind("Report,Status")] TaskItem input)
    {
        var item = await _context.TaskItems.FindAsync(id);
        if (item == null) return NotFound();
        if (!User.IsInRole("Admin"))
        {
            var uid = int.Parse(_userManager.GetUserId(User)!);
            if (item.ExecutorId != uid) return Forbid();
        }
        if (ModelState.IsValid)
        {
            item.Report = input.Report;
            item.Status = input.Status;
            _context.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }
}
