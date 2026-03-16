using Xunit;
using CherkasovLibrary.Models;
using System.Collections.Generic;

namespace CherkasovTests
{
    public class PartnerTests
    {
        [Fact]
        public void Discount_DefaultValue_IsZero()
        {
            // Arrange
            var partner = new Partner();

            // Act
            var discount = partner.Discount;

            // Assert
            Assert.Equal(0, discount);
        }

        [Fact]
        public void Discount_CanBeSetAndGet()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Discount = 15;

            // Assert
            Assert.Equal(15, partner.Discount);
        }

        [Fact]
        public void Partner_HasBasicProperties()
        {
            // Arrange
            var partner = new Partner();

            // Act
            partner.Id = 1;
            partner.Name = "Тест";

            // Assert
            Assert.Equal(1, partner.Id);
            Assert.Equal("Тест", partner.Name);
        }
    }
}