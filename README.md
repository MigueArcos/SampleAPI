# Useful commands
## PowerShell util functions 
```powershell
function cleanTestResults {
  ## cmd.exe /c 'FOR /d /r . %d IN (TestResults) DO @IF EXIST "%d" (echo "%d" & rd /s /q "%d")'
  Get-ChildItem ./ -Include TestResults -Recurse | ForEach-Object {
    Write-Host "Deleting folder: $($_.FullName)"
    Remove-Item $_.FullName -Force -Recurse
  }
}

function cleanDotNet {
  Get-ChildItem ./ -Include bin,obj -Recurse | ForEach-Object {
    Write-Host "Deleting folder: $($_.FullName)"
    Remove-Item $_.FullName -Force -Recurse
  }
}

function generateTestsReport {
  reportgenerator "-reports:./tests/**/TestResults/**/coverage.cobertura.xml" "-targetdir:./TestResults" "-reporttypes:htmlInline"
}
```

## Same util functions for Linux
```bash
alias cleanDotNet="find . -type d '(' -name bin -o -name obj ')' -prune -print0 | xargs -I {} -0 rm -vrf {}"
alias cleanTestResults="find . -type d '(' -name TestResults ')' -prune -print0 | xargs -I {} -0 rm -vrf {}"
alias generateTestsReport='reportgenerator "-reports:./tests/**/TestResults/**/coverage.cobertura.xml" "-targetdir:./TestResults" "-reporttypes:htmlInline"'
```

## Shorthand all in one to run Unit Tests and generate Reports
```bash
# Run unit tests and create coverage report
cleanTestResults && dotnet test --collect:"XPlat Code Coverage" && generateTestsReport
```