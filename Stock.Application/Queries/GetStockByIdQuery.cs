using MediatR;
using Stock.Application.DTOs;

namespace Stock.Application.Queries
{
    public class GetStockByIdQuery : IRequest<StockDto>
    {
        public Guid Id { get; set; }
    }
}
