namespace FunctionalBlog.Roles;

public static class RoleForm
{
    public static DecodedRoleForm Decode(Request request)
    {
        var name = request.Form.GetValueOrDefault("name", string.Empty).Trim();
        var errors = new List<string>();

        if (string.IsNullOrEmpty(name))
        {
            errors.Add("Der Rollenname darf nicht leer sein.");
        }

        return new DecodedRoleForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Name: name);
    }
}
