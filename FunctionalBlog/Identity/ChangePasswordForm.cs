namespace FunctionalBlog.Identity;

public static class ChangePasswordForm
{
    public static DecodedChangePasswordForm Decode(Request request)
    {
        var current = request.Form.GetValueOrDefault("current", string.Empty);
        var password = request.Form.GetValueOrDefault("password", string.Empty);
        var confirmation = request.Form.GetValueOrDefault("confirmation", string.Empty);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(current))
        {
            errors.Add("Bitte geben Sie Ihr aktuelles Passwort ein.");
        }

        if (password.Length < 8)
        {
            errors.Add("Das neue Passwort muss mindestens 8 Zeichen lang sein.");
        }

        if (password != confirmation)
        {
            errors.Add("Die Passwörter stimmen nicht überein.");
        }

        return new DecodedChangePasswordForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            CurrentPassword: current,
            NewPassword: password);
    }
}
