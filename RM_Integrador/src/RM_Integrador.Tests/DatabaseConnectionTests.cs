using Microsoft.EntityFrameworkCore;
using RM_Integrador.Core.Data;
using Xunit;

namespace RM_Integrador.Tests
{
    public class DatabaseConnectionTests
    {
        [Fact]
        public void CanConnectToDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=.\\SQLEXPRESS;Database=RM_Integrador;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;

            // Act & Assert
            using var context = new ApplicationDbContext(options);
            Assert.True(context.Database.CanConnect());
        }
    }
}