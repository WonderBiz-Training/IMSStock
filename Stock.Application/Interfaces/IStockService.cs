using Stock.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Interfaces
{
    public interface IStockService
    {
        Task<IEnumerable<StockDto>> GetAllStocksAsync(CancellationToken cancellationToken = default);
        Task<StockDto> GetStockByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CreateStockDto> CreateStockAsync(CreateStockDto dto, CancellationToken cancellationToken = default);
        Task<List<UpdateStockDto>> UpdateStockAsync(UpdateStockDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteStockAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
