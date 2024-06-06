using System;
using Avalonia.Collections;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting
{
    public interface IOverlayTemplate
    {
        public abstract bool Match(object data);
        public abstract OverlayBase Build(object data);
    }

    public class OverlayTemplate : IOverlayTemplate
    {
        [Content]
        [TemplateContent(TemplateResultType = typeof(OverlayBase))]
        public object Content { get; set; }

        public Type DataType { get; set; }

        public bool Match(object data)
        {
            if (this.DataType is Type targetType)
                return targetType.IsInstanceOfType(data);
            return true;
        }

        public OverlayBase Build(object data)
        {
            switch (this.Content)
            {
                case Func<IServiceProvider, object> direct:
                    var instatiated = direct(null);
                    return (instatiated as TemplateResult<OverlayBase>)?.Result;

                default:
                    return null;
            }
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

        public OverlayBase Build(object data)
        {
            foreach (var template in this)
                if (template?.Match(data) == true)
                    return template.Build(data);
            return null;
        }
    }
}
