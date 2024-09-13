using FluentValidation;
using Stock.Application.Queries;

namespace Stock.Application.Validators
{
    public class GetStockByIdQueryValidator : AbstractValidator<GetStockByIdQuery>
    {
        public GetStockByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.")
                .NotEqual(Guid.Empty).WithMessage("Id must be a valid GUID.");
        }
    }
}
