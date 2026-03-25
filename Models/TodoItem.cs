namespace TodoApi.Models
{
    public class TodoItem
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Details { get; set; }

        public string Priority { get; set; } = "medium";

        public DateOnly? DueDate { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsPublic { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public User? User { get; set; }
    }
}