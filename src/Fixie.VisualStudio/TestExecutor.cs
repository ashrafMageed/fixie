using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fixie.Listeners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

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
            sources.ToList().ForEach(source =>
            {
                var listener = new VisualStudioTestListener(frameworkHandle, @source);
                var runner = new Runner(listener);
                runner.RunAssembly(Assembly.Load(AssemblyName.GetAssemblyName(@source)));
            });
            
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            try
            {
                var listener = new VisualStudioTestListener(frameworkHandle, tests.First().Source);
                var runner = new Runner(listener);
                tests.ToList().ForEach(test =>
                {
                    var assembly = Assembly.Load(AssemblyName.GetAssemblyName(@test.Source));
                    var methodInfo = GetMethodInfoFromMethodName(assembly, test.FullyQualifiedName);
                    runner.RunMethod(assembly, methodInfo);
                });
            }
            catch (Exception ex)
            {

                frameworkHandle.SendMessage(TestMessageLevel.Error, ex.ToString());
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
