namespace ThreeMorons.DTOs
{
    public record AnnouncementDTO(string title, DateTime createdOn, string body, string author, string shortDescription, string? targetAudience);
}
