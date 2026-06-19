namespace FunctionalBlog.Images;

public static class ImageHandlers
{
    public static App Serve(ImageId id) => _ => async env =>
    {
        if ((await env.Images.Find(id)) is [var image])
        {
            return Response.Bytes(
                image.ContentType.Value,
                image.Data,
                new Dictionary<string, string> { ["Cache-Control"] = "public, max-age=31536000, immutable" });
        }

        return Response.NotFound(env.Ctx);
    };

    public static App Library => _ => async env =>
        Response.Html(ImageViews.Library(await env.Images.List(), env.Ctx));

    public static App Upload => request => async env =>
        await ImageUploadForm.Decode(request).Match(
            failure: async f =>
                Response.Html(ImageViews.Library(await env.Images.List(), env.Ctx, f.Error), 400),
            success: async s =>
            {
                var image = Image.Create(
                    id: await env.Images.NextId(),
                    fileName: s.Value.FileName,
                    contentType: s.Value.ContentType,
                    data: s.Value.Content,
                    uploadedBy: ((AuthenticatedUser)env.CurrentUser).Id,
                    createdAt: env.Clock.Now);

                await env.Images.Save(image);
                return Response.Redirect("/images");
            });

    public static App Delete(ImageId id) => _ => async env =>
    {
        await env.Images.Delete(id);
        return Response.Redirect("/images");
    };
}
