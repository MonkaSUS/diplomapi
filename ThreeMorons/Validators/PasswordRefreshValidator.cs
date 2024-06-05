using ThreeMorons.DTOs;

namespace ThreeMorons.Validators
{
    public class PasswordRefreshValidator : AbstractValidator<PasswordRefreshDTO>
    {
        public PasswordRefreshValidator()
        {
            RuleFor(x => x.newPassword).MinimumLength(8).MaximumLength(16)
                                    .Matches("^(?:(?:(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]))|(?:(?=.*[a-z])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[A-Z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))|(?:(?=.*[0-9])(?=.*[a-z])(?=.*[*.!@$%^&(){}[]:;<>,.?/~_+-=|\\]))).{8,32}$")
                                    .WithMessage("Пароль должен содержать хотя бы одно число от 0 до 9, хотя бы одну маленькую буквук, одну большую букву, один спец. символ, не содержать пробелов, и быть 8-16 символов в длину");
        }
    }
}
