namespace FunctionalBlog;

public sealed record ViewContext(IPrincipal Principal, Translate T, string CsrfToken, string Theme = "light", string Language = Languages.Default, SlugIndex? Slugs = null, string SiteName = "foodblog.ch")
{
    public static ViewContext ForGuest(string csrfToken = "") =>
        new(Guest.Instance, key => key, csrfToken);

    // The canonical path for an entity (e.g. "/recipes/ruehrkuchen"), falling back to the numeric
    // id when no slug is registered or no slug index was threaded in (guest/test contexts).
    public string Url(string entityType, int id) => (Slugs ?? SlugIndex.Empty).Url(entityType, id);

    // The bare slug segment for an entity (numeric id when unregistered) — for callers that build
    // their own URL around it, such as SEO canonical links.
    public string SlugFor(string entityType, int id) => (Slugs ?? SlugIndex.Empty).For(entityType, id);

    // The canonical /tag/{slug} path for a tag referenced by name.
    public string TagUrl(string name) => (Slugs ?? SlugIndex.Empty).TagUrl(name);

    // Most views only need these three; keep the 3-arg deconstruction working alongside the
    // record's generated 4-arg one so they don't all have to spell out Theme.
    public void Deconstruct(out IPrincipal principal, out Translate t, out string csrfToken)
    {
        principal = Principal;
        t = T;
        csrfToken = CsrfToken;
    }
}
