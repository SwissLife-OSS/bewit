using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

static class Helpers
{
    public static IEnumerable<string> GetAllProjects(string sourceDirectory)
    {
        string testDirectory = Path.GetFullPath(Path.Combine(sourceDirectory, "test"));
        string srcDirectory = Path.GetFullPath(Path.Combine(sourceDirectory, "src"));
        string toolsDirectory = Path.GetFullPath(Path.Combine(sourceDirectory, "tools"));
        string sampleDirectory = Path.GetFullPath(Path.Combine(sourceDirectory, "samples"));

        IEnumerable<string> sourceProjects =
            Directory.EnumerateFiles(srcDirectory, "*.csproj", SearchOption.AllDirectories);

        IEnumerable<string> testProjects = Directory.Exists(testDirectory)
            ? Directory.EnumerateFiles(testDirectory, "*.csproj", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

        IEnumerable<string> toolsProjects = Directory.Exists(toolsDirectory)
            ? Directory.EnumerateFiles(toolsDirectory, "*.csproj", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

        IEnumerable<string> sampleProjects = Directory.Exists(sampleDirectory)
            ? Directory.EnumerateFiles(sampleDirectory, "*.csproj", SearchOption.AllDirectories)
            : Enumerable.Empty<string>();

        return sourceProjects.Concat(testProjects).Concat(toolsProjects).Concat(sampleProjects);
    }

    public static IReadOnlyCollection<Output> DotNetConstructSolution(
        string solutionFile,
        IEnumerable<string> projects = null)
    {
        if (File.Exists(solutionFile))
        {
            Log.Information($"Removing current solution file. {solutionFile}");
            File.Delete(solutionFile);
        }

        projects ??= GetAllProjects(Path.GetDirectoryName(solutionFile));
        var workingDirectory = Path.GetDirectoryName(solutionFile);
        var list = new List<Output>();

        list.AddRange(DotNetTasks.DotNet($"new sln -n {Path.GetFileNameWithoutExtension(solutionFile)}", workingDirectory));
        Console.WriteLine(projects.Select(t => $"\"{t}\"").ToArray());
        var projectsArg = string.Join(" ", projects.Select(t => $"\"{t}\""));

        list.AddRange(DotNetTasks.DotNet($"sln \"{solutionFile}\" add {projectsArg}", workingDirectory));

        return list;
    }

    public static IReadOnlyCollection<Output> DotNetBuildTestSolution(
        string solutionFile,
        IEnumerable<Project> projects)
    {
        if (File.Exists(solutionFile))
        {
            return Array.Empty<Output>();
        }

        var workingDirectory = Path.GetDirectoryName(solutionFile);
        var list = new List<Output>();

        list.AddRange(DotNetTasks.DotNet($"new sln -n {Path.GetFileNameWithoutExtension(solutionFile)}", workingDirectory));

        var projectsArg = string.Join(" ", projects.Select(t => $"\"{t}\""));

        list.AddRange(DotNetTasks.DotNet($"sln \"{solutionFile}\" add {projectsArg}", workingDirectory));

        return list;
    }

    public static void TryDelete(string fileName)
    {
        if(File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }
}
