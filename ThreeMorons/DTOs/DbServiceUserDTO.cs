namespace ThreeMorons.DTOs
{
    public record DbServiceUserDTO(string user_login, string user_password, string db_name);
    public record DbServiceUserLoginDTO(string user_login, string user_password);

}
