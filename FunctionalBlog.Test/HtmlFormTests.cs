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

    [Fact]
    public void InputNumber_omits_autofocus_by_default()
    {
        var html = Html.InputNumber("amount", string.Empty).Render();

        Assert.DoesNotContain("autofocus", html);
    }

    [Fact]
    public void InputNumber_emits_autofocus_when_requested()
    {
        var html = Html.InputNumber("amount", string.Empty, autofocus: true).Render();

        Assert.Contains("autofocus", html);
    }
}
