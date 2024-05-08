using System.ComponentModel.DataAnnotations;

namespace Books.Models
{
    public class Genre
    {
        public int Id { get; set; }
        [StringLength(50)]
        [Required]
        [Display(Name = "Genre name")]
        public string GenreName { get; set; }
        public ICollection<BookGenre>? Books { get; set; }
    }
}
