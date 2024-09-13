using MediatR;
using Stock.Application.DTOs;
using System.Collections.Generic;

namespace Stock.Application.Queries
{
    public class GetAllStocksQuery : IRequest<IEnumerable<StockDto>>
    {
    }
}
