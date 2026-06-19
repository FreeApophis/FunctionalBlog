namespace FunctionalBlog.Test.Images;

public abstract class ImageRepositoryContract
{
    [Fact]
    public async Task Save_then_Find_returns_the_saved_image()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        var image = AnImage(id);

        await repo.Save(image);

        var found = FunctionalAssert.Some(await repo.Find(id));
        Assert.Equal(image.Id, found.Id);
        Assert.Equal(image.FileName, found.FileName);
        Assert.Equal(image.ContentType, found.ContentType);
        Assert.Equal(image.ByteSize, found.ByteSize);
        Assert.Equal(image.UploadedBy, found.UploadedBy);
        Assert.True(image.Data.SequenceEqual(found.Data));
    }

    [Fact]
    public async Task Find_returns_none_for_an_unknown_id()
    {
        var repo = CreateRepository();

        FunctionalAssert.None(await repo.Find(new ImageId(987_654)));
    }

    [Fact]
    public async Task NextId_returns_an_id_that_does_not_yet_exist()
    {
        var repo = CreateRepository();

        var id = await repo.NextId();

        FunctionalAssert.None(await repo.Find(id));
    }

    [Fact]
    public async Task NextId_returns_distinct_values_across_calls()
    {
        var repo = CreateRepository();

        var first = await repo.NextId();
        var second = await repo.NextId();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public async Task List_returns_metadata_for_saved_images()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        await repo.Save(AnImage(id, fileName: "katze.png"));

        var summary = Assert.Single(await repo.List(), s => s.Id == id);

        Assert.Equal("katze.png", summary.FileName);
        Assert.Equal(ImageContentType.Png, summary.ContentType);
        Assert.Equal(4, summary.ByteSize);
    }

    [Fact]
    public async Task Delete_removes_the_image()
    {
        var repo = CreateRepository();
        var id = await repo.NextId();
        await repo.Save(AnImage(id));

        await repo.Delete(id);

        FunctionalAssert.None(await repo.Find(id));
    }

    [Fact]
    public async Task Delete_is_idempotent_for_unknown_id()
    {
        var repo = CreateRepository();

        await repo.Delete(new ImageId(987_654));
    }

    protected abstract IImageRepository CreateRepository();

    private static Image AnImage(ImageId id, string fileName = "bild.png") =>
        Image.Create(
            id,
            fileName,
            ImageContentType.Png,
            [0x89, 0x50, 0x4E, 0x47],
            new UserId(1),
            new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero));
}
