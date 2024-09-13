using FluentValidation;
using Moq;
using Stock.Application.Commands;
using Stock.Application.Exceptions;
using Stock.Domain.Entities;
using Stock.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Stock.Application.Tests.Commands
{
    public class DeleteStockCommandHandlerTests
    {
        private readonly Mock<IStockRepository> _repositoryMock;
        private readonly Mock<IValidator<DeleteStockCommand>> _validatorMock;
        private readonly DeleteStockCommandHandler _handler;

        public DeleteStockCommandHandlerTests()
        {
            _repositoryMock = new Mock<IStockRepository>();
            _validatorMock = new Mock<IValidator<DeleteStockCommand>>();
            _handler = new DeleteStockCommandHandler(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldDeleteStockSuccessfully()
        {
            // Arrange
            var stockId = Guid.NewGuid();
            var deleteStockCommand = new DeleteStockCommand { Id = stockId };

            _validatorMock.Setup(v => v.ValidateAsync(deleteStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new StockModel());

            _repositoryMock.Setup(x => x.DeleteStockAsync(It.IsAny<StockModel>(), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(deleteStockCommand, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_StockNotFound_ShouldThrowStockNotFoundException()
        {
            // Arrange
            var stockId = Guid.NewGuid();
            var deleteStockCommand = new DeleteStockCommand { Id = stockId };

            _validatorMock.Setup(v => v.ValidateAsync(deleteStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((StockModel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<StockNotFoundException>(() =>
                _handler.Handle(deleteStockCommand, CancellationToken.None));

            Assert.Equal($"No stock found for id: {stockId}", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidId_ShouldThrowValidationException()
        {
            // Arrange
            var deleteStockCommand = new DeleteStockCommand { Id = Guid.Empty };

            _validatorMock.Setup(v => v.ValidateAsync(deleteStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                          {
                              new FluentValidation.Results.ValidationFailure("Id", "Id must be a valid GUID.")
                          }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(deleteStockCommand, CancellationToken.None));

            Assert.Contains("Id must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_EmptyId_ShouldThrowValidationException()
        {
            // Arrange
            var deleteStockCommand = new DeleteStockCommand { Id = Guid.Empty };

            _validatorMock.Setup(v => v.ValidateAsync(deleteStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                          {
                              new FluentValidation.Results.ValidationFailure("Id", "Id is required and cannot be empty.")
                          }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(deleteStockCommand, CancellationToken.None));

            Assert.Contains("Id is required and cannot be empty.", exception.Errors.Select(e => e.ErrorMessage));
        }
    }
}
