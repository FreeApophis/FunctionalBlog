namespace Bbcode.Test;

public class BbcodeRendererTests
{
    [Fact]
    public void Plain_text_becomes_a_paragraph()
    {
        Assert.Equal("<p>Hallo Welt</p>", BbcodeRenderer.RenderToHtml("Hallo Welt"));
    }

    [Fact]
    public void Blank_lines_separate_paragraphs()
    {
        Assert.Equal("<p>Eins</p><p>Zwei</p>", BbcodeRenderer.RenderToHtml("Eins\n\nZwei"));
    }

    [Fact]
    public void Single_newline_becomes_a_line_break()
    {
        Assert.Equal("<p>Eins<br />Zwei</p>", BbcodeRenderer.RenderToHtml("Eins\nZwei"));
    }

    [Fact]
    public void Bold_and_italic_render_to_strong_and_em()
    {
        Assert.Equal(
            "<p>Ein <strong>fetter</strong> und <em>kursiver</em> Text</p>",
            BbcodeRenderer.RenderToHtml("Ein [b]fetter[/b] und [i]kursiver[/i] Text"));
    }

    [Fact]
    public void Img_tag_renders_a_gallery_image()
    {
        var html = BbcodeRenderer.RenderToHtml("[img]/images/5[/img]");

        Assert.Contains("""<img src="/images/5" """, html);
        Assert.Contains("loading=\"lazy\"", html);
    }

    [Fact]
    public void Url_tag_renders_a_link()
    {
        Assert.Equal(
            """<p><a href="/pages/3">Impressum</a></p>""",
            BbcodeRenderer.RenderToHtml("[url=/pages/3]Impressum[/url]"));
    }

    [Fact]
    public void Absolute_http_urls_are_allowed()
    {
        var html = BbcodeRenderer.RenderToHtml("[url=https://example.com]Extern[/url]");

        Assert.Contains("""<a href="https://example.com">Extern</a>""", html);
    }

    [Fact]
    public void Literal_html_is_encoded_not_executed()
    {
        var html = BbcodeRenderer.RenderToHtml("<script>alert('x')</script>");

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void Javascript_urls_are_not_turned_into_links()
    {
        var html = BbcodeRenderer.RenderToHtml("[url=javascript:alert(1)]klick[/url]");

        Assert.DoesNotContain("<a ", html);
    }

    [Fact]
    public void Javascript_image_sources_are_not_rendered_as_images()
    {
        var html = BbcodeRenderer.RenderToHtml("[img]javascript:alert(1)[/img]");

        Assert.DoesNotContain("<img", html);
    }

    [Fact]
    public void A_quote_inside_a_url_cannot_break_out_of_the_attribute()
    {
        var html = BbcodeRenderer.RenderToHtml("""[img]/images/5" onerror="alert(1)[/img]""");

        Assert.DoesNotContain("onerror=\"alert", html);
    }

    [Fact]
    public void Unknown_tags_pass_through_as_encoded_text()
    {
        Assert.Equal("<p>[blink]hi[/blink]</p>", BbcodeRenderer.RenderToHtml("[blink]hi[/blink]"));
    }
}
