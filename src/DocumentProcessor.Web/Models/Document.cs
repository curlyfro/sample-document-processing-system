using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentProcessor.Web.Models;

public enum DocumentStatus { Pending, Queued, Processing, Processed, Failed }
public enum DocumentSource { LocalUpload, S3, FileShare, Email }

[Table("documents", Schema = "dps_dbo")]
public class Document
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("filename")]
    public string FileName { get; set; } = string.Empty;
    [Column("originalfilename")]
    public string OriginalFileName { get; set; } = string.Empty;
    [Column("fileextension")]
    public string FileExtension { get; set; } = string.Empty;
    [Column("filesize")]
    public long FileSize { get; set; }
    [Column("contenttype")]
    public string ContentType { get; set; } = string.Empty;
    [Column("storagepath")]
    public string StoragePath { get; set; } = string.Empty;
    [Column("source")]
    public DocumentSource Source { get; set; }
    [Column("status")]
    public DocumentStatus Status { get; set; }
    [Column("documenttypename")]
    public string? DocumentTypeName { get; set; }
    [Column("documenttypecategory")]
    public string? DocumentTypeCategory { get; set; }
    [Column("processingstatus")]
    public string? ProcessingStatus { get; set; }
    [Column("processingretrycount")]
    public int ProcessingRetryCount { get; set; }
    [Column("processingerrormessage")]
    public string? ProcessingErrorMessage { get; set; }
    [Column("processingstartedat")]
    public DateTime? ProcessingStartedAt { get; set; }
    [Column("processingcompletedat")]
    public DateTime? ProcessingCompletedAt { get; set; }
    [Column("extractedtext")]
    public string? ExtractedText { get; set; }
    [Column("summary")]
    public string? Summary { get; set; }
    [Column("uploadedat")]
    public DateTime UploadedAt { get; set; }
    [Column("processedat")]
    public DateTime? ProcessedAt { get; set; }
    [Column("uploadedby")]
    public string UploadedBy { get; set; } = string.Empty;
    [Column("createdat")]
    public DateTime CreatedAt { get; set; }
    [Column("updatedat")]
    public DateTime UpdatedAt { get; set; }
    [Column("isdeleted")]
    public bool IsDeleted { get; set; }
    [Column("deletedat")]
    public DateTime? DeletedAt { get; set; }
}
