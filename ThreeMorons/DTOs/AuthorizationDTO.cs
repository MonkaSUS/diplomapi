namespace ThreeMorons.UserInputTypes
{
    /// <summary>
    /// Рекорд, содержащий все поля, необходимые для авторизации пользователя
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    public record AuthorizationInput(string login, string password);
    
}
