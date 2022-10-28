using Nuke.Common.IO;

partial class Build
{
    const string Debug = "Debug";
    const string Release = "Release";
    const string Net60 = "net6.0";

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath AllSolutionFile => RootDirectory / "All.sln";
    AbsolutePath PublicApiSolutionFile => SourceDirectory / "Build.CheckApi.sln";
    AbsolutePath SonarSolutionFile => SourceDirectory / "Build.Sonar.sln";
    AbsolutePath TestSolutionFile => TemporaryDirectory / "Build.Test.sln";
    AbsolutePath PackSolutionFile => SourceDirectory / "Build.Pack.sln";

    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath TestResultDirectory => OutputDirectory / "test-results";
    AbsolutePath CoverageReportDirectory => OutputDirectory / "coberage-reports";
    AbsolutePath PackageDirectory => OutputDirectory / "packages";
}
