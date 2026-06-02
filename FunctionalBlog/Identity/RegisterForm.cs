namespace FunctionalBlog.Identity;

public static class RegisterForm
{
    public static DecodedRegisterForm Decode(Request request)
    {
        var emailRaw = request.Form.GetValueOrDefault("email", string.Empty).Trim();
        var displayName = request.Form.GetValueOrDefault("displayName", string.Empty).Trim();
        var password = request.Form.GetValueOrDefault("password", string.Empty);
        var confirmation = request.Form.GetValueOrDefault("confirmation", string.Empty);

        var errors = new List<string>();
        var email = Email.Parse(emailRaw);

        if (email is [])
        {
            errors.Add("auth.error.invalid_email");
        }

        if (displayName.Length < 2)
        {
            errors.Add("auth.error.display_name_too_short");
        }

        if (password.Length < 8)
        {
            errors.Add("auth.error.password_too_short");
        }

        if (password != confirmation)
        {
            errors.Add("auth.error.passwords_mismatch");
        }

        return new DecodedRegisterForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Email: email,
            DisplayName: displayName,
            Password: password);
    }
}
