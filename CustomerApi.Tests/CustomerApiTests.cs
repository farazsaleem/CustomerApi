using CustomerApi.Controllers;
using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Tests
{
    public class CustomerApiTests 
    {
        private CustomerContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<CustomerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new CustomerContext(options);

            // Seed data
            context.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                MiddleName = "",
                LastName = "Smith",
                Email = "alice@example.com",
                PhoneNumber = "1112223333"
            });

            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetAll_ReturnsCustomers()
        {
            var context = GetDbContext();
            var controller = new CustomerController(context);

            var result = await controller.GetAll();

            var okResult = Assert.IsType<ActionResult<IEnumerable<Customer>>>(result);
            var customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(okResult.Value);
            Assert.Single(customers);
        }

        [Fact]
        public async Task Get_ReturnsCustomer_WhenFound()
        {
            var context = GetDbContext();
            var existing = context.Customers.First();
            var controller = new CustomerController(context);

            var result = await controller.Get(existing.Id);

            var okResult = Assert.IsType<ActionResult<Customer>>(result);
            Assert.Equal(existing.Email, okResult.Value.Email);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenMissing()
        {
            var context = GetDbContext();
            var controller = new CustomerController(context);

            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_AddsCustomer()
        {
            var context = GetDbContext();
            var controller = new CustomerController(context);
            var newCustomer = new Customer
            {
                FirstName = "Bob",
                MiddleName = "X", // add this line
                LastName = "Jones",
                Email = $"bob{Guid.NewGuid()}@test.com",
                PhoneNumber = "4445556666"
            };

            var result = await controller.Create(newCustomer);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<Customer>(createdResult.Value);

            Assert.Equal(newCustomer.Email, created.Email);
            Assert.Equal(2, context.Customers.Count());
        }

        [Fact]
        public async Task Update_ModifiesCustomer()
        {
            var context = GetDbContext();
            var customer = context.Customers.First();
            customer.FirstName = "Updated";

            var controller = new CustomerController(context);
            var result = await controller.Update(customer.Id, customer);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("Updated", context.Customers.First().FirstName);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdMismatch()
        {
            var context = GetDbContext();
            var customer = context.Customers.First();
            var controller = new CustomerController(context);

            var result = await controller.Update(Guid.NewGuid(), customer);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Delete_RemovesCustomer()
        {
            var context = GetDbContext();
            var customer = context.Customers.First();
            var controller = new CustomerController(context);

            var result = await controller.Delete(customer.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Customers);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            var context = GetDbContext();
            var controller = new CustomerController(context);

            var result = await controller.Delete(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}