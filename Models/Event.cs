using System;
using System.Collections.Generic;

namespace SmartTaskTracker.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
