using Microsoft.EntityFrameworkCore;
using DocumentProcessor.WebForms.Models;

namespace DocumentProcessor.WebForms.Data
{
    public class DocumentDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DocumentDbContext"/> using the specified options.
        /// The connection string is configured at DI registration time (e.g. via
        /// <c>builder.Services.AddDbContext&lt;DocumentDbContext&gt;(o =&gt; o.UseSqlServer(connectionString))</c>
        /// in Program.cs).
        /// </summary>
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
    }
}
