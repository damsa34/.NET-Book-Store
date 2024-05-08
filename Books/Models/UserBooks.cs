using System.ComponentModel.DataAnnotations;

namespace Books.Models
{
    public class UserBooks
    {
        public int Id { get; set; }
        [StringLength(450)]
        [Display(Name = "Username")]
        [Required]
        public string AppUser { get; set; }
        [Display(Name = "Book")]
        [Required]
        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
