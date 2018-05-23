using System;
using System.Diagnostics;
using Markdig.Extensions.AutoIdentifiers;
using Omnibus.Core.Config;
using Omnibus.Core.Github;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.GitHub;
using Wyam.Html;
using Wyam.Markdown;
using Wyam.Yaml;

namespace Omnibus.Core
{
    public class DocumentProcessor
    {
        private readonly RootConfig _config;

        public DocumentProcessor(RootConfig config)
        {
            _config = config;
        }

        public void Process()
        {
            Engine eng = new Engine();

            eng.Pipelines.Add("MarkdownInput",
                //new ReadStrings("# hey there"),
                new ReadFiles("**/*.md"),

                //We can pass repos into here, otherwise they'll be loaded from the config
                new ReadFromGithub(_config.Github),

                new If((doc, _) => doc.String(Keys.RelativeFilePath)?.EndsWith(".md") ?? false,
                    new FrontMatter(new Yaml()),
                    new Markdown()
                        .UseExtension<RelativeUrlMarkdownExtension>()
                        .UseExtension(new AutoIdentifierExtension(AutoIdentifierOptions.Default))),

                new WriteFiles(".html")
                    .IgnoreEmptyContent(false));

            eng.Execute();
        }
    }
}
