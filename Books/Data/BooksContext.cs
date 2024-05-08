using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Books.Models;

namespace Books.Data
{
    public class BooksContext : DbContext
    {
        public BooksContext (DbContextOptions<BooksContext> options)
            : base(options)
        {
        }

        public DbSet<Books.Models.Book> Book { get; set; } = default!;
        public DbSet<Books.Models.Author> Author { get; set; } = default!;
        public DbSet<Books.Models.Genre> Genre { get; set; } = default!;
        public DbSet<Books.Models.Review> Review { get; set; } = default!;
        public DbSet<Books.Models.UserBooks> UserBooks { get; set; } = default!;
        public DbSet<Books.Models.BookGenre> BookGenre { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId);

            builder.Entity<BookGenre>()
                .HasOne(b => b.Book)
                .WithMany(a => a.Genres)
                .HasForeignKey(b => b.BookId);

            builder.Entity<BookGenre>()
                .HasOne(b => b.Genre)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.GenreId);

            builder.Entity<Review>()
                .HasOne(b => b.Book)
                .WithMany(a => a.Reviews)
                .HasForeignKey(b => b.BookId);

            builder.Entity<UserBooks>()
                .HasOne(b => b.Book)
                .WithMany(a => a.Users)
                .HasForeignKey(b => b.BookId);

            builder.Entity<Author>().Ignore(a => a.FullName);
        }
    }
}
