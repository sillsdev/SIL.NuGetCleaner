# SIL.NuGetCleaner

## Description

`nugetclean` allows to unlist pre-release versions of a nuget package once a new
version is released.

## Installation

`nugetclean` is available as a .NET Core Global Tool:

```bash
dotnet tool install --global SIL.NuGetCleaner
```

## Usage

```bash
nugetclean --api-key=<nuget api key> [--dry-run] <packageid>
```

`--api-key` - the NuGet API key with the permission to unlist a package

`--dry-run` - only list the package version that would be unlisted, but don't do it

`<packageid>` is the NuGet package id.
