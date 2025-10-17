using DbOperationsWithEFCoreApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // <-- Add this using directive

namespace DbOperationsWithEFCoreApp.Controllers
{
    [Route("api/currencies")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;  // Field Injection, DI

        public CurrencyController(AppDbContext appDbContext)  // Dependency Injection
        {
            this._appDbContext = appDbContext;  // Initialization, Constructor Injection, DI
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllCurrencies()
        {

            //var result = _appDbContext.Currencies.ToList();
            //var result = (from currencies in _appDbContext.Currencies
            //select currencies).ToList();

            //var result = await _appDbContext.Currencies.ToListAsync();
            var result = await (from currencies in _appDbContext.Currencies
                                select currencies).AsNoTracking().ToListAsync();

            return Ok(result);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCurrencyByIdAsync([FromRoute] int id)  // Attribute Routing
        {
            var result = await _appDbContext.Currencies.FindAsync(id);  // Returns null if not found

            //var result = await (from currencies in _appDbContext.Currencies
            //                    where currencies.Id == id
            //                    select currencies).SingleOrDefaultAsync();  // Returns null if not found
            if (result == null)
            {
                return NotFound($"Currency with Id {id} not found.");  // Displays error message 404 if not found
            }
            return Ok(result);
        }
        //[HttpGet("{name}")]
        //public async Task<IActionResult> GetCurrencyByNameAsync([FromRoute] string name)  // Attribute Routing
        //{

        //    //var result = await (from currencies in _appDbContext.Currencies
        //    //                    where currencies.Title.Equals(name, StringComparison.OrdinalIgnoreCase)
        //    //                    select currencies).FirstOrDefaultAsync();  // Returns null if not found

        //    var result = await _appDbContext.Currencies.Where(x => x.Title == name).FirstOrDefaultAsync();  // Returns null if not found
        //    //var result = await _appDbContext.Currencies.Where(x => x.Title == name).FirstAsync(); // Throws InvalidOperationException if not found
        //    //var result = await _appDbContext.Currencies.Where(x => x.Title == name).SingleAsync();
        //    //var result = await _appDbContext.Currencies.Where(x => x.Title == name).SingleOrDefaultAsync(); // Returns null if not found
        //    return Ok(result);
        //}

        //[HttpGet("{name}/{description}")]
        [HttpGet("{name}")]
        public async Task<IActionResult> GetCurrencyByNameAsync([FromRoute] string name, [FromQuery] string? description)
        {
            //var result = await _appDbContext.Currencies.FirstOrDefaultAsync(x => x.Title == name && x.Description == description);  // Returns null if not found
            //var result = await _appDbContext.Currencies.SingleOrDefaultAsync(x => x.Title == name && x.Description == description); // Returns null if not found

            //var result = await _appDbContext.Currencies
            //    .FirstOrDefaultAsync(x => 
            //    x.Title == name 
            //    && (string.IsNullOrEmpty(description) || x.Description == description));  // Returns null if not found

            var result = await _appDbContext.Currencies
                .Where(x =>
                x.Title == name
                && (string.IsNullOrEmpty(description) || x.Description == description)).ToListAsync();

            //var result = await _appDbContext.Currencies
            //                    .Where(x => x.Title == name && x.Description == description)
            //                    .FirstOrDefaultAsync();  // Returns null if not found

            //var result = await (from currencies in _appDbContext.Currencies
            //                    where currencies.Title.Equals(name, StringComparison.OrdinalIgnoreCase)
            //                    && currencies.Description.Equals(description, StringComparison.OrdinalIgnoreCase)
            //                    select currencies).FirstOrDefaultAsync(); 
            return Ok(result);
        }
        [HttpGet("all")]  // get data from in-memory id list
        public async Task<IActionResult> GetCurrencyByIdsAsync()
        {
            var ids = new List<int> { 1, 2, 3, 4, 5, 6 };
            var result = await _appDbContext.Currencies
                .Where(x => ids.Contains(x.Id)).ToListAsync();
            return Ok(result);
        }

        [HttpPost("all")] // get data from client side id list
        public async Task<IActionResult> GetCurrencyByIdsAsync([FromBody] List<int> ids)
        {
            //var result = await _appDbContext.Currencies
            //    .Where(x => ids.Contains(x.Id)).ToListAsync(); // Returns all column data

            //var result = await _appDbContext.Currencies
            //    .Where(x => ids.Contains(x.Id))
            //    .Select(x => new { x.Id, x.Title }) // Returns only Id and Title column data
            //    .ToListAsync();

            var result = await _appDbContext.Currencies
                 .Where(x => ids.Contains(x.Id))
                 .Select(x => new Currency() { Id = x.Id, Title = x.Title }) // Returns only Id and Title column data as Currency object
                 .ToListAsync();

            return Ok(result);
        }
    }
}
