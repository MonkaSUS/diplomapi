using FluentValidation;
using ThreeMorons.Model;

namespace ThreeMorons.Validators
{
    public class GroupValidator : AbstractValidator<Group>
    {
        public GroupValidator() 
        {
            RuleFor(x => x.GroupName).MaximumLength(6).MinimumLength(4);
        }
    }
}
