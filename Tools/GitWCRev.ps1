$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

$template = $executingScriptDirectory + "\..\Sources\LMConnect.Web\SVNDataAttribute.template.cs"
$output = $executingScriptDirectory + "\..\Sources\LMConnect.Web\SVNDataAttribute.cs"

Get-Content $template |
	ForEach-Object { $_ -replace '\$WCNOW\$', (Get-Date -Format G) } |
	ForEach-Object { $_ -replace '\$WCREV\$', (git describe --always) } |
	ForEach-Object { $_ -replace '\$WCDATE\$', [datetime]::Parse([string](git show -s --format=%cD)).ToString("G") } |
	Set-Content $output