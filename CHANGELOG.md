# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Changed

- in case we exceed the quota on nuget.org we wait 60 minutes and try again.

## [1.2.2] - 2021-03-04

### Changed

- use .NET 5.0

### Fixed

- allow to run on Linux (again)

## [1.2.1] - 2021-03-04

### Fixed

- allow to run on Linux (again)

## [1.2.0] - 2021-03-04

### Changed

- internal housekeeping
- use netcore 3.1

### Fixed

- find all packages, also ones that use SemVer v2.0.0

## [1.1.1] - 2019-07-24

### Fixed

- deal with exception when we don't get any versions back

## [1.1.0] - 2019-07-24

### Added

- allow to specify minimum and maximum versions

## [1.0.1] - 2019-07-19

### Changed

- some internal refactorings

## [1.0.0] - 2019-07-17

### Added

- Initial release
