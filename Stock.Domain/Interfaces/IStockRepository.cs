using Stock.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Domain.Interfaces
{
    public interface IStockRepository
    {
        Task<IEnumerable<StockModel>> GetAllStocksAsync(CancellationToken cancellationToken = default);
        Task<StockModel?> GetStockByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<StockModel?> GetStockByLocationAndProductIdAsync(Guid locationId, Guid productId, CancellationToken cancellationToken = default);
        Task<bool> StockExistsAsync(Guid locationId, Guid productId, CancellationToken cancellationToken = default);
        Task AddStockAsync(StockModel stock, CancellationToken cancellationToken = default);
        Task UpdateStockAsync(StockModel stock, CancellationToken cancellationToken = default);
        Task<bool> DeleteStockAsync(StockModel stock, CancellationToken cancellationToken = default);
    }
}
