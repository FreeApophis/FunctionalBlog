namespace FunctionalBlog;

public sealed record UploadedFile(
    string FieldName,
    string FileName,
    string ContentType,
    byte[] Content);
