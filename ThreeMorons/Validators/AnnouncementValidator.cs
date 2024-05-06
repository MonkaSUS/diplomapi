using ThreeMorons.DTOs;

namespace ThreeMorons.Validators
{
    public class AnnouncementValidator : AbstractValidator<AnnouncementDTO>
    {
        public AnnouncementValidator() 
        {
            RuleFor(x=> x.author).NotEmpty().NotNull();
            RuleFor(x=> x.description).NotEmpty().NotNull().MinimumLength(2).MaximumLength(100);
            RuleFor(x => x.body).NotEmpty().NotNull().MinimumLength(1).MaximumLength(1000);
        }
    }
}
