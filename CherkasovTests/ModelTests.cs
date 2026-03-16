using Xunit;
using CherkasovLibrary.Models;

namespace CherkasovTests
{
    public class ModelTests
    {
        [Fact]
        public void PartnerType_HasIdAndName()
        {
            // Arrange
            var type = new PartnerType();

            // Act
            type.Id = 1;
            type.Name = "ООО";

            // Assert
            Assert.Equal(1, type.Id);
            Assert.Equal("ООО", type.Name);
        }

        [Fact]
        public void Product_HasIdNameAndPrice()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 2;
            product.Name = "Продукт";
            product.Price = 100.50m;

            // Assert
            Assert.Equal(2, product.Id);
            Assert.Equal("Продукт", product.Name);
            Assert.Equal(100.50m, product.Price);
        }

        [Fact]
        public void Sale_HasIdQuantityAndAmount()
        {
            // Arrange
            var sale = new Sale();

            // Act
            sale.Id = 3;
            sale.Quantity = 10;
            sale.TotalAmount = 1000m;

            // Assert
            Assert.Equal(3, sale.Id);
            Assert.Equal(10, sale.Quantity);
            Assert.Equal(1000m, sale.TotalAmount);
        }
    }
}