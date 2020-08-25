#load "build/helpers.cake"
#load "build/version.cake"
#tool "nuget:https://api.nuget.org/v3/index.json?package=nuget.commandline&version=5.3.1"

#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0
#tool dotnet:?package=gpr&version=0.1.233


///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projects = GetProjects(File("./src/Spectre.Logging.sln"), configuration);
var artifacts = "./dist/";
var frameworks = new List<string> { "netstandard2.0" };
var packageVersion = string.Empty;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
	CreateDirectory(artifacts);
	packageVersion = BuildVersion(fallbackVersion);
	if (FileExists("./build/.dotnet/dotnet.exe")) {
		Information("Using local install of `dotnet` SDK!");
		Context.Tools.RegisterFile("./build/.dotnet/dotnet.exe");
	}
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projects.AllProjectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	DotNetCoreRestore(projects.SolutionPath);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	foreach (var project in projects.SourceProjectPaths) {
		Information($"Building {project.GetDirectoryName()} for {configuration}");
		var settings = new DotNetCoreBuildSettings {
			Configuration = configuration,
			ArgumentCustomization = args => args.Append("/p:NoWarn=NU1701"),
		};
		DotNetCoreBuild(project.FullPath, settings);
	}
	
});

Task("NuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Building NuGet package");
    CreateDirectory(artifacts + "package/");
    var packSettings = new DotNetCorePackSettings {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = $"{artifacts}package",
        ArgumentCustomization = args => args
            .Append($"/p:Version=\"{packageVersion}\"")
            .Append("/p:NoWarn=\"NU1701 NU1602\"")
    };
    foreach(var project in projects.SourceProjectPaths) {
        Information($"Packing {project.GetDirectoryName()}...");
        DotNetCorePack(project.FullPath, packSettings);
    }
});

Task("Post-Build")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "build");
	foreach (var project in projects.SourceProjects) {
		CreateDirectory(artifacts + "build/" + project.Name);
		foreach (var framework in frameworks) {
			var frameworkDir = $"{artifacts}build/{project.Name}/{framework}";
			CreateDirectory(frameworkDir);
			var files = GetFiles($"{project.Path.GetDirectory()}/bin/{configuration}/{framework}/*.*");
			CopyFiles(files, frameworkDir);
		}
	}
});

Task("Publish-NuGet-Package")
.IsDependentOn("NuGet")
.WithCriteria(() => HasEnvironmentVariable("NUGET_TOKEN"))
.WithCriteria(() => HasEnvironmentVariable("GITHUB_REF"))
//this criteria is kind of counter-intuitive: we ignore master because we're also going to be building tagged builds and they're the only stable builds we should be pushing
.WithCriteria(() => EnvironmentVariable("GITHUB_REF").StartsWith("refs/tags/v") || EnvironmentVariable("GITHUB_REF") == "refs/heads/develop")
.Does(() => {
    var nugetToken = EnvironmentVariable("NUGET_TOKEN");
    var pkgFiles = GetFiles($"{artifacts}package/*.nupkg");
	Information($"Pushing {pkgFiles.Count} package files!");
    NuGetPush(pkgFiles, new NuGetPushSettings {
      Source = "https://api.nuget.org/v3/index.json",
      ApiKey = nugetToken
    });
});

Task("Publish-GitHub-Package")
.IsDependentOn("NuGet")
.WithCriteria(() => HasEnvironmentVariable("GITHUB_REF"))
.WithCriteria(() => EnvironmentVariable("GITHUB_REF").StartsWith("refs/tags/v"))
.Does(() => {
    // Publish to GitHub Packages
    var exitCode = 0;
    var pkgFiles = GetFiles($"{artifacts}package/*.nupkg");
    foreach(var file in pkgFiles) 
    {
        Information("Publishing {0}...", file.GetFilename().FullPath);
        exitCode += StartProcess("dotnet", 
            new ProcessSettings {
                Arguments = new ProcessArgumentBuilder()
                    .Append("gpr")
                    .Append("push")
                    .AppendQuoted(file.FullPath)
                    .AppendSwitchSecret("-k", " ", EnvironmentVariable("GITHUB_TOKEN"))
            }
        );
    }
    if(exitCode != 0) 
    {
        throw new CakeException("Could not push GitHub packages.");
    }
});

Task("Release")
.IsDependentOn("Default")
.IsDependentOn("Publish-GitHub-Package")
.IsDependentOn("Publish-NuGet-Package");

Task("Default")
.IsDependentOn("Post-Build")
.IsDependentOn("NuGet");

RunTarget(target);