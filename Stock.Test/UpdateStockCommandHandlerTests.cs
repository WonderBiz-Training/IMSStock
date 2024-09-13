using FluentValidation;
using FluentValidation.Results;
using Moq;
using Stock.Application.Commands;
using Stock.Application.DTOs;
using Stock.Application.Exceptions;
using Stock.Domain.Entities;
using Stock.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Stock.Application.Tests.Commands
{
    public class UpdateStockCommandHandlerTests
    {
        private readonly Mock<IStockRepository> _repositoryMock;
        private readonly Mock<IValidator<UpdateStockCommand>> _validatorMock;
        private readonly UpdateStockCommandHandler _handler;

        public UpdateStockCommandHandlerTests()
        {
            _repositoryMock = new Mock<IStockRepository>();
            _validatorMock = new Mock<IValidator<UpdateStockCommand>>();
            _handler = new UpdateStockCommandHandler(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldUpdateStockSuccessfully()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = locationId,
                StockId = stockId,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>
                {
                    new ProductStockDto
                    {
                        ProductId = productId,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new StockModel());

            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

            _repositoryMock.Setup(x => x.GetStockByLocationAndProductIdAsync(locationId, productId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new StockModel());

            // Act
            var result = await _handler.Handle(updateStockCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task Handle_StockNotFound_ShouldThrowStockNotFoundException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var stockId = Guid.NewGuid();
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = locationId,
                StockId = stockId,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>
                {
                    new ProductStockDto
                    {
                        ProductId = productId,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.GetStockByIdAsync(stockId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync((StockModel)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<StockNotFoundException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Equal($"No stock found for id: {stockId}", exception.Message);
        }

        [Fact]
        public async Task Handle_ProductNotFound_ShouldThrowStockNotFoundException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = locationId,
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>
                {
                    new ProductStockDto
                    {
                        ProductId = productId,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<StockNotFoundException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Equal($"No stock found for LocationId: {locationId} and ProductId: {productId}", exception.Message);
        }

        [Fact]
        public async Task Handle_EmptyProducts_ShouldThrowArgumentException()
        {
            // Arrange
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = Guid.NewGuid(),
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>()
            };

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Equal("At least one product is required.", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = Guid.NewGuid(),
                UpdatedBy = 1,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Products = new List<ProductStockDto>
                {
                    new ProductStockDto
                    {
                        ProductId = Guid.Empty,
                        AddStock = -10,
                        LessStock = -5,
                        Purchase = -8,
                        Sales = -3
                    }
                }
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("ProductId", "ProductId must be a valid GUID."),
                new ValidationFailure("AddStock", "AddStock cannot be negative."),
                new ValidationFailure("LessStock", "LessStock cannot be negative."),
                new ValidationFailure("Purchase", "Purchase cannot be negative."),
                new ValidationFailure("Sales", "Sales cannot be negative.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Contains("ProductId must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("AddStock cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("LessStock cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("Purchase cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("Sales cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
        }


        [Fact]
        public async Task Handle_ValidationFailureForEmptyLocationId_ShouldThrowValidationException()
        {
            // Arrange
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = Guid.Empty,
                Products = new List<ProductStockDto>
            {
                new ProductStockDto
                {
                    ProductId = Guid.NewGuid(),
                    AddStock = 10,
                    LessStock = 2,
                    Purchase = 5,
                    Sales = 3
                }
            }
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("LocationId", "LocationId is required."),
                new ValidationFailure("LocationId", "LocationId must be a valid GUID.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Contains("LocationId is required.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("LocationId must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_ValidationFailureForProductsListIsNull_ShouldThrowValidationException()
        {
            // Arrange
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = Guid.NewGuid(),
                Products = null // Products list is null
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Products", "Products cannot be null."),
                new ValidationFailure("Products", "At least one product is required.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Contains("Products cannot be null.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("At least one product is required.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_ValidationFailureForMissingProductId_ShouldThrowValidationException()
        {
            // Arrange
            var updateStockCommand = new UpdateStockCommand
            {
                LocationId = Guid.NewGuid(),
                    Products = new List<ProductStockDto>
            {
                new ProductStockDto
                {
                    AddStock = 10,
                    LessStock = 2,
                    Purchase = 5,
                    Sales = 3
                }
            }
            };

            var validationResult = new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Products[0].ProductId", "ProductId is required."),
                new ValidationFailure("Products[0].ProductId", "ProductId must be a valid GUID.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(updateStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(updateStockCommand, CancellationToken.None));

            Assert.Contains("ProductId is required.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("ProductId must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
        }


    }
}
