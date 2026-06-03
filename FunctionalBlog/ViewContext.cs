namespace FunctionalBlog;

public sealed record ViewContext(IPrincipal Principal, Translate T, string CsrfToken)
{
    public static ViewContext ForGuest(string csrfToken = "") =>
        new(Guest.Instance, key => key, csrfToken);
}
