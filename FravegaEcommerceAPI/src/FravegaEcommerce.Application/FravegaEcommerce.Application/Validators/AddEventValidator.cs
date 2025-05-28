using FluentValidation;
using FravegaEcommerceAPI.Models.DTOs.Requests;

namespace FravegaEcommerceAPI.Validators
{
    public class AddEventValidator : AbstractValidator<AddEventRequest>
    {
        public AddEventValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("The Id field is required.")
                .MaximumLength(50).WithMessage("Event ID max length is 50");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("The Type field is required.")
                .Must(BeValidEventType).WithMessage("Invalid event type");

            RuleFor(x => x.Date)
                .Must(date => date != default(DateTime) && date != DateTime.MinValue).When(x => x.Date != null)
                .NotEmpty().WithMessage("Event date is required")
                .Must(BeUtcDate).WithMessage("Event date must be in UTC")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Event date cannot be in the future");

            RuleFor(x => x.User)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.User))
                .WithMessage("User max length is 100");
        }

        private bool BeValidEventType(string type)
        {
            var validTypes = new[] {
            "PaymentReceived",
            "Cancelled",
            "Invoiced",
            "Returned",
            "Created"
        };
            return validTypes.Contains(type);
        }

        private bool BeUtcDate(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc;
        }
    }
}
