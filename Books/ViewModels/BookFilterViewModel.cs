using Books.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Books.ViewModels
{
    public class BookFilterViewModel
    {
        public IList<Book>? TemporaryBooks { get; set; }
        public SelectList? FilterOptions { get; set; }
        public string SearchString { get; set; }
    }
}
