namespace ThreeMorons.Validators
{
    /// <summary>
    /// Валидатор для данных, которые пользователь предоставялет при регистрации, содержит в себе
    /// ОТВРАТИТЕЛЬНОЕ, ГРЯЗНОЕ, ИЗВРАЩЕНСКОЕ, ОТТАЛКИВАЮЩЕЕ, ВЫЗЫВАЮЩЕЕ РВОТУ, ОТВРАЩАЮЩЕЕ, ПУГАЮЩЕЕ, НАПРЯГАЮЩЕЕ ПОРНО С ИСПОЛЬЗОВАНИЕМ REGEX. 
    /// ОСТАВЬ НАДЕЖДУ ВСЯК СЮДА ВХОДЯЩИЙ
    /// </summary>
    public class RegistrationValidator : AbstractValidator<RegistrationInput>
    {
        public RegistrationValidator() 
        {
            RuleFor(u => u.login).MinimumLength(5).MaximumLength(20).Matches("[A-Za-z0-9]+").WithMessage("Логин должен быть 5-20 символов длиной и состоять только из цифр и латинских букв"); ;
            RuleFor(u => u.password).MinimumLength(8).MaximumLength(16)
                                    .Matches("^(?:(?:(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]))|(?:(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[a-z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))).{8,32}$")
                                    .WithMessage("Пароль должен содержать хотя бы одно число от 0 до 9, хотя бы одну маленькую буквук, одну большую букву, один спец. символ, не содержать пробелов, и быть 8-16 символов в длину");
            RuleFor(u => u.name).MinimumLength(2).Matches("[\\p{IsCyrillic}]").WithMessage("Имя должно быть написано кириллицой");
            RuleFor(u => u.surname).Matches("[\\p{IsCyrillic}]").WithMessage("Фамилия должна быть написана кириллицой");
            RuleFor(u => u.patronymic).Matches("[\\p{IsCyrillic}]").WithMessage("Отчество должно быть написано кириллицой");
        }
    }
}
