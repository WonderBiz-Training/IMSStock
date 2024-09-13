using MediatR;
using Stock.Application.Commands;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Domain.Entities;
using Stock.Domain.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Commands
{
    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, List<UpdateStockDto>>
    {
        private readonly IStockRepository _repository;
        private readonly IValidator<UpdateStockCommand> _validator;

        public UpdateStockCommandHandler(IStockRepository repository, IValidator<UpdateStockCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<List<UpdateStockDto>> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var updatedStocks = new List<UpdateStockDto>();

            if (request.Products == null || !request.Products.Any())
            {
                throw new ArgumentException("At least one product is required.");
            }

            if (request.StockId.HasValue)
            {
                var stock = await _repository.GetStockByIdAsync(request.StockId.Value, cancellationToken);

                if (stock == null)
                {
                    throw new StockNotFoundException($"No stock found for id: {request.StockId.Value}");
                }

                stock.LocationId = request.LocationId;

                foreach (var product in request.Products)
                {
                    if (!await _repository.StockExistsAsync(request.LocationId, product.ProductId, cancellationToken))
                    {
                        throw new StockNotFoundException($"No stock found for LocationId: {request.LocationId} and ProductId: {product.ProductId}");
                    }

                    stock.AddStock = product.AddStock;
                    stock.LessStock = product.LessStock;
                    stock.Purchase = product.Purchase;
                    stock.Sales = product.Sales;
                    stock.Total = (product.Purchase - product.Sales) + (product.AddStock - product.LessStock);
                    stock.UpdatedBy = request.UpdatedBy;
                    stock.UpdatedAt = request.UpdatedAt;
                    stock.IsActive = request.IsActive;

                    await _repository.UpdateStockAsync(stock, cancellationToken);

                    updatedStocks.Add(new UpdateStockDto
                    {
                        Id = stock.Id,
                        LocationId = stock.LocationId,
                        Products = request.Products,
                        UpdatedBy = request.UpdatedBy,
                        UpdatedAt = request.UpdatedAt,
                        IsActive = request.IsActive
                    });
                }
            }
            else
            {
                foreach (var product in request.Products)
                {
                    if (!await _repository.StockExistsAsync(request.LocationId, product.ProductId, cancellationToken))
                    {
                        throw new StockNotFoundException($"No stock found for LocationId: {request.LocationId} and ProductId: {product.ProductId}");
                    }

                    var stock = await _repository.GetStockByLocationAndProductIdAsync(request.LocationId, product.ProductId, cancellationToken);

                    stock.LocationId = request.LocationId;
                    stock.ProductId = product.ProductId;
                    stock.AddStock = product.AddStock;
                    stock.LessStock = product.LessStock;
                    stock.Purchase = product.Purchase;
                    stock.Sales = product.Sales;
                    stock.Total = (product.Purchase - product.Sales) + (product.AddStock - product.LessStock);
                    stock.UpdatedBy = request.UpdatedBy;
                    stock.UpdatedAt = request.UpdatedAt;
                    stock.IsActive = request.IsActive;

                    await _repository.UpdateStockAsync(stock, cancellationToken);

                    updatedStocks.Add(new UpdateStockDto
                    {
                        Id = stock.Id,
                        LocationId = stock.LocationId,
                        Products = request.Products,
                        UpdatedBy = request.UpdatedBy,
                        UpdatedAt = request.UpdatedAt,
                        IsActive = request.IsActive
                    });
                }
            }

            return updatedStocks;
        }
    }

}
