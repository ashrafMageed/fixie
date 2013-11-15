using System;
using System.Collections.Generic;
using System.IO;
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
            var testResult = new TestResult(testCase)
            {
                Outcome = TestOutcome.Failed,
                ErrorMessage = result.Exceptions.First().GetType().FullName,
                ErrorStackTrace = CompoundStackTrace(result.Exceptions)
            };
            frameworkHandle.RecordEnd(testCase, TestOutcome.Failed);
            frameworkHandle.RecordResult(testResult);
        }

        static string CompoundStackTrace(IEnumerable<Exception> exceptions)
        {
            using (var writer = new StringWriter())
            {
                writer.WriteCompoundStackTrace(exceptions);
                return writer.ToString();
            }
        }

        public void AssemblyCompleted(System.Reflection.Assembly assembly, AssemblyResult result)
        {
            
        }
    }
}
