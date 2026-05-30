namespace FunctionalBlog.Roles;

public sealed record DecodedRuleForm(
    bool IsValid,
    IReadOnlyList<string> Errors,
    string ActionName,
    string ResourceKey);
