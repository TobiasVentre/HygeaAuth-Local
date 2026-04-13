using System.Net;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Service;

internal sealed class EmailTemplateRenderer
{
    private readonly string _templatesRoot;
    private readonly ILogger<EmailTemplateRenderer> _logger;

    public EmailTemplateRenderer(ILogger<EmailTemplateRenderer> logger)
    {
        _logger = logger;
        _templatesRoot = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");
    }

    public string Render(string templateName, IReadOnlyDictionary<string, string?> values, string fallbackHtml, params string[] rawTokens)
    {
        var template = LoadTemplate(templateName) ?? fallbackHtml;
        var rawTokenSet = new HashSet<string>(rawTokens, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in values)
        {
            var token = "{{" + entry.Key + "}}";
            var value = entry.Value ?? string.Empty;
            var renderedValue = rawTokenSet.Contains(entry.Key) ? value : WebUtility.HtmlEncode(value);
            template = template.Replace(token, renderedValue, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private string? LoadTemplate(string templateName)
    {
        try
        {
            var templatePath = Path.Combine(_templatesRoot, templateName);
            return File.Exists(templatePath) ? File.ReadAllText(templatePath) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar la plantilla de email {TemplateName}. Se usara fallback interno.", templateName);
            return null;
        }
    }
}
