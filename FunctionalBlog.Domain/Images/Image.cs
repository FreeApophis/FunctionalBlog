namespace FunctionalBlog.Domain.Images;

public sealed record Image(
    ImageId Id,
    string FileName,
    ImageContentType ContentType,
    byte[] Data,
    int ByteSize,
    UserId UploadedBy,
    DateTimeOffset CreatedAt)
{
    public static Image Create(
        ImageId id,
        string fileName,
        ImageContentType contentType,
        byte[] data,
        UserId uploadedBy,
        DateTimeOffset createdAt) =>
        new(id, fileName, contentType, data, data.Length, uploadedBy, createdAt);
}
