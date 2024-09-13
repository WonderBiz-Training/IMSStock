using FluentValidation;
using MediatR;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Domain.Entities;
using Stock.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Commands
{
    public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, CreateStockDto>
    {
        private readonly IStockRepository _repository;
        private readonly IValidator<CreateStockCommand> _validator;

        public CreateStockCommandHandler(IStockRepository repository, IValidator<CreateStockCommand> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<CreateStockDto> Handle(CreateStockCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            if (request.Products == null || request.Products.Count == 0)
            {
                throw new ArgumentException("At least one product is required.");
            }

            var createStockDto = new CreateStockDto
            {
                LocationId = request.LocationId,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>()
            };

            foreach (var product in request.Products)
            {
                if (await _repository.StockExistsAsync(request.LocationId, product.ProductId, cancellationToken))
                {
                    throw new StockAlreadyExistsException($"A stock entry with LocationId {request.LocationId} and ProductId {product.ProductId} already exists.", null);
                }

                var stock = new StockModel
                {
                    LocationId = request.LocationId,
                    ProductId = product.ProductId,
                    AddStock = product.AddStock,
                    LessStock = product.LessStock,
                    Purchase = product.Purchase,
                    Sales = product.Sales,
                    Total = (product.Purchase - product.Sales) + (product.AddStock - product.LessStock),
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                await _repository.AddStockAsync(stock, cancellationToken);

                createStockDto.Products.Add(new ProductStockDto
                {
                    Id = stock.Id,
                    ProductId = product.ProductId,
                    AddStock = stock.AddStock,
                    LessStock = stock.LessStock,
                    Purchase = stock.Purchase,
                    Sales = stock.Sales,
                    Total = stock.Total
                });
            }

            return createStockDto;
        }
    }
}
