
/* ----------------------------------------------------------------------------
 * Build script for Lanceur Bis
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
#tool nuget:?package=GitVersion.CommandLine&version=5.12.0
#tool nuget:?package=GitReleaseManager&version=0.17.0

#addin nuget:?package=Cake.Figlet&version=2.0.1

///////////////////////////////////////////////////////////////////////////////
// PREPARATION
///////////////////////////////////////////////////////////////////////////////
var solution = "./Lanceur.sln";

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var repoName = "lanceur-bis";

var target          = Argument("target", "Default");
var configuration   = Argument("configuration", "Release").ToLower();
var verbosity       = Argument("verbosity", Verbosity.Minimal);

var binDirectory    = $"./src/Lanceur/bin/{configuration}/net6.0-windows10.0.19041.0/";
var setupIconFile   = $"./src/Lanceur/Assets/appIcon.ico";
var publishDir      = "./Publish";
var inno_setup      = "./setup.iss";

GitVersion gitVersion;
string zipName;
string innoSetupName;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{

    // https://gitversion.net/docs/usage/cli/arguments
    // https://cakebuild.net/api/Cake.Core.Tooling/ToolSettings/50AAB3A8
    gitVersion = GitVersion(new GitVersionSettings 
    { 
        OutputType            = GitVersionOutput.Json,
        Verbosity             = GitVersionVerbosity.Verbose,        
        ArgumentCustomization = args => args.Append("/updateprojectfiles")
    });
    var branchName = gitVersion.BranchName;

    zipName         = new FilePath(publishDir + "/Lanceur." + gitVersion.SemVer + ".bin.zip").FullPath;
    innoSetupName   = new FilePath(publishDir + "/Lanceur." + gitVersion.SemVer + ".setup.exe").FullPath;

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
        Information("Cleaning files...");
        var dirToDelete = GetDirectories("./**/obj")
                            .Concat(GetDirectories("./**/bin"))
                            .Concat(GetDirectories("./**/Output"))
                            .Concat(GetDirectories("./**/Publish"));
        DeleteDirectories(dirToDelete, new DeleteDirectorySettings{ Recursive = true, Force = true});

        DotNetTool(
            solution,
            "clean"
        );
});

Task("restore")
    .Does(()=>{

        DotNetTool(
            solution,
            "restore"
        );
});

Task("build")
    .Does(() => {        
        DotNetTool(
            solution,
            "build",
            "--no-restore -c release"
        );
});

Task("test")
    .Does(()=>{

        DotNetTool(
            solution,
            "test",
            "--no-restore --no-build -c release"
        );
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

        Information("Bin path      : {0}", path);
        Information("Output dir    : {0}", publishDir);
        Information("MyAppVersion  : {0}", gitVersion?.SemVer ?? "<NULL>");
        Information("BinDirectory  : {0}", path?.FullPath ?? "<NULL>");
        Information("SetupIconFile : {0}", setupIconFile ?? "<NULL>");

        InnoSetup(inno_setup, new InnoSetupSettings { 
            OutputDirectory = publishDir,
            Defines = new Dictionary<string, string> {
                { "MyAppVersion", gitVersion.SemVer },
                { "BinDirectory", path.FullPath + "/" },
                { "SetupIconFile", setupIconFile },
            }
        });
});

///////////////////////////////////////////////////////////////////////////////
// BATCHES
///////////////////////////////////////////////////////////////////////////////
Task("relnote")
    .Does(()=>{
        //https://stackoverflow.com/questions/42761777/hide-services-passwords-in-cake-build
        var token = EnvironmentVariable("CAKE_PUBLIC_GITHUB_TOKEN");
        var owner = EnvironmentVariable("CAKE_PUBLIC_GITHUB_USERNAME");

        var fZip = MakeAbsolute(File(zipName));
        var fInn = MakeAbsolute(File(innoSetupName));
        var assets = $"{fZip},{fInn}";

        var alphaVersions = new[] { "alpha", "beta" };
        var isPrerelease = alphaVersions.Any(x => gitVersion.SemVer.ToLower().Contains(x));

        if(isPrerelease) { Information("This is a prerelease"); }        
        Information("Has token      : {0}", !string.IsNullOrEmpty(token));
        Information("Has owner      : {0}", !string.IsNullOrEmpty(owner));
        Information("Zip            : {0}", fZip);
        Information("Inno setup     : {0}", fInn);

        var parameters = $"create --token {token} -o {owner} -r {repoName} " +
                         $"--milestone {gitVersion.MajorMinorPatch} --name {gitVersion.SemVer} " +
                         $"{(isPrerelease ? "--pre" : "")} " +
                         $"--targetDirectory {Environment.CurrentDirectory} " + 
                         $"--assets {assets}"
                         // + "--debug --verbose"
                         ;
        DotNetTool(
            solution, 
            "gitreleasemanager",
            parameters 
        );
    });


Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .IsDependentOn("build")
    .IsDependentOn("test");

Task("bin")
    .IsDependentOn("default")
    .IsDependentOn("zip")
    .IsDependentOn("inno-setup");

    
Task("github")    
    .IsDependentOn("bin")
    .IsDependentOn("relnote");

RunTarget(target);
