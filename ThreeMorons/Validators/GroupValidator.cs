namespace ThreeMorons.Validators
{
    public class GroupValidator : AbstractValidator<GroupInput>
    {
        public GroupValidator() 
        {
            RuleFor(x => x.GroupName).MaximumLength(8).MinimumLength(5);
        }
    }
}
