using Microsoft.EntityFrameworkCore;
using Stock.Domain.Entities;
using Stock.Infrastructure.Data;
using Stock.Domain.Interfaces;

namespace Stock.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly StockDbContext _context;

        public StockRepository(StockDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockModel>> GetAllStocksAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Stocks.ToListAsync(cancellationToken);
        }

        public async Task<StockModel?> GetStockByLocationAndProductIdAsync(Guid locationId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Stocks
                .FirstOrDefaultAsync(s => s.LocationId == locationId && s.ProductId == productId, cancellationToken);
        }

        public async Task<StockModel?> GetStockByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Stocks.FindAsync(new object[] { id }, cancellationToken);
        }


        public async Task<bool> StockExistsAsync(Guid locationId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Stocks
                .AnyAsync(s => s.LocationId == locationId && s.ProductId == productId, cancellationToken);
        }

        public async Task AddStockAsync(StockModel stock, CancellationToken cancellationToken = default)
        {
            if (await StockExistsAsync(stock.LocationId, stock.ProductId, cancellationToken))
            {
                throw new InvalidOperationException($"A stock entry with LocationId {stock.LocationId} and ProductId {stock.ProductId} already exists.");
            }

            await _context.Stocks.AddAsync(stock, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateStockAsync(StockModel stock, CancellationToken cancellationToken = default)
        {
            if (await StockExistsAsync(stock.LocationId, stock.ProductId, cancellationToken))
            {
                var existingStock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.LocationId == stock.LocationId && s.ProductId == stock.ProductId && s.Id != stock.Id, cancellationToken);
                if (existingStock != null)
                {
                    throw new InvalidOperationException($"A stock entry with LocationId {stock.LocationId} and ProductId {stock.ProductId} already exists.");
                }
            }

            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync(cancellationToken);
        }


        public async Task<bool> DeleteStockAsync(StockModel stock, CancellationToken cancellationToken = default)
        {
            _context.Stocks.Remove(stock);
            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }
}
