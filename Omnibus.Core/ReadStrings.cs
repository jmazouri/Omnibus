using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Omnibus.Core
{
    public class ReadStrings : IModule
    {
        private readonly string[] _input;

        public ReadStrings(params string[] strings) => _input = strings;

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var dict = new Dictionary<string, object>
            {
                { Keys.RelativeFilePath, "test.md" }
            };

            return _input.Select(d => context.GetDocument(context.GetContentStream(d), dict));
        }
    }
}
