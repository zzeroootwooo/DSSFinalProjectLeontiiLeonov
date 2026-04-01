using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs
{
    public class TodoResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateOnly? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateTodoRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Details { get; set; }

        [Required]
        [RegularExpression("low|medium|high")]
        public string Priority { get; set; } = string.Empty;

        public DateOnly? DueDate { get; set; }
        public bool IsPublic { get; set; }
    }

    public class UpdateTodoRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Details { get; set; }

        [Required]
        [RegularExpression("low|medium|high")]
        public string Priority { get; set; } = string.Empty;

        public DateOnly? DueDate { get; set; }
        public bool IsPublic { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class SetCompletionRequest
    {
        [Required]
        public bool IsCompleted { get; set; }
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}