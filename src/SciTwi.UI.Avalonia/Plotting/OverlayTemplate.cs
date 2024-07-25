using System;
using Avalonia.Collections;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;


namespace SciTwi.UI.Controls.Plotting;

public interface IOverlayTemplate
{
    public bool Match(object data);
    public OverlayBase? Build(object data);
    public OverlayBase? Build(object data, OverlayBase? existing);
}

public class OverlayTemplate : IOverlayTemplate
{
    [Content]
    [TemplateContent(TemplateResultType = typeof(OverlayBase))]
    public object? Content { get; set; }

    [DataType]
    public Type? DataType { get; set; }

    public bool Match(object data)
    {
        if (this.DataType is Type targetType)
            return targetType.IsInstanceOfType(data);
        return true;
    }

    public OverlayBase? Build(object data) => this.Build(data, null);

    public OverlayBase? Build(object data, OverlayBase? existing)
    {
        return existing ?? TemplateContent.Load<OverlayBase?>(this.Content)?.Result;
    }
}

public class OverlayTemplates : AvaloniaList<IOverlayTemplate>, IOverlayTemplate
{
    public bool Match(object data)
    {
        foreach (var template in this)
            if (template?.Match(data) == true)
                return true;
        return false;
    }

    public OverlayBase? Build(object data)
    {
        foreach (var template in this)
            if (template?.Match(data) == true)
                return template.Build(data);
        return null;
    }

    public OverlayBase? Build(object data, OverlayBase? existing) =>
        existing ?? this.Build(data);
}
