using System.Net;

namespace EventFlow.Application.Common.Helpers;

public static class EmailHtml
{
    // Names, event titles and tenant names are user-controlled.
    // Every interpolated value goes through this.
    public static string Encode(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);

    // A call-to-action button, with the raw URL underneath as a fallback.
    public static string Button(string url, string text) => $"""
        <p style="margin:24px 0;">
            <a href="{Encode(url)}"
               style="background:#1a1a2e;color:#ffffff;padding:12px 24px;
                      text-decoration:none;border-radius:6px;display:inline-block;">
                {Encode(text)}
            </a>
        </p>
        <p style="font-size:13px;color:#6b7280;">
            If the button doesn't work, copy this link into your browser:<br>
            {Encode(url)}
        </p>
        """;

    // A big, copyable OTP block.
    public static string Code(string code) => $"""
        <p style="font-size:28px;font-weight:bold;letter-spacing:6px;
                  font-family:Consolas,monospace;background:#f3f4f6;
                  padding:12px 20px;display:inline-block;border-radius:6px;">
            {Encode(code)}
        </p>
        """;
}