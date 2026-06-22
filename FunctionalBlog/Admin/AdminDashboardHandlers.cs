namespace FunctionalBlog.Admin;

public static class AdminDashboardHandlers
{
    // The /admin landing page. Reachable by any authenticated user (the route gates with
    // RequireAuth); the view itself only renders the cards the principal has access to.
    public static App Dashboard => _ => env =>
        ValueTask.FromResult(Response.Html(AdminDashboardViews.Dashboard(env.Ctx)));
}
