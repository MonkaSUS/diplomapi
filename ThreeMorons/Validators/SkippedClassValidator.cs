namespace ThreeMorons.Validators
{
    public class SkippedClassValidator : AbstractValidator<SkippedClassInput>
    {
        public SkippedClassValidator() 
        {
            RuleFor(x => x.StudNumber).NotEmpty().Length(5);
        }
    }
}
