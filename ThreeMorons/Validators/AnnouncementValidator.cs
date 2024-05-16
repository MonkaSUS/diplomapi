using ThreeMorons.DTOs;

namespace ThreeMorons.Validators
{
    public class AnnouncementValidator : AbstractValidator<AnnouncementDTO>
    {
        public AnnouncementValidator()
        {
            RuleFor(x => x.author).NotEmpty().NotNull();
            RuleFor(x => x.body).NotEmpty().NotNull().MinimumLength(2).MaximumLength(500);
            RuleFor(x => x.title).NotNull().NotEmpty();
        }
    }
}
