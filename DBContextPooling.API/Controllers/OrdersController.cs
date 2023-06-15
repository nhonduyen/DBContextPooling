using DBContextPooling.API.Data;
using DBContextPooling.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DBContextPooling.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderingContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderingContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> CreateMany(int quantity, CancellationToken cancellationToken)
        {
            var customers = new List<Customer>(quantity);
            for (int i = 0; i < quantity; i++)
            {
                var customer = new Customer
                {
                    FirstName = Faker.Name.First(),
                    LastName = Faker.Name.Last(),
                    Email = Faker.Internet.Email(),
                    ContactNumber = Faker.Phone.Number(),
                    Address = Faker.Address.StreetName()
                };
                customers.Add(customer);
            }

            _context.Customers.AddRange(customers);
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"{result} items have been created");
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMany(int quantity, CancellationToken cancellationToken)
        {
            var customers = await _context.Customers.OrderBy(x => Guid.NewGuid()).Take(quantity).ToListAsync();
            foreach (var customer in customers)
            {
                customer.FirstName = Faker.Name.First();
                customer.LastName = Faker.Name.Last();
                customer.Email = Faker.Internet.Email();
                customer.ContactNumber = Faker.Phone.Number();
                customer.Address = Faker.Address.StreetName();
            }
            _context.UpdateRange(customers);
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"{result} items have been updated");
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(u => u.Id.Equals(id), cancellationToken);
            customer.FirstName = Faker.Name.First();
            customer.LastName = Faker.Name.Last();
            customer.Email = Faker.Internet.Email();
            customer.ContactNumber = Faker.Phone.Number();
            customer.Address = Faker.Address.StreetName();

            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"{result} items have been updated; Id = {customer.Id}");
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CancellationToken cancellationToken)
        {
            var customer = new Customer
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Email = Faker.Internet.Email(),
                ContactNumber = Faker.Phone.Number(),
                Address = Faker.Address.StreetName()
            };
            
            _context.Customers.Add(customer);
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"{result} items have been created; Id = {customer.Id}");
            return Ok(customer);
        }

        [HttpGet]
        public async Task<ActionResult> Get(CancellationToken cancellationToken)
        {
            var users = await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);
            return Ok(users);
        }

        [HttpGet]
        public async Task<ActionResult> Customer([Required] Guid id, CancellationToken cancellationToken)
        {
            var users = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(u => u.Id.Equals(id), cancellationToken);
            return Ok(users);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(CancellationToken cancellationToken)
        {
            var result = await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE customer;", cancellationToken);
            _logger.LogInformation($"Table {nameof(Customer)}'s data has been deleted");
            return Ok(result);
        }
    }
}
