namespace ThreeMorons.DTOs
{
    public class ConnectionStringDTO
    {
        public Dictionary<string, string> data { get; set; } = new();
        public string dbms_name { get; set; }
    }
}
