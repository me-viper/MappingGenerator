#! /bin/pwsh

if (Test-Path ./artifacts) {
    rm ./artifacts -Recurse -Force
}

dotnet build --no-incremental `
    && dotnet test `
    && dotnet pack -o ./artifacts