using FluentValidation.Results;

namespace FravegaEcommerceAPI.Exceptions
{
    public class ValidationException : Exception
    {
        public List<ValidationFailure> Errors { get; }

        public ValidationException(string message) : base(message) { }

        public ValidationException(List<ValidationFailure> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}
