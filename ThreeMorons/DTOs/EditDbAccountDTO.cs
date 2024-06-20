namespace ThreeMorons.DTOs
{
    public record EditDbAccountDTO(string user_telegram_id, string account_login, string account_password, string new_account_login,  string new_account_password, string dbms_name);
    
}
