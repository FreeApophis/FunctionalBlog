namespace FunctionalBlog.Images;

public static class ImageViews
{
    public static string Library(IReadOnlyList<ImageSummary> images, ViewContext ctx, IReadOnlyList<string>? errors = null)
    {
        var (_, t, csrfToken) = ctx;

        var errorHtml = errors is { Count: > 0 }
            ? Html.Div("errors", Html.Ul(errors.Select(key => Html.Text(t(key)))))
            : HtmlString.Empty;

        var uploadBody = Html.CsrfField(csrfToken) +
            Html.Label(Html.Text(t("image.field.file")) + Html.InputFile("file", "image/*")) +
            Html.Button(t("image.upload"));
        var uploadForm = Html.Form("/images", uploadBody, enctype: "multipart/form-data");

        var gallery = images.Count == 0
            ? Html.P(Html.Text(t("image.empty")))
            : Html.Div("image-gallery", HtmlString.Concat(images.Select(image => Card(image, t, csrfToken))));

        var breadcrumb = Html.Breadcrumb(
            Crumb.Link(t("nav.admin"), "/admin"),
            Crumb.Current(t("image.library_title")));

        var body = breadcrumb +
            Html.H1(t("image.library_title")) +
            errorHtml +
            uploadForm +
            gallery;

        return Layout.Page(t("image.library_title"), body, ctx);
    }

    private static HtmlString Card(ImageSummary image, Translate t, string csrfToken)
    {
        var deleteForm = Html.Form(
            $"/images/{image.Id.Value}/delete",
            Html.CsrfField(csrfToken) + Html.Button(t("common.delete")),
            style: "display:inline",
            confirm: t("common.confirm_delete"));

        var snippet = $"[img]/images/{image.Id.Value}[/img]";
        var embedField = Html.Raw($"""<input class="embed-snippet" type="text" value="{Html.Encode(snippet)}" readonly onclick="this.select()" />""");

        var cardBody = Html.Img($"/images/{image.Id.Value}", image.FileName) +
            Html.Small(image.FileName) +
            embedField +
            deleteForm;

        return Html.Div("image-card", cardBody);
    }
}
