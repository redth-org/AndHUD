#tool nuget:?package=GitVersion.CommandLine&version=5.10.3
#addin nuget:?package=Cake.Figlet&version=2.0.1

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var sln = new FilePath("./AndHUD.sln");
var artifactsDir = new DirectoryPath("./artifacts");
var gitVersionLog = new FilePath("./gitversion.log");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
GitVersion versionInfo = null;

Setup(context => 
{
   versionInfo = context.GitVersion(new GitVersionSettings 
   {
      UpdateAssemblyInfo = true,
      OutputType = GitVersionOutput.Json,
      LogFilePath = gitVersionLog.MakeAbsolute(context.Environment)
   });

   if (isRunningOnAppVeyor)
   {
      var buildNumber = AppVeyor.Environment.Build.Number;
      AppVeyor.UpdateBuildVersion(versionInfo.InformationalVersion
         + "-" + buildNumber);
   }

   var cakeVersion = typeof(ICakeContext).Assembly.GetName().Version.ToString();

   Information(Figlet("AndHUD"));
   Information("Building version {0}, ({1}, {2}) using version {3} of Cake.",
      versionInfo.SemVer,
      configuration,
      target,
      cakeVersion);
});

Task("Clean")
   .Does(() =>
{
   CleanDirectories("./AndHUD/bin");
   CleanDirectories("./AndHUD/obj");
   CleanDirectories("./lib");

   EnsureDirectoryExists(artifactsDir);
});

Task("Restore")
   .Does(() => 
{
   DotNetRestore(sln.ToString());
});

Task("Build")
   .IsDependentOn("Clean")
   .IsDependentOn("Restore")
   .Does(() => 
{
    var msBuildSettings = new DotNetMSBuildSettings
    {
        Version = versionInfo.SemVer,
        PackageVersion = versionInfo.SemVer,
        InformationalVersion = versionInfo.InformationalVersion
    };

    var settings = new DotNetBuildSettings
    {
         Configuration = configuration,
         OutputDirectory = artifactsDir.ToString()
    };
   
    DotNetBuild(sln.ToString(), settings);
});

Task("CopyArtifacts")
   .IsDependentOn("Build")
   .Does(() => 
{
   var nugetFiles = GetFiles("AndHUD/bin/" + configuration + "/**/*.nupkg");
   CopyFiles(nugetFiles, artifactsDir);
   CopyFileToDirectory(gitVersionLog, artifactsDir);
});

Task("Default")
   .IsDependentOn("CopyArtifacts");

RunTarget(target);
