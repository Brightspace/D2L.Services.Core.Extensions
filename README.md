D2L.Services.Core.Extensions
============================

Common D2L extension methods for .NET classes

Releasing new versions
----------------------

This library uses [semantic versioning](http://semver.org).

Make sure you've opened and built the package in Visual Studio at least once so that the `build` folder exists.

After your changes have been merged into master and your working directory is clean and up to date you need to decide how to bump the version.

Assuming you want to bump the minor version, run `build/Bump_Minor.ps1`. This will increment the version and push to `origin/master`.
Jenkins will then take care of deploying the new package for you and email the results (TODO).
