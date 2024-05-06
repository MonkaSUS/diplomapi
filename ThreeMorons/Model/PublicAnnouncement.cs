namespace ThreeMorons.Model
{
    public class PublicAnnouncement
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
    }
}
