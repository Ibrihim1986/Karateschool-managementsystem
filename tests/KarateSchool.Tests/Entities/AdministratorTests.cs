using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class AdministratorTests
{
    private static Administrator CreateAdmin() =>
        new("Test Admin", "test-admin@example.com", "hash", "555-0000", "Full");

    [Fact]
    public void Constructor_EmptyAccessLevel_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Administrator("Test", "admin2@example.com", "hash", "555-0000", ""));
    }

    [Fact]
    public void AdminId_MatchesUserId()
    {
        var admin = CreateAdmin();
        Assert.Equal(admin.UserId, admin.AdminId);
    }

    [Fact]
    public void PostAnnouncement_AddsToAnnouncementsCollection()
    {
        var admin = CreateAdmin();
        var announcement = admin.PostAnnouncement("Title", "Content", DateTime.UtcNow.Date);

        Assert.Single(admin.Announcements);
        Assert.Same(announcement, admin.Announcements.First());
    }

    [Fact]
    public void GenerateReport_IncludesAccessLevelAndAnnouncementCount()
    {
        var admin = CreateAdmin();
        admin.PostAnnouncement("Title", "Content", DateTime.UtcNow.Date);

        var report = admin.GenerateReport();

        Assert.Contains("Full", report);
        Assert.Contains("Announcements posted: 1", report);
    }

    [Fact]
    public void ToString_IncludesAccessLevel()
    {
        var admin = CreateAdmin();
        Assert.Contains("Access: Full", admin.ToString());
    }
}
