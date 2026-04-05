using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/todos")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TodosController(AppDbContext db)
        {
            _db = db;
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return Guid.Parse(claim);
        }

        private TodoResponse MapToResponse(TodoItem todo) => new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Details = todo.Details,
            Priority = todo.Priority,
            DueDate = todo.DueDate,
            IsCompleted = todo.IsCompleted,
            IsPublic = todo.IsPublic,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt
        };

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string status = "all",
            [FromQuery] string? priority = null,
            [FromQuery] DateOnly? dueFrom = null,
            [FromQuery] DateOnly? dueTo = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortDir = "desc",
            [FromQuery] string? search = null)
        {
            var query = _db.Todos.Where(t => t.IsPublic);
            query = ApplyFilters(query, status, priority, dueFrom, dueTo, search);
            query = ApplySort(query, sortBy, sortDir);
            return Ok(await ToPagedResponse(query, page, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTodos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string status = "all",
            [FromQuery] string? priority = null,
            [FromQuery] DateOnly? dueFrom = null,
            [FromQuery] DateOnly? dueTo = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortDir = "desc",
            [FromQuery] string? search = null)
        {
            var userId = GetUserId();
            var query = _db.Todos.Where(t => t.UserId == userId);
            query = ApplyFilters(query, status, priority, dueFrom, dueTo, search);
            query = ApplySort(query, sortBy, sortDir);
            return Ok(await ToPagedResponse(query, page, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTodoRequest request)
        {
            var userId = GetUserId();

            var todo = new TodoItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Details = request.Details,
                Priority = request.Priority,
                DueDate = request.DueDate,
                IsPublic = request.IsPublic,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Todos.Add(todo);
            await _db.SaveChangesAsync();
            return StatusCode(201, MapToResponse(todo));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null) return NotFound();
            if (todo.UserId != userId) return Forbid();
            return Ok(MapToResponse(todo));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoRequest request)
        {
            var userId = GetUserId();
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null) return NotFound();
            if (todo.UserId != userId) return Forbid();

            todo.Title = request.Title;
            todo.Details = request.Details;
            todo.Priority = request.Priority;
            todo.DueDate = request.DueDate;
            todo.IsPublic = request.IsPublic;
            todo.IsCompleted = request.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(MapToResponse(todo));
        }

        [HttpPatch("{id}/completion")]
        public async Task<IActionResult> SetCompletion(Guid id, [FromBody] SetCompletionRequest request)
        {
            var userId = GetUserId();
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null) return NotFound();
            if (todo.UserId != userId) return Forbid();

            todo.IsCompleted = request.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(MapToResponse(todo));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var todo = await _db.Todos.FindAsync(id);

            if (todo == null) return NotFound();
            if (todo.UserId != userId) return Forbid();

            _db.Todos.Remove(todo);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private IQueryable<TodoItem> ApplyFilters(
            IQueryable<TodoItem> query,
            string status, string? priority,
            DateOnly? dueFrom, DateOnly? dueTo, string? search)
        {
            if (status == "active") query = query.Where(t => !t.IsCompleted);
            else if (status == "completed") query = query.Where(t => t.IsCompleted);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (dueFrom.HasValue) query = query.Where(t => t.DueDate >= dueFrom);
            if (dueTo.HasValue) query = query.Where(t => t.DueDate <= dueTo);

            if (!string.IsNullOrEmpty(search))
            {
                var lower = search.ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(lower) ||
                    (t.Details != null && t.Details.ToLower().Contains(lower)));
            }
            return query;
        }

        private IQueryable<TodoItem> ApplySort(IQueryable<TodoItem> query, string sortBy, string sortDir)
        {
            return (sortBy, sortDir) switch
            {
                ("dueDate", "asc") => query.OrderBy(t => t.DueDate),
                ("dueDate", "desc") => query.OrderByDescending(t => t.DueDate),
                ("priority", "asc") => query.OrderBy(t => t.Priority),
                ("priority", "desc") => query.OrderByDescending(t => t.Priority),
                ("title", "asc") => query.OrderBy(t => t.Title),
                ("title", "desc") => query.OrderByDescending(t => t.Title),
                (_, "asc") => query.OrderBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };
        }

        private async Task<PagedResponse<TodoResponse>> ToPagedResponse(
            IQueryable<TodoItem> query, int page, int pageSize)
        {
            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(page, 1);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToResponse(t))
                .ToListAsync();

            return new PagedResponse<TodoResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}