using System;
using System.Collections.Generic;

namespace TodoApi.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<TodoItem> Todos { get; set; } = new();
    }
}
