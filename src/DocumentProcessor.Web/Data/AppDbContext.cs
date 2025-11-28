using Microsoft.EntityFrameworkCore;
using DocumentProcessor.Web.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace DocumentProcessor.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Document>(e =>
        {
            // Table mapping with schema
            e.ToTable("documents", "dps_dbo");
            
            // Primary key
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id");
            
            // Column mappings for existing configured properties
            e.Property(d => d.FileName).IsRequired().HasMaxLength(500).HasColumnName("filename");
            e.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(500).HasColumnName("originalfilename");
            e.Property(d => d.FileExtension).HasMaxLength(50).HasColumnName("fileextension");
            e.Property(d => d.ContentType).HasMaxLength(100).HasColumnName("contenttype");
            e.Property(d => d.StoragePath).HasMaxLength(1000).HasColumnName("storagepath");
            e.Property(d => d.UploadedBy).IsRequired().HasMaxLength(255).HasColumnName("uploadedby");
            e.Property(d => d.DocumentTypeName).HasMaxLength(255).HasColumnName("documenttypename");
            e.Property(d => d.DocumentTypeCategory).HasMaxLength(100).HasColumnName("documenttypecategory");
            e.Property(d => d.ProcessingStatus).HasMaxLength(50).HasColumnName("processingstatus");
            e.Property(d => d.ProcessingErrorMessage).HasMaxLength(1000).HasColumnName("processingerrormessage");
            e.Property(d => d.Status).HasColumnName("status");
            e.Property(d => d.IsDeleted).HasColumnName("isdeleted").HasConversion<int>();
            e.Property(d => d.UploadedAt).HasColumnName("uploadedat");
            
            // Column mappings for properties not previously configured
            e.Property(d => d.FileSize).HasColumnName("filesize");
            e.Property(d => d.Source).HasColumnName("source");
            e.Property(d => d.ProcessingRetryCount).HasColumnName("processingretrycount");
            e.Property(d => d.ProcessingStartedAt).HasColumnName("processingstartedat");
            e.Property(d => d.ProcessingCompletedAt).HasColumnName("processingcompletedat");
            e.Property(d => d.ExtractedText).HasColumnName("extractedtext");
            e.Property(d => d.Summary).HasColumnName("summary");
            e.Property(d => d.ProcessedAt).HasColumnName("processedat");
            e.Property(d => d.CreatedAt).HasColumnName("createdat");
            e.Property(d => d.UpdatedAt).HasColumnName("updatedat");
            e.Property(d => d.DeletedAt).HasColumnName("deletedat");
            
            // Preserve existing indexes and query filters
            e.HasIndex(d => d.Status);
            e.HasIndex(d => d.UploadedAt);
            e.HasIndex(d => d.IsDeleted);
            e.HasQueryFilter(d => !d.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<Document>().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (e.State == EntityState.Added) e.Entity.CreatedAt = DateTime.UtcNow;
            e.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}
