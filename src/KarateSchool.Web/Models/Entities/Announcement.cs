namespace KarateSchool.Web.Models.Entities;

public class Announcement
{
    public int AnnouncementId { get; private set; }

    public int AdminId { get; private set; }
    public Administrator Administrator { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime DatePosted { get; private set; }

    private Announcement()
    {
    }

    public Announcement(Administrator administrator, string title, string content, DateTime datePosted)
    {
        Administrator = administrator ?? throw new ArgumentNullException(nameof(administrator));
        AdminId = administrator.UserId;

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        Title = title.Trim();

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required.", nameof(content));
        Content = content.Trim();

        DatePosted = datePosted;
    }

    public override string ToString() => $"{DatePosted:yyyy-MM-dd} - {Title}";
}
