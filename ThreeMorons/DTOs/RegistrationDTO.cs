namespace ThreeMorons.UserInputTypes
{
    /// <summary>
    /// Рекорд, содержащий все поля, необходимые для регистрации нового пользователя.
    /// </summary>
    public record RegistrationInput(string login, string password, string name, string surname, string patronymic, int UserClassId);



    
    
}
