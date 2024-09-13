using MediatR;
using Stock.Application.DTOs;
using Stock.Application.Queries;
using Stock.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stock.Application.Queries
{
    public class GetAllStocksQueryHandler : IRequestHandler<GetAllStocksQuery, IEnumerable<StockDto>>
    {
        private readonly IStockRepository _repository;

        public GetAllStocksQueryHandler(IStockRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<StockDto>> Handle(GetAllStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllStocksAsync(cancellationToken);
            return stocks.Select(stock => new StockDto
            {
                Id = stock.Id,
                LocationId = stock.LocationId,
                ProductId = stock.ProductId,
                AddStock = stock.AddStock,
                LessStock = stock.LessStock,
                Purchase = stock.Purchase,
                Sales = stock.Sales,
                Total = stock.Total
            });
        }
    }
}
