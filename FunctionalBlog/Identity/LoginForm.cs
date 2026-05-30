namespace FunctionalBlog.Identity;

public static class LoginForm
{
    public static DecodedLoginForm Decode(Request request)
    {
        var email = request.Form.GetValueOrDefault("email", string.Empty).Trim();
        var password = request.Form.GetValueOrDefault("password", string.Empty);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(email))
        {
            errors.Add("Bitte geben Sie Ihre E-Mail-Adresse ein.");
        }

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Bitte geben Sie Ihr Passwort ein.");
        }

        return new DecodedLoginForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            EmailRaw: email,
            Password: password);
    }
}
