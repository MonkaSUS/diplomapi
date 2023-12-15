using FluentValidation;
using ThreeMorons.Model;

namespace ThreeMorons.Validators
{
    public class RegistrationValidator : AbstractValidator<User>
    {
        public RegistrationValidator() 
        {
            RuleFor(u => u.Login).MinimumLength(5).MaximumLength(20).Matches("[A-Za-z0-9]+").WithMessage("Логин должен быть 5-20 символов длиной и состоять только из цифр и латинских букв"); ;
            RuleFor(u => u.Password).MinimumLength(8).MaximumLength(16)
                                    .Matches("/^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*\\W)(?!.* ).{8,16}$/")
                                    .WithMessage("Пароль должен содержать хотя бы одно число от 0 до 9, хотя бы одну маленькую буквук, одну большую букву, один спец. символ, не содержать пробелов, и быть 8-16 символов в длину");
        }
    }
}
