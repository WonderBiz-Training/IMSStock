using MediatR;
using Stock.Application.DTOs;
using System;
using System.Collections.Generic;

namespace Stock.Application.Commands
{
    public class UpdateStockCommand : IRequest<List<UpdateStockDto>>
    {
        public Guid? StockId { get; set; }
        public Guid LocationId { get; set; }
        public List<ProductStockDto>? Products { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = false;
    }
}
