namespace ThreeMorons.Model
{
    public class DbServiceUser
    {
        public Guid? id { get; set; }
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public string user_login { get; set; }
        public string user_password { get; set; }
        public string telegram_id { get; set; }
        public string db_type { get; set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public string? db_name { get; set; }
    }
}
