namespace FunctionalBlog;

public sealed record ViewContext(IPrincipal Principal, Translate T, string CsrfToken, string Theme = "light", string Language = Languages.Default)
{
    public static ViewContext ForGuest(string csrfToken = "") =>
        new(Guest.Instance, key => key, csrfToken);

    // Most views only need these three; keep the 3-arg deconstruction working alongside the
    // record's generated 4-arg one so they don't all have to spell out Theme.
    public void Deconstruct(out IPrincipal principal, out Translate t, out string csrfToken)
    {
        principal = Principal;
        t = T;
        csrfToken = CsrfToken;
    }
}
