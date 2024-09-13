using System.ComponentModel.DataAnnotations;

namespace Stock.Application.DTOs
{
    public class StockDto
    {
        public Guid Id { get; set; }

        public Guid LocationId { get; set; }

        public Guid ProductId { get; set; }

        public long AddStock { get; set; }

        public long LessStock { get; set; }

        public long Purchase { get; set; }

        public long Sales { get; set; }

        public long Total { get; set; }

        public long CreatedBy { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public long UpdatedBy { get; set; } = 1;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = false;
    }

    public class CreateStockDto
    {
        public Guid LocationId { get; set; }
        public List<ProductStockDto>? Products { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }



    public class ProductStockDto
    {
        
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public long AddStock { get; set; }
        public long LessStock { get; set; }
        public long Purchase { get; set; }
        public long Sales { get; set; }
        public long Total { get; set; }
    }

    public class UpdateStockDto
    {
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }
        public List<ProductStockDto>? Products { get; set; }
        public long UpdatedBy { get; set; } = 1;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
    }

}
