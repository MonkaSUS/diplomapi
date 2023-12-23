using FluentValidation;
using ThreeMorons.Model;
using ThreeMorons.UserInputTypes;

namespace ThreeMorons.Validators
{
    public class GroupValidator : AbstractValidator<GroupInput>
    {
        public GroupValidator() 
        {
            RuleFor(x => x.GroupName).MaximumLength(6).MinimumLength(4);
        }
    }
}
