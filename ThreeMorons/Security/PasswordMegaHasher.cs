namespace ThreeMorons.SecurityThings
{
    public static class PasswordMegaHasher
    {
        private static KeyDerivationPrf pRandomFunction = KeyDerivationPrf.HMACSHA256;



        /// <summary>
        /// Метод, используемый для создания новой пары пароль+соль при регистрации.
        /// </summary>
        /// <param name="inp">Пароль, который необходимо хешировать</param>
        /// <returns>Тупл, первый элемент - хешированный пароль, второй - соль которая к нему была применена</returns>
        public static (string hashpass, byte[] salt) HashPass(string inp)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inp,
                salt: salt,
                prf: pRandomFunction,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return (hashed, salt);
        }
        /// <summary>
        /// Метод, используемый при авторизации пользователя. На основе существующей соли хеширует пароль тем же алгоритмом, что использовался при регистрации.
        /// </summary>
        /// <param name="inp">Пароль пользователя</param>
        /// <param name="salt">Соль, которую пользователь получил при регистриации в формате UTF8.</param>
        /// <returns>Пароль, хешированный с использованием существующей соли</returns>
        public static string HashPass(string inp, byte[] Salt)
        {
            byte[] salt = Salt;
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inp,
                salt: salt,
                prf: pRandomFunction,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }

    }

}
