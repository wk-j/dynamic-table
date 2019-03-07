#addin "wk.StartProcess"
#addin "wk.ProjectParser"

using PS = StartProcess.Processor;
using ProjectParser;

var npi = EnvironmentVariable("npi");
var name = "DynamicTables";
var publishDir = ".publish";
var version = DateTime.Now.ToString("yy.MM.dd.HHss");

var currentDir = new DirectoryInfo(".").FullName;
var info = Parser.Parse($"src/{name}/{name}.csproj");

Task("Pack").Does(() => {
    CleanDirectory("publish");
    var settings = new DotNetCoreMSBuildSettings();
    settings.Properties["Version"] = new string[] { version };

    DotNetCorePack($"src/{name}", new DotNetCorePackSettings {
        OutputDirectory = publishDir,
        MSBuildSettings = settings
    });
});

Task("Publish-NuGet")
    .IsDependentOn("Pack")
    .Does(() => {
        var nupkg = new DirectoryInfo(publishDir).GetFiles("*.nupkg").LastOrDefault();
        var package = nupkg.FullName;
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = npi
        });
});

var target = Argument("target", "Pack");
RunTarget(target);