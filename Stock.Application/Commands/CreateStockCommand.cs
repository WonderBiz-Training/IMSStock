using MediatR;
using Stock.Application.DTOs;
using System;
using System.Collections.Generic;

namespace Stock.Application.Commands
{
    public class CreateStockCommand : IRequest<CreateStockDto>
    {
        public Guid LocationId { get; set; } = Guid.Empty;
        public List<ProductStockCommand>? Products { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = false;
    }

    public class ProductStockCommand
    {
        public Guid ProductId { get; set; }
        public int AddStock { get; set; }
        public int LessStock { get; set; }
        public long Purchase { get; set; }
        public long Sales { get; set; }
    }
}
