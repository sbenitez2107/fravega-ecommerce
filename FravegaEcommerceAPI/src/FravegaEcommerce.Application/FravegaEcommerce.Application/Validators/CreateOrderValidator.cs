using FluentValidation;
using FravegaEcommerceAPI.Models.DTOs.Requests;

namespace FravegaEcommerceAPI.Validators
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.ExternalReferenceId)
                .NotEmpty().WithMessage("ExternalReferenceId is required")
                .MaximumLength(255).WithMessage("ExternalReferenceId max length is 255");

            RuleFor(x => x.Channel)
                .NotEmpty().WithMessage("The Channel field is required")
                .Must(BeValidChannel).WithMessage("Invalid channel value");

            RuleFor(x => x.PurchaseDate)
                .NotEmpty().WithMessage("PurchaseDate is required")
                .Must(BeUtcDate).WithMessage("PurchaseDate must be in UTC");

            RuleFor(x => x.TotalValue)
                .GreaterThan(0).WithMessage("TotalValue must be greater than 0");

            RuleFor(x => x.Buyer)
                .NotNull().WithMessage("Buyer information is required")
                .SetValidator(new BuyerValidator());

            RuleFor(x => x.Products)
                .NotEmpty().WithMessage("At least one product is required")
                .Must(p => p.Count >= 1).WithMessage("Minimum one product required");

            RuleForEach(x => x.Products)
                .SetValidator(new ProductValidator());
        }

        private bool BeValidChannel(string channel)
        {
            var validChannels = new[] { "Ecommerce", "CallCenter", "Store", "Affiliate" };
            return validChannels.Contains(channel);
        }

        private bool BeUtcDate(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc;
        }
    }    
}
