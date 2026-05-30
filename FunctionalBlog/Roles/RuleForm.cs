namespace FunctionalBlog.Roles;

public static class RuleForm
{
    public static DecodedRuleForm Decode(Request request)
    {
        var action = request.Form.GetValueOrDefault("action", string.Empty).Trim();
        var resource = request.Form.GetValueOrDefault("resource", string.Empty).Trim();
        var errors = new List<string>();

        if (string.IsNullOrEmpty(action))
        {
            errors.Add("Bitte wählen Sie eine Aktion aus.");
        }

        if (string.IsNullOrEmpty(resource))
        {
            errors.Add("Bitte wählen Sie eine Ressource aus.");
        }

        return new DecodedRuleForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            ActionName: action,
            ResourceKey: resource);
    }
}
