using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SmartTaskTracker.Controllers;
using Xunit;

namespace SmartTaskTracker.Tests;

public class AuthorizationTests
{
    [Fact]
    public void DeleteEvent_HasAdminAuthorizeAttribute()
    {
        var method = typeof(EventsController).GetMethod("Delete");
        var attr = method?.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().FirstOrDefault();
        Assert.NotNull(attr);
        Assert.Equal("Admin", attr!.Roles);
    }
}
