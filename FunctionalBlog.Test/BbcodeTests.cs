namespace FunctionalBlog.Test;

public class BbcodeTests
{
    [Fact]
    public void Plain_text_becomes_a_paragraph()
    {
        Assert.Equal("<p>Hallo Welt</p>", Bbcode.Render("Hallo Welt").Render());
    }

    [Fact]
    public void Blank_lines_separate_paragraphs()
    {
        Assert.Equal("<p>Eins</p><p>Zwei</p>", Bbcode.Render("Eins\n\nZwei").Render());
    }

    [Fact]
    public void Single_newline_becomes_a_line_break()
    {
        Assert.Equal("<p>Eins<br />Zwei</p>", Bbcode.Render("Eins\nZwei").Render());
    }

    [Fact]
    public void Bold_and_italic_render_to_strong_and_em()
    {
        Assert.Equal(
            "<p>Ein <strong>fetter</strong> und <em>kursiver</em> Text</p>",
            Bbcode.Render("Ein [b]fetter[/b] und [i]kursiver[/i] Text").Render());
    }

    [Fact]
    public void Img_tag_renders_a_gallery_image()
    {
        var html = Bbcode.Render("[img]/images/5[/img]").Render();

        Assert.Contains("""<img src="/images/5" """, html);
        Assert.Contains("loading=\"lazy\"", html);
    }

    [Fact]
    public void Url_tag_renders_a_link()
    {
        Assert.Equal(
            """<p><a href="/pages/3">Impressum</a></p>""",
            Bbcode.Render("[url=/pages/3]Impressum[/url]").Render());
    }

    [Fact]
    public void Absolute_http_urls_are_allowed()
    {
        var html = Bbcode.Render("[url=https://example.com]Extern[/url]").Render();

        Assert.Contains("""<a href="https://example.com">Extern</a>""", html);
    }

    [Fact]
    public void Literal_html_is_encoded_not_executed()
    {
        var html = Bbcode.Render("<script>alert('x')</script>").Render();

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void Javascript_urls_are_not_turned_into_links()
    {
        var html = Bbcode.Render("[url=javascript:alert(1)]klick[/url]").Render();

        Assert.DoesNotContain("<a ", html);
    }

    [Fact]
    public void Javascript_image_sources_are_not_rendered_as_images()
    {
        var html = Bbcode.Render("[img]javascript:alert(1)[/img]").Render();

        Assert.DoesNotContain("<img", html);
    }

    [Fact]
    public void A_quote_inside_a_url_cannot_break_out_of_the_attribute()
    {
        var html = Bbcode.Render("""[img]/images/5" onerror="alert(1)[/img]""").Render();

        Assert.DoesNotContain("onerror=\"alert", html);
    }

    [Fact]
    public void Unknown_tags_pass_through_as_encoded_text()
    {
        Assert.Equal("<p>[blink]hi[/blink]</p>", Bbcode.Render("[blink]hi[/blink]").Render());
    }
}
