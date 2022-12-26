
/* ----------------------------------------------------------------------------
 * Build script for Perdeval vNext
 * ----------------------------------------------------------------------------
 * This script uses environment variables. To run correctly, this script needs
 * these variables to be set: 
 *  - CAKE_PUBLIC_GITHUB_TOKEN    - Github token of the repository
 *  - CAKE_PUBLIC_GITHUB_USERNAME - Github username with R/W for LogReader repository
 */
///////////////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
///////////////////////////////////////////////////////////////////////////////

#tool nuget:?package=vswhere&version=3.0.1
#tool nuget:?package=GitVersion.CommandLine&version=5.10.1
// #tool xunit.runner.console
#tool nuget:?package=GitReleaseManager&version=0.13.0

#addin nuget:?package=Cake.Figlet&version=2.0.1

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////
var solution = "./Lanceur.sln";

// https://gitversion.net/docs/usage/cli/arguments
// https://cakebuild.net/api/Cake.Core.Tooling/ToolSettings/50AAB3A8
GitVersion gitVersion = GitVersion(new GitVersionSettings 
{ 
    OutputType = GitVersionOutput.Json,
    ArgumentCustomization = args => args.Append("/updateprojectfiles")
});
var branchName = gitVersion.BranchName;

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var repoName = "Lanceur Bis";

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release").ToLower();
var verbosity       = Argument("verbosity", Verbosity.Minimal);

var binDirectory    = $"./src/Lanceur/bin/{configuration}/net6.0-windows10.0.19041.0/";
var setupIconFile   = $"./src/Lanceur/Assets/appIcon.ico";
var publishDir      = "./Publish";
var zipName         = new FilePath(publishDir + "/Lanceur." + gitVersion.SemVer + ".bin.zip").FullPath;
var innoSetupName   = new FilePath(publishDir + "/Lanceur." + gitVersion.SemVer + ".setup.exe").FullPath;
var inno_setup      = "./setup.iss";
///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
    if(!IsRunningOnWindows())
    {
        throw new NotImplementedException($"{repoName} should only run on Windows");
    }
    
    Information(Figlet($"{repoName}"));

    Information("Configuration             : {0}", configuration);
    Information("Branch                    : {0}", branchName);
    Information("Informational      Version: {0}", gitVersion.InformationalVersion);
    Information("SemVer             Version: {0}", gitVersion.SemVer);
    Information("AssemblySemVer     Version: {0}", gitVersion.AssemblySemVer);
    Information("AssemblySemFileVer Version: {0}", gitVersion.AssemblySemFileVer);
    Information("MajorMinorPatch    Version: {0}", gitVersion.MajorMinorPatch);
    Information("NuGet              Version: {0}", gitVersion.NuGetVersion);  
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////
Task("info")
    .Does(()=> { 
        /*Does nothing but specifying information of the build*/ 
});

Task("clean")
    .Does(()=> {
        var dirToDelete = GetDirectories("./**/obj")
                            .Concat(GetDirectories("./**/bin"))
                            .Concat(GetDirectories("./**/Output"))
                            .Concat(GetDirectories("./**/Publish"));
        DeleteDirectories(dirToDelete, new DeleteDirectorySettings{ Recursive = true, Force = true});
});

Task("zip")
    .Does(()=> {
        var path = MakeAbsolute(Directory(binDirectory));

        Information("Bin path   : {0}", path);
        Information("Output dir : {0}", zipName);

        EnsureDirectoryExists(Directory(publishDir));
        Zip(path, zipName);
});

Task("inno-setup")
    .Does(() => {
        var path = MakeAbsolute(Directory(binDirectory));

        Information("Bin path   : {0}", path);
        Information("Output dir : {0}", publishDir);

        InnoSetup(inno_setup, new InnoSetupSettings { 
            OutputDirectory = publishDir,
            Defines = new Dictionary<string, string> {
                { "MyAppVersion", gitVersion.SemVer },
                { "BinDirectory", path.FullPath + "/" },
                { "SetupIconFile", setupIconFile },
            }
        });
});

Task("build")
    .Does(() => {  
        var settings = new DotNetBuildSettings {
            Configuration = "release"
        };
        DotNetBuild(solution, settings);        
});

Task("tests")
    .Does(()=>{
        var projects = GetFiles("./src/Tests/**/*.csproj");
        foreach(var project in projects)
        {
            DotNetTest(
                project.FullPath,
                new DotNetTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true
                });
        }
});

///////////////////////////////////////////////////////////////////////////////
// BATCHES
///////////////////////////////////////////////////////////////////////////////
Task("release-github")
    .Does(()=>{
        //https://stackoverflow.com/questions/42761777/hide-services-passwords-in-cake-build
        var token = EnvironmentVariable("CAKE_PUBLIC_GITHUB_TOKEN");
        var owner = EnvironmentVariable("CAKE_PUBLIC_GITHUB_USERNAME");

        var fZip = MakeAbsolute(File(zipName));
        var fInn = MakeAbsolute(File(innoSetupName));
        var assets = @$"""{fZip}"",  ""{fInn}""";
        
        Information("Has token     : {0}", !string.IsNullOrEmpty(token));
        Information("Has owner     : {0}", !string.IsNullOrEmpty(owner));
        Information("Zip           : {0}", fZip);
        Information("Inno setup    : {0}", fInn);
        Information($"Assets to add: '{assets}'");



        var stg = new GitReleaseManagerCreateSettings 
        {
            Milestone  = gitVersion.MajorMinorPatch,            
            Name       = gitVersion.SemVer,
            Prerelease = gitVersion.SemVer.Contains("alpha"),
            Debug      = false,
            Assets     = assets
        };

        GitReleaseManagerCreate(token, owner, "lanceur-bis", stg);  
    });


Task("default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Tests");

Task("bin")
    .IsDependentOn("default")
    .IsDependentOn("zip")
    .IsDependentOn("inno-setup");

    
Task("github")    
    .IsDependentOn("bin")
    .IsDependentOn("Release-GitHub");

RunTarget(target);