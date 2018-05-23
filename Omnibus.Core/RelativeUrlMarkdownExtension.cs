using System;
using System.Collections.Generic;
using System.Text;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Omnibus.Core
{
    public class RelativeUrlMarkdownExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
            {
                var inlineRenderer = htmlRenderer.ObjectRenderers.FindExact<LinkInlineRenderer>();

                if (inlineRenderer != null)
                {
                    inlineRenderer.TryWriters.Remove(TryLinkInlineRenderer);
                    inlineRenderer.TryWriters.Add(TryLinkInlineRenderer);
                }
            }
        }

        private bool TryLinkInlineRenderer(HtmlRenderer renderer, LinkInline linkInline)
        {
            if (linkInline.Url == null ||
                linkInline.Url.StartsWith("http") ||
                linkInline.Url.StartsWith("#") ||
                linkInline.Url.EndsWith(".html"))
            {
                return false;
            }

            linkInline.Url += ".html";

            renderer.Write(linkInline);

            return true;
        }
    }
}
