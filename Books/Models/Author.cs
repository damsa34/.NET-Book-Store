using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Books.Models
{
    public class Author
    {
        public int Id { get; set; }
        [StringLength(50)]
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [StringLength(50)]
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Display(Name = "Birth date")]
        public DateOnly? BirthDate { get; set; }
        [StringLength(50)]
        public string? Nationality { get; set; }
        [StringLength(50)]
        public string? Gender { get; set; }
        public ICollection<Book>? Books { get; set; }
        [Display(Name = "Author")]
        [NotMapped]
        public string FullName => FirstName + " " + LastName;
    }
}
