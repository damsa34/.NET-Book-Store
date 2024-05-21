using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Models;
using Books.ViewModels;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Authorization;

namespace Books.Controllers
{
    public class BooksController : Controller
    {
        private readonly BooksContext _context;

        public BooksController(BooksContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Book> books = _context.Book.Include(b => b.Author).Include(b => b.Genres).ThenInclude(bg => bg.Genre).Include(b => b.Reviews);
            IQueryable<string> titleQuery = _context.Book.OrderBy(b => b.Title).Select(b => b.Title).Distinct();

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => 
                b.Title.Contains(searchString) || 
                b.Author.FirstName.Contains(searchString) ||
                b.Author.LastName.Contains(searchString) ||
                b.Genres.Any(b => b.Genre.GenreName.Contains(searchString)
                ));
            }

            foreach (var book in books)
            {
                var distinctGenres = book.Genres
                    .Where(bg => bg.Genre != null)
                    .GroupBy(bg => bg.GenreId)
                    .Select(group => group.First())
                    .ToList();

                book.Genres = distinctGenres;
            }

            var bookVM = new BookFilterViewModel
            {
                TemporaryBooks = await books.ToListAsync(),
                FilterOptions = new SelectList(await titleQuery.ToListAsync())
            };

            return View(bookVM);
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Buy(int id)
        {
            var userId = User.Identity.Name; // Assuming the user name is unique
            var userBook = new UserBooks
            {
                AppUser = userId,
                BookId = id
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .Include(b => b.Genres)
                .ThenInclude(b => b.Genre)
                .Distinct()
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }
            
            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName");
            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();

                if (selectedGenres != null)
                {
                    foreach (var genreId in selectedGenres)
                    {
                        _context.Add(new BookGenre { BookId = book.Id, GenreId = genreId });
                    }
                    await _context.SaveChangesAsync(); 
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName", book.AuthorId);
            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", selectedGenres);
            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genres)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var selectedGenreIds = book.Genres.Select(bg => bg.GenreId).ToList();
            
            ViewBag.AuthorId = new SelectList(await _context.Author.ToListAsync(), "Id", "FullName", book.AuthorId);
            ViewBag.Genres = new SelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", book.Genres.Select(bg => bg.GenreId));
            ViewBag.SelectedGenres = selectedGenreIds;
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,YearPublished,NumPages,Description,Publisher,FrontPage,DownloadUrl,AuthorId")] Book book, int[] selectedGenres)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);

                    var existingGenres = _context.BookGenre.Where(bg => bg.BookId == id);
                    _context.BookGenre.RemoveRange(existingGenres);

                    if (selectedGenres != null)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            _context.Add(new BookGenre { BookId = book.Id, GenreId = genreId });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new MultiSelectList(await _context.Genre.ToListAsync(), "Id", "GenreName", selectedGenres);
            ViewBag.AuthorId = new SelectList(_context.Author, "Id", "FullName", book.AuthorId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.Id == id);
        }
    }
}
