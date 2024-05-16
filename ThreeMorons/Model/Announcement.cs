namespace ThreeMorons.Model
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string ShortDescription { get; set; }
        public string? TargetAudience { get; set; }
    }
}
