using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Tests;

public class LeaveRequestModelTests
{
    [Fact]
    public void GetTotalDays_IsInclusiveOfBothEndpoints()
    {
        var request = new LeaveRequest
        {
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 1)
        };

        Assert.Equal(1, request.GetTotalDays());
    }

    [Fact]
    public void GetTotalDays_CountsFullRangeAcrossMultipleDays()
    {
        var request = new LeaveRequest
        {
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 5)
        };

        Assert.Equal(5, request.GetTotalDays());
    }
}
