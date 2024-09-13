using MediatR;

namespace Stock.Application.Commands
{
    public class DeleteStockCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }
}
