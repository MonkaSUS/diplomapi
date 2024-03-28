namespace ThreeMorons.Model
{
    public partial class Session
    {
        public Guid id { get; set; }
        public string JwtToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool IsValid { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
    }
}
