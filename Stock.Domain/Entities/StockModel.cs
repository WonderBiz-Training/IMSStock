using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock.Domain.Entities
{
    public class StockModel
    {
        public Guid Id { get; set; }

        public Guid LocationId { get; set; }

        public Guid ProductId { get; set; }

        public long AddStock { get; set; }

        public long LessStock { get; set; }

        public long Purchase { get; set; }

        public long Sales { get; set; }

        public long Total { get; set; }

        public DateTime CreatedAt { get; set; }

        public long CreatedBy { get; set; }

        public long UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsActive { get; set; } = false;
    }
}
