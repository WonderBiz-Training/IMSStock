using MediatR;
using Stock.Application.DTOs;
using Stock.Application.Interfaces;
using Stock.Application.Queries;
using Stock.Application.Commands;
using Stock.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IMediator _mediator;

        public StockService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<StockDto>> GetAllStocksAsync(CancellationToken cancellationToken = default)
        {
            var query = new GetAllStocksQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        public async Task<StockDto> GetStockByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetStockByIdQuery { Id = id };
                var stock = await _mediator.Send(query, cancellationToken);
                return stock;
            }
            catch (StockNotFoundException)
            {
                throw;
            }
        }

        public async Task<CreateStockDto> CreateStockAsync(CreateStockDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var productStockCommands = dto.Products?.Select(p => new ProductStockCommand
                {
                    ProductId = p.ProductId,
                    AddStock = (int)p.AddStock,
                    LessStock = (int)p.LessStock,
                    Purchase = p.Purchase,
                    Sales = p.Sales
                }).ToList();

                var command = new CreateStockCommand
                {
                    LocationId = dto.LocationId,
                    CreatedBy = dto.CreatedBy,
                    Products = productStockCommands,
                    CreatedDate = dto.CreatedAt,
                    IsActive = dto.IsActive
                };

                var result = await _mediator.Send(command, cancellationToken);
                return result;
            }
            catch (StockAlreadyExistsException)
            {
                throw;
            }
        }


        public async Task<List<UpdateStockDto>> UpdateStockAsync(UpdateStockDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new UpdateStockCommand
                {
                    StockId = dto.Id,
                    LocationId = dto.LocationId,
                    Products = dto.Products,
                    UpdatedBy = dto.UpdatedBy,
                    UpdatedAt = dto.UpdatedAt,
                    IsActive = dto.IsActive
                };
                var result = await _mediator.Send(command, cancellationToken);
                if (result == null || result.Count == 0)
                {
                    throw new StockNotFoundException("No stocks found for the provided details.");
                }
                return result;
            }
            catch (StockNotFoundException)
            {
                throw;
            }
            catch (StockValidationException)
            {
                throw;
            }
        }

        public async Task<bool> DeleteStockAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new DeleteStockCommand { Id = id };
                var result = await _mediator.Send(command, cancellationToken);
                return result;
            }
            catch (StockNotFoundException)
            {
                throw;
            }
        }
    }
}
