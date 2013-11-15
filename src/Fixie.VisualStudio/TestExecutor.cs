using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio
{
    [ExtensionUri(Constants.EXECUTOR_URI_STRING)]
    public class TestExecutor : ITestExecutor
    {
        private bool _cancelled;

        public void Cancel()
        {
            _cancelled = true;
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if(runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;
            
            foreach(var source in sources)
            {
                if (_cancelled)
                    break;

                var listener = new VisualStudioTestListener(frameworkHandle, @source);
                var runner = new Runner(listener);
                runner.RunAssembly(Assembly.Load(AssemblyName.GetAssemblyName(@source)));
            }
            
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if(runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;

            var testCaseGroupedBySource = tests.ToList().GroupBy(test => test.Source);

            foreach (var groupedTestCases in testCaseGroupedBySource)
            {
                var listener = new VisualStudioTestListener(frameworkHandle, groupedTestCases.Key);
                var runner = new Runner(listener);
                foreach (var test in groupedTestCases)
                {
                    if (_cancelled)
                        break;

                    var assembly = Assembly.Load(AssemblyName.GetAssemblyName(@test.Source));
                    var methodInfo = GetMethodInfoFromMethodName(assembly, test.FullyQualifiedName);

                    runner.RunMethod(assembly, methodInfo);
                }
            }
        }

        static MethodInfo GetMethodInfoFromMethodName(Assembly assembly, string fullyQualifiedName)
        {
            var lastPeriodInTestName = fullyQualifiedName.LastIndexOf('.');
            var methodName = fullyQualifiedName.Substring(lastPeriodInTestName + 1).Replace(".", "");
            var typeName = fullyQualifiedName.Substring(0, lastPeriodInTestName);
            return assembly.GetType(typeName).GetMethod(methodName);
        }
    }
}
