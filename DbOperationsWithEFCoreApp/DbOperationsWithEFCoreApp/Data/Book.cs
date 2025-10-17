namespace DbOperationsWithEFCoreApp.Data
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NoOfPages { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }  // Timestamp of when the book record was created
        public int LanguageId { get; set; }  // Foreign key for Language
        public int? AuthorId { get; set; }  // Foreign key for Author (nullable)
        // 👇 These are navigation properties
        //public Language? Language { get; set; } // Navigation property for Language
        public virtual Author? Author { get; set; }  // Navigation property for Author
    }
}
