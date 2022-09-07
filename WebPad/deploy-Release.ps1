
$deployPath = "$($env:USERPROFILE)\programs\WebPad"
$buildConfig = "Release"

# clear existing folder
remove-item ([system.io.path]::Combine($deployPath, "*")) -Recurse -Force


& dotnet @("publish","-c", $buildConfig, "-o", "`"$deployPath`"", "-p:PublishSingleFile=true",
		"-p:RuntimeIdentifier=win-x64", "-p:SelfContained=false", "-p:ExcludeSymbolsFromSingleFile=true",
		"-nowarn:nu1605"
 )

