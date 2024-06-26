﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Books.Data;
using Books.Models;
using Books.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Books.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly BooksContext _context;

        public AuthorsController(BooksContext context)
        {
            _context = context;
        }

        // GET: Authors
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Author> authors = _context.Author.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                authors = authors.Where(a => a.FirstName.Contains(searchString) 
                || a.LastName.Contains(searchString));
            }

            var authorList = await authors.ToListAsync();

            return View(authorList);
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Author
                .Include(a => a.Books)
                .ThenInclude(b => b.Genres)
                .ThenInclude(b => b.Genre)
                .Include(a => a.Books)
                .ThenInclude(b => b.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            foreach (var book in author.Books)
            {
                var updatedBook = await _context.Book
                    .Include(b => b.Genres)
                    .ThenInclude(b => b.Genre)
                    .Distinct()
                    .FirstOrDefaultAsync(b => b.Id == book.Id);

                if (updatedBook != null)
                {
                    book.Genres = updatedBook.Genres;
                }
            }

            return View(author);
        }

        // GET: Authors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Nationality,Gender")] Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = _context.Author.Include(a => a.Books).FirstOrDefault(a => a.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            var allBooks = _context.Book.ToList();

            var bookOptions = allBooks.Select(book => new SelectListItem
            {
                Value = book.Id.ToString(),
                Text = book.Title
            }).ToList();

            var selectedBooks = author.Books.Select(book => book.Id).ToList();

            var viewModel = new AuthorViewModel
            {
                Author = author,
                BookOptions = new SelectList(bookOptions, "Value", "Text"),
                SelectedBooks = selectedBooks
            };

            return View(viewModel);
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,BirthDate,Nationality,Gender")] Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.Id))
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
            return View(author);
        }

        // GET: Authors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _context.Author
                .FirstOrDefaultAsync(m => m.Id == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Author.FindAsync(id);
            if (author != null)
            {
                _context.Author.Remove(author);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Author.Any(e => e.Id == id);
        }
    }
}
