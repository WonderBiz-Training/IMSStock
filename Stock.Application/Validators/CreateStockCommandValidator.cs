using FluentValidation;
using Stock.Application.Commands;

namespace Stock.Application.Validators
{
    public class CreateStockCommandValidator : AbstractValidator<CreateStockCommand>
    {
        public CreateStockCommandValidator()
        {
            RuleFor(x => x.LocationId)
                .NotEmpty().WithMessage("LocationId is required.")
                .NotEqual(Guid.Empty).WithMessage("LocationId must be a valid GUID.");

            RuleFor(x => x.Products)
                .NotNull().WithMessage("Products cannot be null.")
                .NotEmpty().WithMessage("At least one product is required.");

            RuleForEach(x => x.Products).ChildRules(product =>
            {
                product.RuleFor(p => p.ProductId)
                    .NotEmpty().WithMessage("ProductId is required.")
                    .NotEqual(Guid.Empty).WithMessage("ProductId must be a valid GUID.");

                product.RuleFor(p => p.AddStock)
                    .GreaterThanOrEqualTo(0).WithMessage("AddStock cannot be negative.");

                product.RuleFor(p => p.LessStock)
                    .GreaterThanOrEqualTo(0).WithMessage("LessStock cannot be negative.");

                product.RuleFor(p => p.Purchase)
                    .GreaterThanOrEqualTo(0).WithMessage("Purchase cannot be negative.");

                product.RuleFor(p => p.Sales)
                    .GreaterThanOrEqualTo(0).WithMessage("Sales cannot be negative.");
            });
        }
    }

}
