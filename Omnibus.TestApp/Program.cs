using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Omnibus.Core;
using Omnibus.Core.Config;

namespace Omnibus.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Wyam.Common.Tracing.Trace.Level = SourceLevels.Verbose;
            Wyam.Common.Tracing.Trace.AddListener(new TextWriterTraceListener(Console.Out));

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("config.json");

            var config = new RootConfig();
            configBuilder.Build().Bind(config);

            var processor = new DocumentProcessor(config);
            processor.Process();
            
            Console.ReadLine();

            //
        }
    }
}
