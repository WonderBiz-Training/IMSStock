using FluentValidation;
using Moq;
using Stock.Application.Queries;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentValidation.Results;
using Stock.Domain.Entities;
using Stock.Application.Validators;

namespace Stock.Application.Tests.Queries
{
    public class GetStockByIdQueryHandlerTests
    {
        private readonly Mock<IStockRepository> _repositoryMock;
        private readonly Mock<IValidator<GetStockByIdQuery>> _validatorMock;
        private readonly GetStockByIdQueryHandler _handler;

        public GetStockByIdQueryHandlerTests()
        {
            _repositoryMock = new Mock<IStockRepository>();
            _validatorMock = new Mock<IValidator<GetStockByIdQuery>>();
            _handler = new GetStockByIdQueryHandler(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldReturnStockSuccessfully()
        {
            // Arrange
            var stockId = Guid.NewGuid();
            var query = new GetStockByIdQuery { Id = stockId };
            var stock = new StockModel
            {
                Id = stockId,
                LocationId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                AddStock = 100,
                LessStock = 10,
                Purchase = 50,
                Sales = 20,
                Total = (50 - 20) + (100 - 10)
            };

            _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(stock);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(stock.Id, result.Id);
            Assert.Equal(stock.LocationId, result.LocationId);
            Assert.Equal(stock.ProductId, result.ProductId);
            Assert.Equal(stock.AddStock, result.AddStock);
            Assert.Equal(stock.LessStock, result.LessStock);
            Assert.Equal(stock.Purchase, result.Purchase);
            Assert.Equal(stock.Sales, result.Sales);
            Assert.Equal(stock.Total, result.Total);
        }

        [Fact]
        public async Task Handle_StockNotFound_ShouldThrowStockNotFoundException()
        {
            // Arrange
            var stockId = Guid.NewGuid();
            var query = new GetStockByIdQuery { Id = stockId };

            _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(null as StockModel);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<StockNotFoundException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Equal($"No stock found for id: {stockId}", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidId_ShouldThrowValidationException()
        {
            // Arrange
            var query = new GetStockByIdQuery { Id = Guid.Empty };
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("Id", "Id must be a valid GUID.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Contains("Id must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_EmptyId_ShouldThrowValidationException()
        {
            // Arrange
            var query = new GetStockByIdQuery { Id = Guid.Empty };
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("Id", "Id is required.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(query, CancellationToken.None));

            Assert.Contains("Id is required.", exception.Errors.Select(e => e.ErrorMessage));
        }
    }
}
