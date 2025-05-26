using FluentValidation;
using FravegaEcommerceAPI.Models.DTOs.Requests;

namespace FravegaEcommerceAPI.Validators
{
    public class BuyerValidator : AbstractValidator<BuyerDto>
    {
        public BuyerValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(100).WithMessage("FirstName max length is 100");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(100).WithMessage("LastName max length is 100");

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("DocumentNumber is required")
                .MinimumLength(5).WithMessage("DocumentNumber min length is 5")
                .MaximumLength(20).WithMessage("DocumentNumber max length is 20");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\+\d{8,15}$").WithMessage("Invalid phone number format");
        }
    }
}
