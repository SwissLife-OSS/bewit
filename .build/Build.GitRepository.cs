using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;

partial class Build
{
    [GitRepository] readonly GitRepository GitRepository;
}
