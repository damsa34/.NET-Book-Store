using System.ComponentModel.DataAnnotations;

namespace Books.Models
{
    public class Review
    {
        public int Id { get; set; }
        [Display(Name = "Book")]
        public int BookId { get; set; }
        [StringLength(450)]
        [Required]
        [Display(Name = "Username")]
        public string AppUser { get; set; }
        [StringLength(500)]
        public string Comment { get; set; }
        [Range(1, 10)]
        public int? Rating { get; set; }
        public Book? Book { get; set; }
    }
}
