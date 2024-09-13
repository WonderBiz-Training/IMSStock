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
    public class CreateStockCommandHandlerTests
    {
        private readonly Mock<IStockRepository> _repositoryMock;
        private readonly Mock<IValidator<CreateStockCommand>> _validatorMock;
        private readonly CreateStockCommandHandler _handler;

        public CreateStockCommandHandlerTests()
        {
            _repositoryMock = new Mock<IStockRepository>();
            _validatorMock = new Mock<IValidator<CreateStockCommand>>();
            _handler = new CreateStockCommandHandler(_repositoryMock.Object, _validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateStockSuccessfully()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var createStockCommand = new CreateStockCommand
            {
                LocationId = locationId,
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = productId,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(createStockCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationId, result.LocationId);
            if (result.Products != null)
            {
                Assert.Single(result.Products);

                var product = result.Products[0];
                Assert.Equal(productId, product.ProductId);
                Assert.Equal(10, product.AddStock);
                Assert.Equal(2, product.LessStock);
                Assert.Equal(5, product.Purchase);
                Assert.Equal(3, product.Sales);
                Assert.Equal((5 - 3) + (10 - 2), product.Total);
            }
           
        }

        [Fact]
        public async Task Handle_StockAlreadyExists_ShouldThrowException()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var createStockCommand = new CreateStockCommand
            {
                LocationId = locationId,
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = productId,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(true);

            // Act
            var exception = await Assert.ThrowsAsync<StockAlreadyExistsException>(() =>
                _handler.Handle(createStockCommand, CancellationToken.None));

            // Assert
            Assert.Equal($"A stock entry with LocationId {locationId} and ProductId {productId} already exists.", exception.Message);
        }

        [Fact]
        public async Task Handle_MultipleProducts_ShouldCreateAllStocksSuccessfully()
        {
            // Arrange
            var locationId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var createStockCommand = new CreateStockCommand
            {
                LocationId = locationId,
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = productId1,
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    },
                    new ProductStockCommand
                    {
                        ProductId = productId2,
                        AddStock = 15,
                        LessStock = 5,
                        Purchase = 8,
                        Sales = 6
                    }
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId1, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(false);
            _repositoryMock.Setup(x => x.StockExistsAsync(locationId, productId2, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(createStockCommand, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            if(result.Products != null)
            {
                Assert.Equal(2, result.Products.Count);

                var product1 = result.Products[0];
                Assert.Equal(productId1, product1.ProductId);
                Assert.Equal((5 - 3) + (10 - 2), product1.Total);

                var product2 = result.Products[1];
                Assert.Equal(productId2, product2.ProductId);
                Assert.Equal((8 - 6) + (15 - 5), product2.Total);
            }
            
        }

        [Fact]
        public async Task Handle_EmptyProducts_ShouldThrowArgumentException()
        {
            // Arrange
            var createStockCommand = new CreateStockCommand
            {
                LocationId = Guid.NewGuid(),
                CreatedBy = 1,
                Products = new List<ProductStockCommand>()
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(createStockCommand, CancellationToken.None));

            // Assert
            Assert.Equal("At least one product is required.", exception.Message);
        }
        [Fact]
        public async Task Handle_InvalidRequest_ShouldThrowValidationException()
        {
            // Arrange
            var createStockCommand = new CreateStockCommand
            {
                LocationId = Guid.NewGuid(),
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = Guid.Empty,
                        AddStock = -10,
                        LessStock = -5,
                        Purchase = -8,
                        Sales = -3
                    }
                }
            };

            var validationResult = new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new ValidationFailure("Products[0].ProductId", "ProductId must be a valid GUID."),
                    new ValidationFailure("Products[0].AddStock", "AddStock cannot be negative."),
                    new ValidationFailure("Products[0].LessStock", "LessStock cannot be negative."),
                    new ValidationFailure("Products[0].Purchase", "Purchase cannot be negative."),
                    new ValidationFailure("Products[0].Sales", "Sales cannot be negative.")
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(createStockCommand, CancellationToken.None));

            Assert.Contains("ProductId must be a valid GUID.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("AddStock cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("LessStock cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("Purchase cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("Sales cannot be negative.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_LocationIdRequired_ShouldThrowValidationException()
        {
            // Arrange
            var createStockCommand = new CreateStockCommand
            {
                LocationId = Guid.Empty, // Invalid LocationId (required)
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = Guid.NewGuid(),
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            var validationResult = new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new ValidationFailure("LocationId", "LocationId is required.")
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(createStockCommand, CancellationToken.None));

            Assert.Contains("LocationId is required.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_ProductIdRequired_ShouldThrowValidationException()
        {
            // Arrange
            var createStockCommand = new CreateStockCommand
            {
                LocationId = Guid.NewGuid(),
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = Guid.Empty, // Invalid ProductId (required)
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            var validationResult = new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new ValidationFailure("Products[0].ProductId", "ProductId is required.")
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(createStockCommand, CancellationToken.None));

            Assert.Contains("ProductId is required.", exception.Errors.Select(e => e.ErrorMessage));
        }

        [Fact]
        public async Task Handle_LocationIdAndProductIdRequired_ShouldThrowValidationException()
        {
            // Arrange
            var createStockCommand = new CreateStockCommand
            {
                LocationId = Guid.Empty, // Invalid LocationId
                CreatedBy = 1,
                Products = new List<ProductStockCommand>
                {
                    new ProductStockCommand
                    {
                        ProductId = Guid.Empty, // Invalid ProductId
                        AddStock = 10,
                        LessStock = 2,
                        Purchase = 5,
                        Sales = 3
                    }
                }
            };

            var validationResult = new ValidationResult
            {
                Errors = new List<ValidationFailure>
                {
                    new ValidationFailure("LocationId", "LocationId is required."),
                    new ValidationFailure("Products[0].ProductId", "ProductId is required.")
                }
            };

            _validatorMock.Setup(v => v.ValidateAsync(createStockCommand, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(createStockCommand, CancellationToken.None));

            Assert.Contains("LocationId is required.", exception.Errors.Select(e => e.ErrorMessage));
            Assert.Contains("ProductId is required.", exception.Errors.Select(e => e.ErrorMessage));
        }
    }
}

