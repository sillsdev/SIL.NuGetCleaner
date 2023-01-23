# SIL.NuGetCleaner

[![NuGet version (SIL.NuGetCleaner)](https://img.shields.io/nuget/v/sil.nugetcleaner.svg?style=flat-square)](https://www.nuget.org/packages/SIL.NuGetCleaner)
[![Build, Test and Pack](https://github.com/sillsdev/SIL.NuGetCleaner/actions/workflows/CI-CD.yml/badge.svg)](https://github.com/sillsdev/SIL.NuGetCleaner/actions/workflows/CI-CD.yml)

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
