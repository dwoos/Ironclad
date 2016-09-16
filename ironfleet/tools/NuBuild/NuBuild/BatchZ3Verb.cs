//--
// <copyright file="BatchVerifyVerb.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//--

namespace NuBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    class BatchZ3Verb 
        : Verb
    {
        private const string BATCH_EXTN = ".z3batch";
        private const int version = 1;

        private AbstractId abstractId;
        private List<Verb> upstream;
        private List<BuildObject> deps;
        private BuildObject output;

        public BatchZ3Verb(SourcePath batch_file)
        {
            this.upstream = new List<Verb>();
            foreach (string line in File.ReadAllLines(IronRootDirectory.PathTo(batch_file)))
            {
                if (line.Equals("") || line[0] == '#')
                {
                    continue;
                }

                SourcePath src = new SourcePath(line);

                this.upstream.Add(new Z3Verb(src));
            }
            this.output = batch_file.makeOutputObject(".z3batch-done");
            this.abstractId = new AbstractId(this.GetType().Name, version, batch_file.ToString());
        }

        public override AbstractId getAbstractIdentifier()
        {
            return this.abstractId;
        }

        public override IEnumerable<BuildObject> getDependencies(out DependencyDisposition ddisp)
        {
            if (this.deps == null)
            {
                this.deps = new List<BuildObject>();
                foreach (Verb verb in this.upstream)
                {
                    this.deps.AddRange(verb.getOutputs());
                }
            }

            ddisp = DependencyDisposition.Complete;
            return this.deps;
        }
        public override IEnumerable<IVerb> getVerbs()
        {
            return this.upstream;
        }

        public override IVerbWorker getWorker(WorkingDirectory workingDirectory)
        {
            string outputPath = workingDirectory.PathTo(output);
            File.WriteAllText(outputPath, "");
            return new VerbSyncWorker(workingDirectory, new Fresh());
        }

        public override IEnumerable<BuildObject> getOutputs()
        {
            return new BuildObject[] { this.output };
        }
    }
}
