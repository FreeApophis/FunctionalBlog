using System.Text;

public static class HttpAdapter
{
    public static async ValueTask<Request> ToRequest(HttpContext http)
    {
        var form = new Dictionary<string, string>();

        if (http.Request.HasFormContentType)
        {
            var parsed = await http.Request.ReadFormAsync();
            form = parsed.ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        return new Request(
            Method: http.Request.Method.ToUpperInvariant(),
            Path: http.Request.Path.Value ?? "/",
            Headers: http.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Query: http.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Form: form);
    }

    public static async ValueTask WriteResponse(HttpContext http, Response response)
    {
        http.Response.StatusCode = response.Status;
        http.Response.ContentType = response.ContentType;

        foreach (var (key, value) in response.Headers)
        {
            http.Response.Headers[key] = value;
        }

        await http.Response.WriteAsync(response.Body, Encoding.UTF8);
    }
}
