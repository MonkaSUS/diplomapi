namespace ThreeMorons.Model
{
    public class DbServiceUser
    {
        public Guid? id { get; set; }
        public string user_login { get; set; }
        public string user_password { get; set; }
        public string telegram_id { get; set; }
        public string db_type { get; set; }
        public string? db_name { get; set; }
    }
}
