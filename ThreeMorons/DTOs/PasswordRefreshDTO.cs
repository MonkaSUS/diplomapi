namespace ThreeMorons.DTOs
{
    public record PasswordRefreshDTO(string jwtToken, string refreshToken, string oldPassword, string newPassword)
    {
    }
}
