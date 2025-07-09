using System;
using SmartTaskTracker.Models;
using Xunit;

namespace SmartTaskTracker.Tests;

public class TaskItemTests
{
    [Fact]
    public void PastDeadlineSetsStatusDone()
    {
        var task = new TaskItem { DeadlineUtc = DateTime.UtcNow.AddDays(-1) };
        task.UpdateStatusByDeadline(DateTime.UtcNow);
        Assert.Equal(Models.TaskStatus.Done, task.Status);
    }
}
