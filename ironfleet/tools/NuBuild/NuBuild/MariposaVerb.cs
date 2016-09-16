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

    internal class MariposaVerb
        : Verb, IProcessInvokeAsyncVerb
    {
        public const string Mariposa_EXTN = ".smt2";

        private static SourcePath MariposaExecutable;

        private BuildObject MariposaInput;
        private AbstractId abstractId;
        private IEnumerable<IVerb> upstreamVerbs;

        private int version = 1;

        private BuildObject outputFile;
        private string modeid;
        private IEnumerable<string> modeargs;

        public MariposaVerb(BuildObject MariposaInput, string modeid, IEnumerable<string> args)
        {
            this.MariposaInput = MariposaInput;
            this.MariposaInput.shouldCompress = true;
            this.modeid = modeid;
            this.modeargs = args;
            upstreamVerbs = new List<IVerb>();
            this.abstractId = new AbstractId(
                this.GetType().Name,
                version,
                MariposaInput.ToString() + "-" + modeid);
            this.outputFile = MariposaInput.makeOutputObjectWithFilename(modeid + Mariposa_EXTN);
        }

        public override IEnumerable<BuildObject> getDependencies(out DependencyDisposition ddisp)
        {
            ddisp = DependencyDisposition.Complete;
            List<BuildObject> result = new List<BuildObject>() { MariposaInput };
            result.Add(getMariposaExecutable());
            result.AddRange(getMariposaDLLs());
            return result;
        }

        public override IEnumerable<IVerb> getVerbs()
        {
            return upstreamVerbs;
        }

        public override IEnumerable<BuildObject> getOutputs()
        {
            return new BuildObject[] { outputFile };
        }

        public override AbstractId getAbstractIdentifier()
        {
            return abstractId;
        }

        public override IVerbWorker getWorker(WorkingDirectory workingDirectory)
        {
            List<string> args = new List<string>();
            args.Add(MariposaInput.getRelativePath());
            args.Add(outputFile.getRelativePath());
            args.AddRange(modeargs);


            return new ProcessInvokeAsyncWorker(
                workingDirectory,
                this,
                getMariposaExecutable().getRelativePath(),
                args.ToArray(),
                ProcessExitCodeHandling.NonzeroIsFailure,
                getDiagnosticsBase(),
                allowCloudExecution: true,
                returnStandardOut: true,
                returnStandardError: true);
        }

        public Disposition Complete(WorkingDirectory workingDirectory, double cpuTimeSeconds, string stdout, string stderr, Disposition disposition)
        {
            if (disposition is Failed)
            {
                string outputPath = workingDirectory.PathTo(this.outputFile);
                File.WriteAllText(outputPath, stdout);
            }
            return disposition;
        }


        private static SourcePath getMariposaExecutable()
        {
            // TODO this should eventually be a BuildObject from *building* the executable.
            if (MariposaVerb.MariposaExecutable == null)
            {
                MariposaVerb.MariposaExecutable = new SourcePath("tools\\Mariposa\\Mariposa.exe", SourcePath.SourceType.Tools);
            }

            return MariposaVerb.MariposaExecutable;
        }

        private static IEnumerable<SourcePath> getMariposaDLLs()
        {
            var dlls = new List<SourcePath>();
            dlls.Add(new SourcePath("tools\\Mariposa\\Mariposa.exe.config", SourcePath.SourceType.Tools));
            dlls.Add(new SourcePath("tools\\Mariposa\\FSharp.Core.dll", SourcePath.SourceType.Tools));
            dlls.Add(new SourcePath("tools\\Mariposa\\FsLexYacc.Runtime.dll", SourcePath.SourceType.Tools));
            dlls.Add(new SourcePath("tools\\Mariposa\\SMTLib.dll", SourcePath.SourceType.Tools));
            return dlls;
        }
    }

}
