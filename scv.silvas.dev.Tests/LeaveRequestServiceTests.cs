using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Tests;

public class LeaveRequestServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private readonly LeaveRequestService _sut;

    public LeaveRequestServiceTests()
    {
        _sut = new LeaveRequestService(_factory);

        using var db = _factory.CreateDbContext();
        db.Users.Add(new User { Id = 1, Username = "dev1", FullName = "Developer One" });
        db.SaveChanges();
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_ForcesStatusToPending()
    {
        var request = await _sut.CreateLeaveRequestAsync(new LeaveRequest
        {
            UserId = 1,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(2),
            Status = "Approved" // moet genegeerd worden
        });

        Assert.Equal("Pending", request.Status);
    }

    [Fact]
    public async Task GetPendingLeaveRequestsAsync_ReturnsOnlyPendingRequests()
    {
        var pending = await _sut.CreateLeaveRequestAsync(new LeaveRequest { UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today });
        var toApprove = await _sut.CreateLeaveRequestAsync(new LeaveRequest { UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today });
        await _sut.ApproveLeaveRequestAsync(toApprove.Id, "Manager");

        var result = await _sut.GetPendingLeaveRequestsAsync();

        var single = Assert.Single(result);
        Assert.Equal(pending.Id, single.Id);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_SetsStatusAndApprover()
    {
        var request = await _sut.CreateLeaveRequestAsync(new LeaveRequest { UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today });

        var result = await _sut.ApproveLeaveRequestAsync(request.Id, "Manager");

        Assert.True(result);
        var all = await _sut.GetLeaveRequestsByUserAsync(1);
        var updated = Assert.Single(all);
        Assert.Equal("Approved", updated.Status);
        Assert.Equal("Manager", updated.ApprovedBy);
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_SetsStatusAndReason()
    {
        var request = await _sut.CreateLeaveRequestAsync(new LeaveRequest { UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today });

        var result = await _sut.RejectLeaveRequestAsync(request.Id, "Te druk");

        Assert.True(result);
        var all = await _sut.GetLeaveRequestsByUserAsync(1);
        var updated = Assert.Single(all);
        Assert.Equal("Rejected", updated.Status);
        Assert.Equal("Te druk", updated.RejectionReason);
    }

    [Fact]
    public async Task GetTotalLeaveDaysAsync_OnlyCountsApprovedRequestsInGivenYear()
    {
        var approvedThisYear = await _sut.CreateLeaveRequestAsync(new LeaveRequest
        {
            UserId = 1,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 5) // 5 dagen
        });
        await _sut.ApproveLeaveRequestAsync(approvedThisYear.Id, "Manager");

        var pendingThisYear = await _sut.CreateLeaveRequestAsync(new LeaveRequest
        {
            UserId = 1,
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 4, 10)
        });

        var approvedLastYear = await _sut.CreateLeaveRequestAsync(new LeaveRequest
        {
            UserId = 1,
            StartDate = new DateTime(2025, 3, 1),
            EndDate = new DateTime(2025, 3, 10)
        });
        await _sut.ApproveLeaveRequestAsync(approvedLastYear.Id, "Manager");

        var total = await _sut.GetTotalLeaveDaysAsync(1, new DateTime(2026, 1, 1));

        Assert.Equal(5, total);
    }

    [Fact]
    public async Task DeleteLeaveRequestAsync_RemovesRequest()
    {
        var request = await _sut.CreateLeaveRequestAsync(new LeaveRequest { UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today });

        var deleted = await _sut.DeleteLeaveRequestAsync(request.Id);

        Assert.True(deleted);
        Assert.Empty(await _sut.GetLeaveRequestsByUserAsync(1));
    }

    public void Dispose() => _factory.Dispose();
}
