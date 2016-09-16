//--
// <copyright file="Z3Verb.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//--

namespace NuBuild
{
using System;
using System.Collections.Generic;
using System.IO;
    using System.Linq;

    internal class Z3Verb
        : Verb, IProcessInvokeAsyncVerb
    {
        public const string Z3_EXTN = ".smt2";

        private static SourcePath z3Executable;

        private BuildObject z3Input;
        private AbstractId abstractId;
        private IEnumerable<IVerb> upstreamVerbs;
        private int timeLimit = 300;

        private int version = 1;

        private BuildObject outputFile;
        private BuildObject timeFile;

        public Z3Verb(BuildObject z3Input)
        {
            this.z3Input = z3Input;
            this.z3Input.shouldCompress = true;
            upstreamVerbs = new List<IVerb>();
            this.abstractId = new AbstractId(
                this.GetType().Name,
                version,
                z3Input.ToString());
            this.outputFile = z3Input.makeOutputObject(".z3-out");
            this.timeFile = z3Input.makeOutputObject(".z3-time");
        }

        public override IEnumerable<BuildObject> getDependencies(out DependencyDisposition ddisp)
        {
            ddisp = DependencyDisposition.Complete;
            List<BuildObject> result = new List<BuildObject>() { z3Input };
            result.Add(getZ3Executable());
            return result;
        }

        public override IEnumerable<IVerb> getVerbs()
        {
            return upstreamVerbs;
        }

        public override IEnumerable<BuildObject> getOutputs()
        {
            return new BuildObject[] { outputFile, timeFile };
        }

        public override AbstractId getAbstractIdentifier()
        {
            return abstractId;
        }

        public override IVerbWorker getWorker(WorkingDirectory workingDirectory)
        {
            List<string> args = new List<string>();
            args.Add("-T:" + timeLimit);
            args.Add(z3Input.getRelativePath());

            return new ProcessInvokeAsyncWorker(
                workingDirectory,
                this,
                getZ3Executable().getRelativePath(),
                args.ToArray(),
                ProcessExitCodeHandling.NonzeroIsOkay,
                getDiagnosticsBase(),
                allowCloudExecution: true,
                returnStandardOut: true,
                returnStandardError: true);
        }

        public Disposition Complete(WorkingDirectory workingDirectory, double cpuTimeSeconds, string stdout, string stderr, Disposition disposition)
        {
            File.WriteAllText(workingDirectory.PathTo(outputFile), stdout);
            File.WriteAllText(workingDirectory.PathTo(timeFile), cpuTimeSeconds.ToString());
            return disposition;
        }


        private static SourcePath getZ3Executable()
        {
            // TODO this should eventually be a BuildObject from *building* the executable.
            if (Z3Verb.z3Executable == null)
            {
                Z3Verb.z3Executable = new SourcePath("tools\\Z3\\Z3.exe", SourcePath.SourceType.Tools);
            }

            return Z3Verb.z3Executable;
        }
    }

}
