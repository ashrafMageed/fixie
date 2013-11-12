using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Fixie.VisualStudio
{
    public class VisualStudioTestListener : Listener
    {
        readonly IFrameworkHandle frameworkHandle;
        readonly string source;

        public VisualStudioTestListener(IFrameworkHandle frameworkHandle, string source)
        {
            this.frameworkHandle = frameworkHandle;
            this.source = source;
        }

        public void AssemblyStarted(System.Reflection.Assembly assembly)
        {
            //
        }

        public void CasePassed(PassResult result)
        {
            var testCase = new TestCase(result.Case.Name, new Uri(Constants.EXECUTOR_URI_STRING), source);
            frameworkHandle.RecordStart(testCase);
            var testResult = new TestResult(testCase) { Outcome = TestOutcome.Passed };
            frameworkHandle.RecordEnd(testCase, TestOutcome.Passed);
            frameworkHandle.RecordResult(testResult);
        }

        public void CaseFailed(FailResult result)
        {
            var testCase = new TestCase(result.Case.Name, new Uri(Constants.EXECUTOR_URI_STRING), source);
            frameworkHandle.RecordStart(testCase);
            var testResult = new TestResult(testCase) { Outcome = TestOutcome.Failed, ErrorMessage = result.Case.Exceptions.Last().Message, ErrorStackTrace = result.Case.Exceptions.Last().StackTrace};
            frameworkHandle.RecordEnd(testCase, TestOutcome.Failed);
            frameworkHandle.RecordResult(testResult);
        }

        public void AssemblyCompleted(System.Reflection.Assembly assembly, AssemblyResult result)
        {
            
        }
    }
}
