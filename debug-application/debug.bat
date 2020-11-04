@echo off
set curDir=%CD%
dotnet watch --project ..\src\OneScript\OneScriptWeb.csproj watch run --framework netcoreapp3.1 --ContentRoot %curDir%
