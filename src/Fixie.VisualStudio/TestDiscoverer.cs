using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Fixie.VisualStudio
{
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(Constants.EXECUTOR_URI_STRING)]
    public class TestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            sources.ToList().ForEach(source =>
            {
                logger.SendMessage(TestMessageLevel.Error, source);
                var assembly = Assembly.Load(AssemblyName.GetAssemblyName(source));
                var runner = new Runner(new VisualStudioTestListener(null, source));
                var testCases = runner.GetTestCases(assembly);
                foreach (var testCase in testCases)
                {
                    logger.SendMessage(TestMessageLevel.Error, testCase.Name);
                    discoverySink.SendTestCase(new TestCase(testCase.Name, new Uri(Constants.EXECUTOR_URI_STRING), source)
                    {
                        DisplayName = GetDislpayName(testCase.Name)
                    });
                }

            });
        }

        static string GetDislpayName(string fullyQualifiedName)
        {
            var lastPeriodInTestName = fullyQualifiedName.LastIndexOf('.');
            return fullyQualifiedName.Substring(lastPeriodInTestName + 1).Replace(".", "");
        }
    }
}
