using FluentValidation;
using FravegaEcommerceAPI.Models.DTOs.Requests;

namespace FravegaEcommerceAPI.Validators
{
    public class ProductValidator : AbstractValidator<ProductDto>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("The Sku field is required.")
                .MaximumLength(50).WithMessage("SKU max length is 50");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The Name field is required.")
                .MaximumLength(200).WithMessage("Product name max length is 200");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description max length is 1000");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1");
        }
    }
}
