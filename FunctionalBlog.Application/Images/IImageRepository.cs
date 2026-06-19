namespace FunctionalBlog.Application.Images;

public interface IImageRepository
{
    ValueTask<ImageId> NextId();

    ValueTask Save(Image image);

    ValueTask<Option<Image>> Find(ImageId id);

    ValueTask<IReadOnlyList<ImageSummary>> List();

    ValueTask Delete(ImageId id);
}
