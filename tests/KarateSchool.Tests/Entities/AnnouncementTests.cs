using KarateSchool.Web.Models.Entities;
using Xunit;

namespace KarateSchool.Tests.Entities;

public class AnnouncementTests
{
    private static Administrator CreateAdmin() =>
        new("Announce Admin", "announce-admin@example.com", "hash", "555-0000", "Full");

    [Fact]
    public void Constructor_NullAdministrator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Announcement(null!, "Title", "Content", DateTime.UtcNow.Date));
    }

    [Fact]
    public void Constructor_EmptyTitle_ThrowsArgumentException()
    {
        var admin = CreateAdmin();
        Assert.Throws<ArgumentException>(() =>
            new Announcement(admin, "", "Content", DateTime.UtcNow.Date));
    }

    [Fact]
    public void Constructor_EmptyContent_ThrowsArgumentException()
    {
        var admin = CreateAdmin();
        Assert.Throws<ArgumentException>(() =>
            new Announcement(admin, "Title", "", DateTime.UtcNow.Date));
    }

    [Fact]
    public void Constructor_Valid_SetsProperties()
    {
        var admin = CreateAdmin();
        var announcement = new Announcement(admin, "Title", "Content", DateTime.UtcNow.Date);

        Assert.Equal("Title", announcement.Title);
        Assert.Equal("Content", announcement.Content);
        Assert.Equal(admin.UserId, announcement.AdminId);
    }

    [Fact]
    public void ToString_IncludesTitle()
    {
        var admin = CreateAdmin();
        var announcement = new Announcement(admin, "Welcome!", "Content", DateTime.UtcNow.Date);

        Assert.Contains("Welcome!", announcement.ToString());
    }
}
