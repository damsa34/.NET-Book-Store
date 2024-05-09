using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Books.Models
{
    public class Book
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string Title { get; set; }
        [Display(Name = "Published in")]
        [Required]
        public int? YearPublished { get; set; }
        [Display(Name = "Number of pages")]
        [Required]
        public int? NumPages { get; set; }
        [StringLength(int.MaxValue)]
        public string? Description { get; set; }
        [StringLength(50)]
        public string? Publisher { get; set; }
        [StringLength(int.MaxValue)]
        [Display(Name = "Front cover")]
        public string? FrontPage { get; set; }
        [StringLength(int.MaxValue)]
        [Display(Name = "Download link")]
        public string? DownloadUrl { get; set; }
        [Display(Name = "Author")]
        public int AuthorId { get; set; }
        public Author? Author { get; set; }
        public ICollection<BookGenre>? Genres { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<UserBooks>? Users { get; set; }
        [Display(Name = "Average Rating")]
        public float AverageRating()
        {
            if (Reviews != null && Reviews.Any() && Reviews.All(r => r.Rating != null))
            {
                int totalRating = (int)Reviews.Sum(r => r.Rating);
                return (float)totalRating / Reviews.Count;
            }

            return 0;
        }
    }
}
