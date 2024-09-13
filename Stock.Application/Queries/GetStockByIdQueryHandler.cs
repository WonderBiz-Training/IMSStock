using MediatR;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Application.Queries;
using Stock.Domain.Interfaces;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using Stock.Application.Commands;

namespace Stock.Application.Queries
{
    public class GetStockByIdQueryHandler : IRequestHandler<GetStockByIdQuery, StockDto>
    {
        private readonly IStockRepository _repository;
        private readonly IValidator<GetStockByIdQuery> _validator;

        public GetStockByIdQueryHandler(IStockRepository repository, IValidator<GetStockByIdQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<StockDto> Handle(GetStockByIdQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            var stock = await _repository.GetStockByIdAsync(request.Id, cancellationToken);

            if (stock == null)
            {
                throw new StockNotFoundException($"No stock found for id: {request.Id}");
            }

            return new StockDto
            {
                Id = stock.Id,
                LocationId = stock.LocationId,
                ProductId = stock.ProductId,
                AddStock = stock.AddStock,
                LessStock = stock.LessStock,
                Purchase = stock.Purchase,
                Sales = stock.Sales,
                Total = stock.Total
            };
        }
    }
}
