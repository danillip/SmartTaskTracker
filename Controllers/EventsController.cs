using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskTracker.Data;
using SmartTaskTracker.Models;

namespace SmartTaskTracker.Controllers;

public class EventsController : Controller
{
    private readonly ApplicationDbContext _context;
    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Events
    public async Task<IActionResult> Index()
    {
        return View(await _context.Events.ToListAsync());
    }

    // GET: Events/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var ev = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,StartUtc,EndUtc")] Event ev)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ev);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,StartUtc,EndUtc")] Event ev)
    {
        if (id != ev.Id) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(ev);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(ev);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var ev = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev != null)
        {
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<FileContentResult> Ics(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
        {
            return File(Array.Empty<byte>(), "text/plain");
        }

        var ics = new StringBuilder()
            .AppendLine("BEGIN:VCALENDAR")
            .AppendLine("VERSION:2.0")
            .AppendLine("PRODID:-//SmartTaskTracker//EN")
            .AppendLine("BEGIN:VEVENT")
            .AppendLine($"UID:{Guid.NewGuid()}")
            .AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}")
            .AppendLine($"DTSTART:{ev.StartUtc:yyyyMMddTHHmmssZ}")
            .AppendLine($"DTEND:{ev.EndUtc:yyyyMMddTHHmmssZ}")
            .AppendLine($"SUMMARY:{ev.Title}")
            .AppendLine("END:VEVENT")
            .AppendLine("END:VCALENDAR")
            .ToString();
        return File(Encoding.UTF8.GetBytes(ics), "text/calendar", $"{ev.Title}.ics");
    }
}
