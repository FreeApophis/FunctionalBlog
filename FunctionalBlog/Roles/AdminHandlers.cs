namespace FunctionalBlog.Roles;

public static class AdminHandlers
{
    public static App UserList => _ => async env =>
    {
        var users = await env.Users.All();
        return Response.Html(AdminViews.UserList(users, env.CurrentUser, env.T));
    };

    public static App UserDetail(int userId) => _ => async env =>
    {
        if ((await env.Users.FindById(new UserId(userId))) is [var user])
        {
            var roles = await env.Roles.All();
            return Response.Html(AdminViews.UserDetail(user, roles, [], env.CurrentUser, env.T));
        }

        return Response.NotFound();
    };

    public static App UpdateUserRoles(int userId) => request => async env =>
    {
        if ((await env.Users.FindById(new UserId(userId))) is [var user])
        {
            var decoded = AssignRoleForm.Decode(request);
            await env.Users.Save(user with { RoleNames = decoded.RoleNames });
            return Response.Redirect($"/admin/users/{userId}");
        }

        return Response.NotFound();
    };

    public static App RoleList => _ => async env =>
    {
        var roles = await env.Roles.All();
        return Response.Html(AdminViews.RoleList(roles, env.CurrentUser, env.T));
    };

    public static App NewRoleForm => _ => env =>
        ValueTask.FromResult(Response.Html(AdminViews.NewRoleForm([], env.CurrentUser, env.T)));

    public static App CreateRole => request => async env =>
    {
        var decoded = RoleForm.Decode(request);

        if (!decoded.IsValid)
        {
            return Response.Html(AdminViews.NewRoleForm(decoded.Errors, env.CurrentUser, env.T), 400);
        }

        var id = await env.Roles.NextId();
        await env.Roles.Save(Role.Create(id, decoded.Name));
        return Response.Redirect("/admin/roles");
    };

    public static App RoleDetail(int roleId) => _ => async env =>
    {
        if ((await env.Roles.FindById(new RoleId(roleId))) is [var role])
        {
            return Response.Html(AdminViews.RoleDetail(role, [], env.CurrentUser, env.T));
        }

        return Response.NotFound();
    };

    public static App AddRule(int roleId) => request => async env =>
        await (await env.Roles.FindById(new RoleId(roleId))).Match(
            none: () => Task.FromResult(Response.NotFound()),
            some: async role =>
            {
                var decoded = RuleForm.Decode(request);

                if (!decoded.IsValid)
                {
                    return Response.Html(AdminViews.RoleDetail(role, decoded.Errors, env.CurrentUser, env.T), 400);
                }

                var rule = new PermissionRule(decoded.ActionName, decoded.ResourceKey);

                if (!role.Rules.Contains(rule))
                {
                    await env.Roles.Save(role.AddRule(rule));
                }

                return Response.Redirect($"/admin/roles/{roleId}");
            });

    public static App DeleteRule(int roleId) => request => async env =>
        await (await env.Roles.FindById(new RoleId(roleId))).Match(
            none: () => Task.FromResult(Response.NotFound()),
            some: async role =>
            {
                var decoded = RuleForm.Decode(request);
                var rule = new PermissionRule(decoded.ActionName, decoded.ResourceKey);
                await env.Roles.Save(role.RemoveRule(rule));
                return Response.Redirect($"/admin/roles/{roleId}");
            });

    public static App DeleteRole(int roleId) => _ => async env =>
    {
        if ((await env.Roles.FindById(new RoleId(roleId))) is [var role])
        {
            await env.Roles.Delete(new RoleId(roleId));
            return Response.Redirect("/admin/roles");
        }

        return Response.NotFound();
    };
}
