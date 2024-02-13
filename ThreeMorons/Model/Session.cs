namespace ThreeMorons.Model
{
    public partial class Session
    {
        public Guid id { get; set; }
        public string JWTToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool IsValid { get; set; }
    }
}
