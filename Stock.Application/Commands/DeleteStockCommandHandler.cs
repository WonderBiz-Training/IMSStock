using FluentValidation;
using MediatR;
using Stock.Application.Commands;
using Stock.Application.Exceptions;
using Stock.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Commands
{
    public class DeleteStockCommandHandler : IRequestHandler<DeleteStockCommand, bool>
    {
        private readonly IStockRepository _repository;
        private readonly IValidator<DeleteStockCommand> _validator;

        public DeleteStockCommandHandler(IStockRepository repository, IValidator<DeleteStockCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<bool> Handle(DeleteStockCommand request, CancellationToken cancellationToken)
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

            return await _repository.DeleteStockAsync(stock, cancellationToken);
        }
    }
}
