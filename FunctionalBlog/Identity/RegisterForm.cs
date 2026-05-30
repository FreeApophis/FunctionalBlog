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

        if (email is null)
        {
            errors.Add("Bitte geben Sie eine gültige E-Mail-Adresse ein.");
        }

        if (displayName.Length < 2)
        {
            errors.Add("Der Anzeigename muss mindestens 2 Zeichen lang sein.");
        }

        if (password.Length < 8)
        {
            errors.Add("Das Passwort muss mindestens 8 Zeichen lang sein.");
        }

        if (password != confirmation)
        {
            errors.Add("Die Passwörter stimmen nicht überein.");
        }

        return new DecodedRegisterForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Email: email,
            DisplayName: displayName,
            Password: password);
    }
}
