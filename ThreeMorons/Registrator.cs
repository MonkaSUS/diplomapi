using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ThreeMorons
{
    public static class PasswordMegaHasher
    {
        public static (string hashpass, string salt) HashPass(string inp)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string StringSalt = salt.ToString();
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inp,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return (hashed, StringSalt);
        }
    }
    /// <summary>
    /// Рекорд, содержащий все поля, необходимые для регистрации нового пользователя и соответствующую валидацию для них.
    /// </summary>
    public record RegistrationInput
    {
        [Required(ErrorMessage = "Это поле необходимо заполнить")]
        [StringLength(20, MinimumLength =5, ErrorMessage = "Длина логина должна составлять от 5 до 20 символов")]
        public string login;
        [Required(ErrorMessage = "Это поле необходимо заполнить")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина пароля должна составлять от 5 до 20 символов")]
        [RegularExpression("[A-Za-z0-9]+\\[\\].*!&\\?\\(\\)@", ErrorMessage = "Пароль должен состоять только из латинских букв, цифр или символов []()!&?@")]
        public string password;
        [RegularExpression("[\u0401\u0451\u0410-\u044f]", ErrorMessage = "Почему ваше имя не на русском?")]
        public string name;
        [RegularExpression("[\u0401\u0451\u0410-\u044f]", ErrorMessage = "Почему ваша фамилия не на русском?")]
        public string surname;
        [RegularExpression("[\u0401\u0451\u0410-\u044f]", ErrorMessage = "Почему ваше отчество не на русском?")]
        public string patronymic;
        public int UserClassId;
        public RegistrationInput(string login, string password, string name, string surname, string patronymic, int UserClassId)
        {
            this.login = login;
            this.password = password;
            this.name = name;
            this.surname = surname;
            this.patronymic = patronymic;
            this.UserClassId = UserClassId;
        }
    }
}
