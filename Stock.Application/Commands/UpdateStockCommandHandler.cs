using MediatR;
using Stock.Application.Commands;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Domain.Entities;
using Stock.Domain.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
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
            await ValidateRequestAsync(request, cancellationToken);

            if (request.Products == null || request.Products.Count == 0)
            {
                throw new ArgumentException("At least one product is required.");
            }

            var updatedStocks = request.StockId.HasValue
                ? await UpdateExistingStockAsync(request, cancellationToken)
                : await UpdateStockByLocationAsync(request, cancellationToken);

            return updatedStocks;
        }

        private async Task ValidateRequestAsync(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        private async Task<List<UpdateStockDto>> UpdateExistingStockAsync(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var updatedStocks = new List<UpdateStockDto>();

            if (!request.StockId.HasValue) // Safeguard for nullable StockId
            {
                throw new ArgumentException("StockId is required.");
            }

            var stock = await _repository.GetStockByIdAsync(request.StockId.Value, cancellationToken);

            if (stock == null)
            {
                throw new StockNotFoundException($"No stock found for id: {request.StockId.Value}");
            }

            stock.LocationId = request.LocationId;

            if(request.Products != null)
            {
                foreach (var product in request.Products)
                {
                    await ValidateStockExistsAsync(request.LocationId, product.ProductId, cancellationToken);

                    UpdateStockValues(stock, product, request);

                    await _repository.UpdateStockAsync(stock, cancellationToken);

                    updatedStocks.Add(CreateUpdateStockDto(stock, request));
                }

            }

            return updatedStocks;
        }

        private async Task<List<UpdateStockDto>> UpdateStockByLocationAsync(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var updatedStocks = new List<UpdateStockDto>();

            if (request.Products != null)

                foreach (var product in request.Products)
            {
                await ValidateStockExistsAsync(request.LocationId, product.ProductId, cancellationToken);

                var stock = await _repository.GetStockByLocationAndProductIdAsync(request.LocationId, product.ProductId, cancellationToken);

                if (stock == null) // Added null check to prevent possible null reference
                {
                    throw new StockNotFoundException($"No stock found for LocationId: {request.LocationId} and ProductId: {product.ProductId}");
                }

                UpdateStockValues(stock, product, request);

                await _repository.UpdateStockAsync(stock, cancellationToken);

                updatedStocks.Add(CreateUpdateStockDto(stock, request));
            }

            return updatedStocks;
        }

        private async Task ValidateStockExistsAsync(Guid locationId, Guid productId, CancellationToken cancellationToken)
        {
            if (!await _repository.StockExistsAsync(locationId, productId, cancellationToken))
            {
                throw new StockNotFoundException($"No stock found for LocationId: {locationId} and ProductId: {productId}");
            }
        }

        // Marked static as it does not access instance data
        private static void UpdateStockValues(StockModel stock, ProductStockDto product, UpdateStockCommand request)
        {
            stock.AddStock = product.AddStock;
            stock.LessStock = product.LessStock;
            stock.Purchase = product.Purchase;
            stock.Sales = product.Sales;
            stock.Total = (product.Purchase - product.Sales) + (product.AddStock - product.LessStock);
            stock.UpdatedBy = request.UpdatedBy;
            stock.UpdatedAt = request.UpdatedAt;
            stock.IsActive = request.IsActive;
        }

        // Marked static as it does not access instance data
        private static UpdateStockDto CreateUpdateStockDto(StockModel stock, UpdateStockCommand request)
        {
            return new UpdateStockDto
            {
                Id = stock.Id,
                LocationId = stock.LocationId,
                Products = request.Products,
                UpdatedBy = request.UpdatedBy,
                UpdatedAt = request.UpdatedAt,
                IsActive = request.IsActive
            };
        }
    }
}
