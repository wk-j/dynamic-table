var project = "DynamicTables";
var path = $"src/{project}/{project}.csproj";
var npi = EnvironmentVariable("npi");

Task("Create-NuGet")
    .Does(() => {
        DotNetCorePack(path, new DotNetCorePackSettings {
            Configuration = "Release",
            OutputDirectory = "./artifacts"
        });
    });

Task("Publish-NuGet")
    .IsDependentOn("Create-NuGet")
    .Does(() => {
        var nupkg = new DirectoryInfo("artifacts").GetFiles("*.nupkg").LastOrDefault();
        var package = nupkg.FullName;
        NuGetPush(package, new NuGetPushSettings {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = npi
        });
});

var target = Argument("target", "default");
RunTarget(target);