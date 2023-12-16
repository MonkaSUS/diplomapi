using System.ComponentModel.DataAnnotations;

namespace ThreeMorons.UserInputTypes
{
    /// <summary>
    /// Рекорд, содержащий все поля, необходимые для регистрации нового пользователя и соответствующую валидацию для них.
    /// </summary>
    public record RegistrationInput(string login, string password, string name, string surname, string patronymic, int UserClassId);
    public record AuthorizationInput(string login, string password);
    
}
