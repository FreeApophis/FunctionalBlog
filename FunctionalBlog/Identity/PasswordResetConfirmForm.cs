namespace FunctionalBlog.Identity;

public static class PasswordResetConfirmForm
{
    public static DecodedPasswordResetConfirmForm Decode(Request request)
    {
        var token = request.Form.GetValueOrDefault("token", string.Empty).Trim();
        var password = request.Form.GetValueOrDefault("password", string.Empty);
        var confirmation = request.Form.GetValueOrDefault("confirmation", string.Empty);

        var errors = new List<string>();

        if (string.IsNullOrEmpty(token))
        {
            errors.Add("Ungültiger oder fehlender Reset-Token.");
        }

        if (password.Length < 8)
        {
            errors.Add("Das Passwort muss mindestens 8 Zeichen lang sein.");
        }

        if (password != confirmation)
        {
            errors.Add("Die Passwörter stimmen nicht überein.");
        }

        return new DecodedPasswordResetConfirmForm(
            IsValid: errors.Count == 0,
            Errors: errors,
            Token: token,
            Password: password);
    }
}
