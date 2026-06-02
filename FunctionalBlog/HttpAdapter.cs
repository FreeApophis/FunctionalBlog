using System.Text;

namespace FunctionalBlog;

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

        var cookies = http.Request.Cookies.ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        return new Request(
            Method: HttpMethod.Parse(http.Request.Method),
            Path: http.Request.Path.Value ?? "/",
            Headers: http.Request.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Query: http.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
            Form: form,
            Cookies: cookies);
    }

    public static async ValueTask WriteResponse(HttpContext http, Response response)
    {
        http.Response.StatusCode = response.Status;
        http.Response.ContentType = response.ContentType;

        foreach (var (key, value) in response.Headers)
        {
            http.Response.Headers[key] = value;
        }

        foreach (var cookie in response.SetCookies)
        {
            http.Response.Headers.Append("Set-Cookie", cookie);
        }

        await http.Response.WriteAsync(response.Body, Encoding.UTF8);
    }
}
