namespace FunctionalBlog.Test;

public sealed class TestLog : ILog
{
    private readonly List<string> _messages = [];

    public void Info(string message) => _messages.Add(message);

    public void Error(Exception exception) => _messages.Add(exception.ToString());

    public string ExtractResetToken()
    {
        var line = _messages.FirstOrDefault(m => m.Contains("reset-token:"));
        if (line is null)
        {
            throw new InvalidOperationException("No reset token found in log messages.");
        }

        var marker = "reset-token:";
        var start = line.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return line[start..].Trim().Split(' ')[0];
    }
}
