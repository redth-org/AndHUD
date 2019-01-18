#tool nuget:?package=GitVersion.CommandLine
#tool nuget:?package=vswhere
#addin nuget:?package=Cake.Figlet

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var sln = new FilePath("./AndHUD.sln");

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
GitVersion versionInfo = null;

Setup(context => 
{
   versionInfo = context.GitVersion(new GitVersionSettings 
   {
      UpdateAssemblyInfo = true,
      OutputType = GitVersionOutput.Json
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
});

FilePath msBuildPath;
Task("ResolveBuildTools")
   .WithCriteria(() => IsRunningOnWindows())
   .Does(() => 
{
   var vsWhereSettings = new VSWhereLatestSettings
   {
      IncludePrerelease = true,
      Requires = "Component.Xamarin"
   };

   var vsLatest = VSWhereLatest(vsWhereSettings);
   msBuildPath = (vsLatest == null)
      ? null
      : vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

   if (msBuildPath != null)
      Information("Found MSBuild at {0}", msBuildPath.ToString());
});

Task("Restore")
   .IsDependentOn("ResolveBuildTools")
   .Does(() => 
{
   var settings = GetDefaultBuildSettings()
      .WithTarget("Restore");
   MSBuild(sln, settings);
});

Task("Build")
   .IsDependentOn("ResolveBuildTools")
   .IsDependentOn("Clean")
   .IsDependentOn("Restore")
   .Does(() =>  {

   var settings = GetDefaultBuildSettings()
      .WithTarget("Build");

   MSBuild(sln, settings);
});

Task("Pack")
   .IsDependentOn("Build")
   .Does(() => 
{
   var settings = new NuGetPackSettings
   {
      Version = versionInfo.NuGetVersionV2,
      OutputDirectory = "./"
   };

   NuGetPack("./AndHUD.nuspec", settings);
});

Task("Default")
   .IsDependentOn("Pack");

RunTarget(target);

MSBuildSettings GetDefaultBuildSettings()
{
   var settings = new MSBuildSettings 
   {
      Configuration = configuration,
      ArgumentCustomization = args => args.Append("/m"),
      ToolVersion = MSBuildToolVersion.VS2017
   };

   if (msBuildPath != null)
      settings.ToolPath = msBuildPath;

   return settings;
}