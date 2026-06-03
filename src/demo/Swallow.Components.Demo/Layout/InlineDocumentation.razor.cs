using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class InlineDocumentation : ComponentBase
{
    private static readonly Assembly? Host = Assembly.GetEntryAssembly();
    private XElement? documentation;

    [Parameter]
    [EditorRequired]
    public required Type Type { get; set; }

    [Parameter]
    public string? Property { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var document = await LoadDocumentationAsync(Type.Assembly);
        if (document is null)
        {
            return;
        }

        var targetMember = Property is null
            ? $"T:{Type.FullName}"
            : $"P:{Type.FullName}.{Property}";

        documentation = document.XPathSelectElement($"//member[@name='{targetMember}']");
        if (documentation is null)
        {
            return;
        }

        var referencesToAdjust = documentation.Descendants("see").ToList();
        foreach (var reference in referencesToAdjust)
        {
            var referencedMember = reference.Attribute("cref")?.Value;
            if (referencedMember is not null && referencedMember.StartsWith("T:"))
            {
                var typeName = referencedMember.Split('.').Last();
                reference.ReplaceWith(new XText(typeName));

                continue;
            }

            if (referencedMember is not null && referencedMember.StartsWith("P:"))
            {
                var propertyName = referencedMember.Split('.').TakeLast(2);
                reference.ReplaceWith(new XText(string.Join('.', propertyName)));

                continue;
            }

            var keyword = reference.Attribute("langword")?.Value;
            if (keyword is not null)
            {
                reference.ReplaceWith(new XText(keyword));

                continue;
            }
        }
    }

    private static async Task<XDocument?> LoadDocumentationAsync(Assembly assembly)
    {
        var targetDirectory = Path.GetDirectoryName(Host?.Location);
        if (targetDirectory is null)
        {
            return null;
        }

        var fileName = assembly.GetName().Name + ".xml";
        var path = Path.Combine(targetDirectory, fileName);

        if (!File.Exists(path))
        {
            return null;
        }

        await using var readStream = File.OpenRead(path);
        return await XDocument.LoadAsync(readStream, LoadOptions.None, CancellationToken.None);
    }
}
