using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;


namespace Test
{
    [TestFixture]
    public class TicketControllerTests
    {
        [Test]
        public async Task TestDbConnection_ShouldReturn100()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LotteriDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using var context = new LotteriDbContext(options);

            // Seed 100 tickets
            if (!context.Tickets.Any())
            {
                context.Tickets.AddRange(Enumerable.Range(1, 100)
                    .Select(i => new Ticket { Id = i }));
                await context.SaveChangesAsync();
            }

            var controller = new Server.Controllers.TicketsController(context);

            // Act
            var result = await controller.TestDbConnection();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            Assert.IsInstanceOf<string>(okResult!.Value);
            var responseString = okResult.Value as string;

            Assert.AreEqual("Database is working. Total tickets: 100", responseString);
        }
    }
}
