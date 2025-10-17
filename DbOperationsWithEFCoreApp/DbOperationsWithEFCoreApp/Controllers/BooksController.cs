using DbOperationsWithEFCoreApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbOperationsWithEFCoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(AppDbContext appDbContext) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAllBookAsync()
        {
            //Case 1:(Default) Simple Loading -Fetches only the Books, not related entities
            var books = await appDbContext.Books.ToListAsync();
            return Ok(books);

            //Case 1.1:Lazy Loading -Fetches the Books first, then related entities when accessed.
            //Requires navigation properties to be virtual and Lazy Loading Proxies package
            //var books = await appDbContext.Books.ToListAsync();
            //foreach (var book in books)
            //{
            //    var authorName = book.Author?.Name; // Accessing Author triggers a separate query to load Author data
            //    //var languageTitle = book.Language?.Title; // Accessing Language triggers a separate query to load Language data
            //}
            //return Ok(books);

            //var books = await appDbContext.Books.FirstAsync();
            //var author = books.Author; // Accessing Author triggers a separate query to load Author data
            //return Ok(books);

            //var books = await appDbContext.Books.FromSql($"Select * from Books;").ToListAsync();
            //return Ok(books);

            //var books = await appDbContext.Books.FromSql($"Select top 2 * from Books;").ToListAsync();
            //return Ok(books);

            //var books = await appDbContext.Books.FromSqlRaw("SELECT * FROM Books").ToListAsync();
            //return Ok(books);



            //Case 2:Eager Loading -Fetches the Books along with related entities in a single query.
            //Loads Books + Author + Language in one query using JOIN. All related entities are available immediately.

            //var books = await appDbContext.Books
            //    .Include(b => b.Author)
            //    .Include(b => b.Language)
            //    .ToListAsync();
            //return Ok(books);

            //var books = await appDbContext.Books.Where(x => x.Id == 18).Include(x => x.Author).FirstAsync();
            //return Ok(books);

            //Case 3:Explicit Loading - You load the main entity first (Books), then manually load related data when required using Entry().
            //Fetches the Books first, then related entities in separate queries.

            //var books = await appDbContext.Books.ToListAsync();
            //foreach (var book in books)
            //{
            //    await appDbContext.Entry(book).Reference(b => b.Author).LoadAsync();
            //    //await appDbContext.Entry(book).Reference(b => b.Language).LoadAsync();
            //}
            //return Ok(books);


            //Case 4:Eager Loading with Projection
            //Eager Loading with Select to fetch only required fields

            //var books = await appDbContext.Books
            //   .Include(b => b.Author)
            //   .Include(b => b.Language)
            //   .Select(x => new
            //   {
            //       Id = x.Id,
            //       Title = x.Title,
            //       Description = x.Description,
            //       NoOfPages = x.NoOfPages,
            //       Language = x.Language != null ? new { x.Language.Id, x.Language.Title, x.Language.Description } : null,
            //       Author = x.Author != null ? new { x.Author.Id, x.Author.Name, x.Author.Email } : null
            //   }).ToListAsync();
            //return Ok(books);

            //Case 5: Eager Loading with Select to fetch only required fields without Include, as Include is redundant when using Select
            //var books = await appDbContext.Books.Select(x => new
            //{
            //    Id = x.Id,
            //    Title = x.Title,
            //    Description = x.Description,
            //    NoOfPages = x.NoOfPages,
            //    Language = x.Language != null ? new { x.Language.Id, x.Language.Title, x.Language.Description } : null,
            //    Author = x.Author != null ? new { x.Author.Id, x.Author.Name, x.Author.Email } : null
            //}).ToListAsync();
            //return Ok(books);

        }

        //[HttpGet("languages")]
        //public async Task<IActionResult> GetAllLanguagesAsync()
        //{
        //    var languages = await appDbContext.Languages
        //        .Include(l => l.Books) // Eagerly load related Books
        //        .ToListAsync();
        //    return Ok(languages);
        //}

        [HttpGet("languages")]
        public async Task<IActionResult> GetAllLanguagesAsync()
        {
            var languages = await appDbContext.Languages.ToListAsync();  // Load Languages first
            foreach (var language in languages)
            {
                await appDbContext.Entry(language).Collection(l => l.Books).LoadAsync();  // Explicitly load related Books
            }
            return Ok(languages);
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNewBook([FromBody] Book model)
        {
            appDbContext.Books.Add(model);  // Adds the new book to the context
            await appDbContext.SaveChangesAsync();
            return Ok(model);

        }

        [HttpPost("bulk")]
        public async Task<IActionResult> AddBooks([FromBody] List<Book> model)
        {
            appDbContext.Books.AddRange(model);  // Adds multiple new books to the context
            await appDbContext.SaveChangesAsync();
            return Ok(model);

        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> UpdateBook([FromRoute] int bookId, [FromBody] Book model)
        {
            //var book = appDbContext.Books.SingleOrDefault(b => b.Id == bookId);  // Using SingleOrDefault to fetch the book
            var book = await appDbContext.Books.FindAsync(bookId);  // Using FindAsync to fetch the book by primary key
            if (book == null)
            {
                return NotFound();
            }
            book.Title = model.Title;
            book.Description = model.Description;
            book.NoOfPages = model.NoOfPages;
            await appDbContext.SaveChangesAsync();
            return Ok(model);
        }
        [HttpPut("")]
        public async Task<IActionResult> UpdateBookWithSingleQuery([FromRoute] int bookId, [FromBody] Book model)
        {
            //appDbContext.Entry(model).State = Microsoft.EntityFrameworkCore.EntityState.Modified; // Mark the entire entity as modified
            appDbContext.Books.Update(model);  // This will update all fields of the book, so ensure the model contains the correct data
            await appDbContext.SaveChangesAsync();
            return Ok(model);
        }
        [HttpPut("bulk")]
        public async Task<IActionResult> UpdateBookInBulk()
        {
            //var books = appDbContext.Books.ToList();
            //foreach (var item in books)
            //{
            //    item.Title = "Updated";
            //}
            //await appDbContext.SaveChangesAsync();
            //await appDbContext.Books.ForEachAsync(b => b.Title = "Updated");

            await appDbContext.Books
                .Where(x => x.NoOfPages == 100)
                .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.Description, p => p.Title + "This is book description 2")
            .SetProperty(p => p.Title, p => p.Title + "updated 2")
            //.SetProperty(p=> p.NoOfPages, 200)
            );
            return Ok();
        }
        //[HttpDelete("{bookId}")]   // Using FindAsync to fetch the book by primary key,then deleting it- two time hit the database
        //public async Task<IActionResult> DeleteBookByIdAsync([FromRoute] int bookId)
        //{
        //    var book = await appDbContext.Books.FindAsync(bookId);
        //    //var book = await appDbContext.Books.FirstOrDefaultAsync(x => x.Id == bookId);
        //    if (book == null)
        //    {
        //        return NotFound();
        //    }
        //    appDbContext.Books.Remove(book);
        //    await appDbContext.SaveChangesAsync();
        //    return Ok(book);
        //}

        [HttpDelete("{bookId}")]  // Deleting without fetching the entity first - one time hit the database
        public async Task<IActionResult> DeleteBookByIdAsync([FromRoute] int bookId)
        {
            var book = new Book { Id = bookId };
            appDbContext.Entry(book).State = EntityState.Deleted;
            await appDbContext.SaveChangesAsync();
            return Ok(book);
        }

        [HttpDelete("bulk")]  // Deleting without fetching the entity first - one time hit the database
        public async Task<IActionResult> DeleteBooksInBulkAsync()
        {
            //var Books = await appDbContext.Books.Where(x => x.Id < 10).ToListAsync();
            //appDbContext.Books.RemoveRange(Books);

            var Books = await appDbContext.Books.ExecuteDeleteAsync();

            //var Books = await appDbContext.Books.Where(x => x.Id < 11).ExecuteDeleteAsync();
            await appDbContext.SaveChangesAsync();
            return Ok();
        }


    }


}
