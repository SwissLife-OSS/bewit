using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Codecov;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Nuke.Common.ProjectModel.ProjectModelTasks;
using static Helpers;

partial class Build
{
    IEnumerable<Project> TestProjects => ProjectModelTasks
        .ParseSolution(AllSolutionFile).GetProjects("*.Tests");

    Target Test => _ => _
        .Produces(TestResultDirectory / "*.trx")
        .Executes(() =>
        {
            DotNetConstructSolution(AllSolutionFile);

            DotNetBuild(c => c
                .SetProjectFile(AllSolutionFile)
                .SetConfiguration(Debug));

            DotNetTest(
                c => c
                    .SetProjectFile(AllSolutionFile)
                    .SetConfiguration(Debug)
                    .SetNoRestore(true)
                    .SetNoBuild(true)
                    .ResetVerbosity()
                    .SetResultsDirectory(TestResultDirectory)
                    .CombineWith(TestProjects, (_, v) => _
                        .SetProjectFile(v)
                        .SetLoggers($"trx;LogFileName={v.Name}.trx")));
        });
}
