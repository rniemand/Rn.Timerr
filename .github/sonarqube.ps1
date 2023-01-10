param (
  [Parameter(Mandatory=$false)]
  [string] $project = "Rn.Timerr",

  [Parameter(Mandatory=$false)]
  [string] $sqUrl = "http://localhost:9000",

  [Parameter(Mandatory=$true)]
  [string] $sqToken
)

$rootDir        = $PSScriptRoot;
$artifactsDir 	= [IO.Path]::GetFullPath((Join-Path $rootDir "../artifacts/"));
$coverageDir	  = [IO.Path]::GetFullPath((Join-Path $artifactsDir "test-coverage/"));
$sqReportPaths  = ($coverageDir + "**/coverage.opencover.xml");

dotnet sonarscanner begin /k:$project /d:sonar.host.url=$sqUrl  /d:sonar.login=$sqToken /d:sonar.cs.opencover.reportsPaths=$sqReportPaths
./ci-test.ps1
dotnet sonarscanner end /d:sonar.login=$sqToken
