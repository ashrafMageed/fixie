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
                var conventions = GetConventions(new RunContext(assembly, Enumerable.Empty<string>().ToLookup(x => x, x => x)));

                conventions.ToList().ForEach(convention =>
                {
                    foreach (var testClass in convention.Classes.Filter(assembly.GetTypes()))
                    {
                        var methods = convention.Methods.Filter(testClass);

                        var cases = methods.Select(m => new Case(testClass, m)).ToList();
                        cases.ForEach(testCase =>
                        {
                            logger.SendMessage(TestMessageLevel.Error, testCase.Method.DeclaringType.ToString());
                            discoverySink.SendTestCase(new TestCase(testCase.Name, new Uri(Constants.EXECUTOR_URI_STRING), source)
                            {
                                DisplayName = GetDislpayName(testCase.Name),
                                CodeFilePath = testCase.Method.DeclaringType.ToString(),
                                LocalExtensionData = testCase.Method.DeclaringType.ToString()
                            });
                        });
                    }
                });

            });
        }

        
        static string GetDislpayName(string fullyQualifiedName)
        {
            var lastPeriodInTestName = fullyQualifiedName.LastIndexOf('.');
            return fullyQualifiedName.Substring(lastPeriodInTestName + 1).Replace(".", "");
        }

        // Don't repeat this.... 
        // Maybe refactor the code to expose these as a separate class (ConventionDiscovery)
        // which can be reused here??
        static Convention[] GetConventions(RunContext runContext)
        {
            var customConventions = runContext.Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)))
                .Select(t => ConstructConvention(t, runContext))
                .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention)new DefaultConvention() };
        }

        static Convention ConstructConvention(Type conventionType, RunContext runContext)
        {
            var constructors = conventionType.GetConstructors();

            if (constructors.Length == 1)
            {
                var parameters = constructors.Single().GetParameters();

                if (parameters.Length == 1 && parameters.Single().ParameterType == typeof(RunContext))
                    return (Convention)Activator.CreateInstance(conventionType, runContext);
            }

            return (Convention)Activator.CreateInstance(conventionType);
        }
    }
}
