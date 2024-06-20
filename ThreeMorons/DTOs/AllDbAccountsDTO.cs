namespace ThreeMorons.DTOs
{
    public class DbAccount
    {
        public int account_id { get; set; }
        public int account_user_id { get; set; }
        public string account_login { get; set; }
        public string account_password { get; set; }
        public int account_type_id { get; set; }

    }
}
