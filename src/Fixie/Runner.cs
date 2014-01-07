﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fixie.Conventions;

namespace Fixie
{
    public class Runner
    {
        readonly Listener listener;
        readonly ILookup<string, string> options;

        public Runner(Listener listener)
            : this(listener, Enumerable.Empty<string>().ToLookup(x => x, x => x)) { }

        public Runner(Listener listener, ILookup<string, string> options)
        {
            this.listener = listener;
            this.options = options;
        }

        public AssemblyResult RunAssembly(Assembly assembly)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes());
        }

        public AssemblyResult RunNamespace(Assembly assembly, string ns)
        {
            var runContext = new RunContext(assembly, options);

            return RunTypes(runContext, assembly.GetTypes().Where(type => type.IsInNamespace(ns)).ToArray());
        }

        public AssemblyResult RunType(Assembly assembly, Type type)
        {
            if (type.IsSubclassOf(typeof(Convention)))
            {
                var singleConventionRunContext = new RunContext(assembly, options);
                var singleConvention = ConstructConvention(type, singleConventionRunContext);

                return RunTypes(singleConventionRunContext, singleConvention, assembly.GetTypes());
            }

            var runContext = new RunContext(assembly, options, type);

            return RunTypes(runContext, type);
        }

        public AssemblyResult RunMethod(Assembly assembly, MethodInfo method)
        {
            var runContext = new RunContext(assembly, options, method);

            var conventions = GetConventions(runContext);

            foreach (var convention in conventions)
                convention.Methods.Where(m => m == method);

            var type = method.DeclaringType;

            return Run(runContext, conventions, type);
        }

        public IEnumerable<Case> GetTestCases(Assembly assembly)
        {
            var runContext = new RunContext(assembly, options);
            var conventions = GetConventions(runContext);
            var testCases = new List<Case>();
            foreach (var convention in conventions)
                testCases.AddRange(convention.GetTestCases(assembly.GetTypes()));

            return testCases;
        }

        private AssemblyResult RunTypes(RunContext runContext, params Type[] types)
        {
            return Run(runContext, GetConventions(runContext), types);
        }

        private AssemblyResult RunTypes(RunContext runContext, Convention convention, params Type[] types)
        {
            return Run(runContext, new[] { convention }, types);
        }

        static Convention[] GetConventions(RunContext runContext)
        {
            var customConventions = runContext.Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Convention)))
                .Select(t => ConstructConvention(t, runContext))
                .ToArray();

            if (customConventions.Any())
                return customConventions;

            return new[] { (Convention) new DefaultConvention() };
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

        AssemblyResult Run(RunContext runContext, IEnumerable<Convention> conventions, params Type[] candidateTypes)
        {
            var resultListener = new AssemblyResultListener(listener);

            resultListener.AssemblyStarted(runContext.Assembly);

            foreach (var convention in conventions)
                convention.Execute(resultListener, candidateTypes);

            var result = resultListener.AssemblyResult;

            resultListener.AssemblyCompleted(runContext.Assembly, result);

            return result;
        }

        class AssemblyResultListener : Listener
        {
            int passed;
            int failed;
            int skipped;
            readonly Listener inner;

            public AssemblyResultListener(Listener inner)
            {
                this.inner = inner;
            }

            public void AssemblyStarted(Assembly assembly)
            {
                passed = 0;
                failed = 0;
                skipped = 0;
                inner.AssemblyStarted(assembly);
            }

            public void CaseSkipped(Case @case)
            {
                skipped++;
                inner.CaseSkipped(@case);
            }

            public void CasePassed(PassResult result)
            {
                passed++;
                inner.CasePassed(result);
            }

            public void CaseFailed(FailResult result)
            {
                failed++;
                inner.CaseFailed(result);
            }

            public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
            {
                inner.AssemblyCompleted(assembly, result);
            }

            public AssemblyResult AssemblyResult
            {
                get { return new AssemblyResult(passed, failed, skipped); }
            }
        }
    }
}