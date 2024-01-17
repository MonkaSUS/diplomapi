namespace ThreeMorons.Validators
{
    public class StudentDelayValidator :AbstractValidator<StudentDelayInput>
    {
        public StudentDelayValidator() 
        {
            RuleFor(x => x.studNumber).Length(5).Matches("^[0-9]+$");
            RuleFor(x => x.className).MaximumLength(8).MinimumLength(5);
        }
    }
}
