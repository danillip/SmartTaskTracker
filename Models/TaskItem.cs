using System;

namespace SmartTaskTracker.Models;

public enum TaskStatus { Planned, InWork, Done }

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
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
        {
            Status = TaskStatus.Done;
        }
    }
}
