using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fixie.Listeners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio
{
    [ExtensionUri(Constants.EXECUTOR_URI_STRING)]
    public class TestExecutor : ITestExecutor
    {
        // use this to cancel execution by breaking out of the foreach
        bool _cancelled;

        public void Cancel()
        {
            _cancelled = true;
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Console.WriteLine("Executing Sources....£$£$$£$**************************************");
            sources.ToList().ForEach(source =>
            {
                var listener = new VisualStudioTestListener(frameworkHandle, @source);
                var runner = new Runner(listener);
                runner.RunAssembly(Assembly.Load(AssemblyName.GetAssemblyName(@source)));
            });
            
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Console.WriteLine("Executing test cases*************************************************");
            var sources = tests.Select(test => test.Source).Distinct();
            RunTests(sources, runContext, frameworkHandle);
//            var listener = new VisualStudioTestListener(frameworkHandle, @tests.First().Source);
//            var runner = new Runner(listener);
//            tests.ToList().ForEach(test => runner.RunMethod(Assembly.LoadFrom(@test.Source), GetType().GetMethod(test.FullyQualifiedName)));
        }
    }
}
