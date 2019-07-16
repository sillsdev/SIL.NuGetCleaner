// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommandLine;

namespace SIL.NuGetCleaner
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(options => Execute(options).Wait());
		}

		private static async Task Execute(Options options)
		{
			Console.WriteLine($"Checking {options.PackageId} for pre-release versions:");
			var nugetPackage = new NuGetPackage(options.PackageId, options.ApiKey);
			var versionsToDelete = await nugetPackage.GetPrereleaseVersionsToDelete();
			if (versionsToDelete.Count == 0)
			{
				Console.WriteLine("No obsolete pre-release versions found.");
				return;
			}
			Console.WriteLine($"Found {versionsToDelete.Count} pre-release versions to unlist:");

			try
			{
				foreach (var version in versionsToDelete)
				{
					if (options.DryRun)
						Console.WriteLine(version);
					else
					{
						Console.WriteLine($"Unlisting {version}");
						var msg = await nugetPackage.DeletePackage(version);
						Console.WriteLine(msg);
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine(
					$"ERROR: Not authorized to unlist {options.PackageId}. Check your NuGet API key.");
			}
			catch (AggregateException e)
			{
				Console.WriteLine($"ERROR: {e.InnerException.Message}");
			}
			catch (HttpRequestException eh)
			{
				Console.WriteLine($"ERROR: {eh.Message}");
			}
		}
	}
}
