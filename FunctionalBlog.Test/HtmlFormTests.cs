using FunctionalBlog;

public class HtmlFormTests
{
    [Fact]
    public void Form_without_confirm_emits_no_data_confirm_attribute()
    {
        var html = Html.Form("/x", Html.Button("Go")).Render();

        Assert.DoesNotContain("data-confirm", html);
    }

    [Fact]
    public void Form_with_confirm_emits_encoded_data_confirm_attribute()
    {
        var html = Html.Form("/x", Html.Button("Go"), confirm: "Delete \"this\"?").Render();

        Assert.Contains("data-confirm=\"Delete &quot;this&quot;?\"", html);
    }
}
