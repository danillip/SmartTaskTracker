using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTaskTracker.Models
{
    public enum TaskStatus { Planned, InWork, Done }

    public class TaskItem
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Дедлайн")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DeadlineUtc { get; set; }

        public string? Report { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Planned;

        public int ExecutorId { get; set; }
        public AppUser Executor { get; set; } = null!;

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public void UpdateStatusByDeadline(DateTime now)
        {
            if (DeadlineUtc < now)
                Status = TaskStatus.Done;
        }
    }
}
