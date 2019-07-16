// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using CommandLine;

namespace SIL.NuGetCleaner
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Options
	{
		[Option("api-key", HelpText = "The NuGet API key.")]
		public string ApiKey { get; set; }

		[Option("dry-run", HelpText = "Lists the pre-release versions to unlist.")]
		public bool DryRun { get; set; }

		[Value(0, Required = true, HelpText = "The NuGet package id.", MetaName = "packageId")]
		public string PackageId { get; set; }
	}
}
