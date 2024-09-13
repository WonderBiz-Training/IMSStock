using FluentValidation;
using Stock.Application.Commands;

namespace Stock.Application.Validators
{
    public class DeleteStockCommandValidator : AbstractValidator<DeleteStockCommand>
    {
        public DeleteStockCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required and cannot be empty.")
                .NotEqual(Guid.Empty).WithMessage("Id must be a valid GUID.");
        }
    }
}
