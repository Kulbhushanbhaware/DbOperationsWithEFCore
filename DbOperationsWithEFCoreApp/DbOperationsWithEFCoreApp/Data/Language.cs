using System.Collections;
using System.Text.Json.Serialization;

namespace DbOperationsWithEFCoreApp.Data
{
    public class Language
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        //[JsonIgnore] // 👈 Prevent cyclic reference
        public virtual ICollection<Book> Books { get; set; } // Navigation property, one-to-many relationship
    }
}
