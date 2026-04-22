using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace NavData.Swagger;

internal sealed class TagOrderProcessor(params string[] pinnedTags) : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        var tags = context.Document.Operations
            .SelectMany(o => o.Operation.Tags ?? [])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var ordered = new List<OpenApiTag>();
        foreach (var pinned in pinnedTags)
        {
            var match = tags.FirstOrDefault(t => t.Equals(pinned, StringComparison.OrdinalIgnoreCase));
            if (match is not null) ordered.Add(new OpenApiTag { Name = match });
        }

        var pinnedSet = ordered.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        ordered.AddRange(tags
            .Where(t => !pinnedSet.Contains(t))
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Select(name => new OpenApiTag { Name = name }));

        context.Document.Tags = ordered;
    }
}
