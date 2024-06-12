namespace ThreeMorons.Model
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string ShortDescription { get; set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public string? TargetAudience { get; set; }
    }
}
