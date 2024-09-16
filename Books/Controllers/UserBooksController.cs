using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Models;
using Microsoft.AspNetCore.Authorization;

namespace Books.Controllers
{
    public class UserBooksController : Controller
    {
        private readonly BooksContext _context;

        public UserBooksController(BooksContext context)
        {
            _context = context;
        }

        // GET: UserBooks
        public async Task<IActionResult> Index()
        {
            var booksContext = _context.UserBooks.Include(u => u.Book);
            return View(await booksContext.ToListAsync());
        }

        // GET: UserBooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userBooks = await _context.UserBooks
                .Include(u => u.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userBooks == null)
            {
                return NotFound();
            }

            return View(userBooks);
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyBooks()
        {
            var userId = User.Identity.Name;
            var myBooks = await _context.UserBooks
                .Include(ub => ub.Book)
                .ThenInclude(b => b.Author)
                .Where(ub => ub.AppUser == userId)
                .ToListAsync();

            var distinctBooks = myBooks
                .GroupBy(ub => ub.BookId)
                .Select(g => g.First())
                .ToList();

            return View(distinctBooks);
        }

        [HttpPost]
        public async Task<IActionResult> EmptyCollection()
        {
            var userId = User.Identity.Name;
            var userBooks = _context.UserBooks
                .Where(ub => ub.AppUser == userId);

            _context.UserBooks.RemoveRange(userBooks);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBooks));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            var userId = User.Identity.Name;
            var userBooks = _context.UserBooks.Where(ub => ub.AppUser == userId && ub.BookId == bookId);

            _context.UserBooks.RemoveRange(userBooks); 
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBooks));
        }

        // GET: UserBooks/Create
        [Authorize(Roles = "Admin, User")]
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Book, "Id", "Title");
            return View();
        }

        // POST: UserBooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Create([Bind("Id,AppUser,BookId")] UserBooks userBooks)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userBooks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Book, "Id", "Title", userBooks.BookId);
            return View(userBooks);
        }

        // GET: UserBooks/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userBooks = await _context.UserBooks.FindAsync(id);
            if (userBooks == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Book, "Id", "Title", userBooks.BookId);
            return View(userBooks);
        }

        // POST: UserBooks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUser,BookId")] UserBooks userBooks)
        {
            if (id != userBooks.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userBooks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserBooksExists(userBooks.Id))
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
            ViewData["BookId"] = new SelectList(_context.Book, "Id", "Title", userBooks.BookId);
            return View(userBooks);
        }

        // GET: UserBooks/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userBooks = await _context.UserBooks
                .Include(u => u.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userBooks == null)
            {
                return NotFound();
            }

            return View(userBooks);
        }

        // POST: UserBooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userBooks = await _context.UserBooks.FindAsync(id);
            if (userBooks != null)
            {
                _context.UserBooks.Remove(userBooks);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserBooksExists(int id)
        {
            return _context.UserBooks.Any(e => e.Id == id);
        }
    }
}
