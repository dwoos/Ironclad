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
    class BatchGetCoreMariposaVerb 
        : Verb
    {
        private const string BATCH_EXTN = ".mariposabatch";
        private const int version = 1;

        private AbstractId abstractId;
        private List<Verb> upstream;
        private List<BuildObject> deps;
        private BuildObject output;

        public BatchGetCoreMariposaVerb(SourcePath batch_file)
        {
            this.upstream = new List<Verb>();
            foreach (string line in File.ReadAllLines(IronRootDirectory.PathTo(batch_file)))
            {
                if (line.Equals("") || line[0] == '#')
                {
                    continue;
                }
                char[] splitOn = {' '};
                string[] words = line.Split(splitOn);

                SourcePath src = new SourcePath(words[0]);
                this.upstream.Add(new MariposaGetCoreVerb(src));
            }
            this.output = batch_file.makeVirtualObject(".mariposabatch-done");
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
            BuildEngine.theEngine.Repository.StoreVirtual(this.output, new Fresh(), new VirtualContents());
            return new VerbSyncWorker(workingDirectory, new Fresh());
        }

        public override IEnumerable<BuildObject> getOutputs()
        {
            return new BuildObject[] { this.output };
        }
    }
}
