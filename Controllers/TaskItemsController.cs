using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTaskTracker.Data;
using SmartTaskTracker.Models;

// Чтобы не путать с System.Threading.Tasks.TaskStatus
using TaskItemStatus = SmartTaskTracker.Models.TaskStatus;

namespace SmartTaskTracker.Controllers
{
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

		// GET: TaskItems
		public async Task<IActionResult> Index(string? search, TaskItemStatus? status, string? sort)
		{
			var q = _context.TaskItems
							.Include(t => t.Event)
							.Include(t => t.Executor)
							.AsQueryable();

			// Обычные юзеры видят только свои задачи
			if (!User.IsInRole("Admin"))
			{
				var uid = int.Parse(_userManager.GetUserId(User)!);
				q = q.Where(t => t.ExecutorId == uid);
			}

			if (!string.IsNullOrWhiteSpace(search))
				q = q.Where(t => t.Title.Contains(search));

			if (status.HasValue)
				q = q.Where(t => t.Status == status.Value);

			q = sort switch
			{
				"deadline" => q.OrderBy(t => t.DeadlineUtc),
				"executor" => q.OrderBy(t => t.Executor.UserName),
				_ => q.OrderBy(t => t.Id),
			};

			return View(await q.ToListAsync());
		}

		// GET: TaskItems/Create
		[Authorize(Roles = "Admin")]
		public IActionResult Create()
		{
			PopulateDropDowns();
			return View();
		}

		// POST: TaskItems/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([Bind("Id,Title,DeadlineUtc,Report,Status,ExecutorId,EventId")] TaskItem item)
		{
			// Снимаем стандартную ошибку по DeadlineUtc и парсим вручную из формы
			var raw = Request.Form["DeadlineUtc"].FirstOrDefault() ?? "";
			ModelState.Remove(nameof(item.DeadlineUtc));
			if (DateTime.TryParseExact(raw,
					new[] { "yyyy-MM-ddTHH:mm", "dd.MM.yyyy HH:mm" },
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dt))
			{
				item.DeadlineUtc = dt;
			}
			else
			{
				ModelState.AddModelError(nameof(item.DeadlineUtc),
					"Неверный формат даты. Используйте выпадающий селектор или ГГГГ-MM-ддTHH:mm.");
			}

			// Снимаем ошибки по навигационным свойствам
			ModelState.Remove(nameof(item.Executor));
			ModelState.Remove(nameof(item.Event));

			if (ModelState.IsValid)
			{
				_context.Add(item);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			PopulateDropDowns(item);
			return View(item);
		}

		// GET: TaskItems/Edit/5
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var item = await _context.TaskItems.FindAsync(id);
			if (item == null) return NotFound();

			PopulateDropDowns(item);
			return View(item);
		}

		// POST: TaskItems/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Title,DeadlineUtc,Report,Status,ExecutorId,EventId")] TaskItem item)
		{
			if (id != item.Id) return NotFound();

			var raw = Request.Form["DeadlineUtc"].FirstOrDefault() ?? "";
			ModelState.Remove(nameof(item.DeadlineUtc));
			if (DateTime.TryParseExact(raw,
					new[] { "yyyy-MM-ddTHH:mm", "dd.MM.yyyy HH:mm" },
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var dt))
			{
				item.DeadlineUtc = dt;
			}
			else
			{
				ModelState.AddModelError(nameof(item.DeadlineUtc),
					"Неверный формат даты. Используйте выпадающий селектор или ГГГГ-MM-ддTHH:mm.");
			}

			ModelState.Remove(nameof(item.Executor));
			ModelState.Remove(nameof(item.Event));

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(item);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.TaskItems.Any(e => e.Id == item.Id))
						return NotFound();
					throw;
				}
				return RedirectToAction(nameof(Index));
			}

			PopulateDropDowns(item);
			return View(item);
		}

		// GET: TaskItems/Delete/5
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

		// POST: TaskItems/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
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

		// GET: TaskItems/Report/5
		[Authorize]
		public async Task<IActionResult> Report(int? id)
		{
			if (id == null) return NotFound();

			var item = await _context.TaskItems
									 .Include(t => t.Event)
									 .Include(t => t.Executor)
									 .FirstOrDefaultAsync(m => m.Id == id);
			if (item == null) return NotFound();

			// Обычные юзеры — только свои задачи
			if (!User.IsInRole("Admin"))
			{
				var uid = int.Parse(_userManager.GetUserId(User)!);
				if (item.ExecutorId != uid) return Forbid();
			}

			return View(item);
		}

		// POST: TaskItems/Report/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Report(int id, [Bind("Report,Status")] TaskItem input)
		{
			// Подгружаем исходный объект с навигационными свойствами
			var item = await _context.TaskItems
									 .Include(t => t.Executor)
									 .Include(t => t.Event)
									 .FirstOrDefaultAsync(m => m.Id == id);
			if (item == null) return NotFound();

			// Проверка прав обычных пользователей
			if (!User.IsInRole("Admin"))
			{
				var uid = int.Parse(_userManager.GetUserId(User)!);
				if (item.ExecutorId != uid) return Forbid();
			}

			// Убираем ошибки валидации по полям, которых нет в форме
			ModelState.Remove(nameof(item.Title));
			ModelState.Remove(nameof(item.DeadlineUtc));
			ModelState.Remove(nameof(item.ExecutorId));
			ModelState.Remove(nameof(item.EventId));
			// Навигационные свойства (на всякий случай)
			ModelState.Remove(nameof(item.Executor));
			ModelState.Remove(nameof(item.Event));

			if (!ModelState.IsValid)
			{
				// Сохраняем введённые значения, чтобы они отобразились
				item.Report = input.Report;
				item.Status = input.Status;
				return View(item);
			}

			// Применяем и сохраняем изменения
			item.Report = input.Report;
			item.Status = input.Status;
			_context.Update(item);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

		private void PopulateDropDowns(TaskItem? item = null)
		{
			ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", item?.EventId);
			ViewData["ExecutorId"] = new SelectList(_context.Users, "Id", "UserName", item?.ExecutorId);
		}
	}
}
