namespace ThreeMorons.Validators
{
    /// <summary>
    /// Валидатор для данных, которые пользователь предоставялет при авторизации, содержит в себе
    /// ОТВРАТИТЕЛЬНОЕ, ГРЯЗНОЕ, ИЗВРАЩЕНСКОЕ, ОТТАЛКИВАЮЩЕЕ, ВЫЗЫВАЮЩЕЕ РВОТУ, ОТВРАЩАЮЩЕЕ, ПУГАЮЩЕЕ, НАПРЯГАЮЩЕЕ ПОРНО С ИСПОЛЬЗОВАНИЕМ REGEX. 
    /// ОСТАВЬ НАДЕЖДУ ВСЯК СЮДА ВХОДЯЩИЙ
    /// </summary>
    public class AuthorizationValidator : AbstractValidator<AuthorizationInput> 
    {
        public AuthorizationValidator() 
        {
            RuleFor(u => u.login).MinimumLength(5).MaximumLength(20).Matches("[A-Za-z0-9]+").WithMessage("Логин неправильного формата"); ;
            RuleFor(u => u.password).MinimumLength(8).MaximumLength(16)
                                    .Matches("^(?:(?:(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]))|(?:(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[a-z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))).{8,32}$")
                                    .WithMessage("Пароль содержит запрещённые символы или имеет неправильный формат.");
        }
    }
}
