// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace SIL.NuGetCleaner
{
	public class NuGetPackage
	{
		private readonly string                _packageId;
		private          dynamic               _nugetPackageJson;
		private          List<SemanticVersion> _versions;
		private readonly string                _apiKey;
		private readonly string                _maxVersionString;

		public static HttpClient HttpClient { private get; set; }

		public NuGetPackage(string packageId, string apiKey = null, string min = "", string max = "")
		{
			_packageId = packageId;
			_apiKey = apiKey;
			if (HttpClient == null)
				HttpClient = new HttpClient();

			if (!string.IsNullOrEmpty(min))
			{
				Minimum = SemanticVersion.Parse(min);
			}

			_maxVersionString = max;
		}

		public async Task<List<SemanticVersion>> GetVersions()
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				$"https://azuresearch-usnc.nuget.org/query?q=packageid:{_packageId}&prerelease=true&semVerLevel=2.0.0");
			request.Headers.Accept.Add(
				new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
			var responseBody = await HttpClient.SendAsync(request);
			var responseString = await responseBody.Content.ReadAsStringAsync();
			_nugetPackageJson = JsonConvert.DeserializeObject(responseString);
			if (_nugetPackageJson.data.Count <= 0)
				return null;

			var versionString = _nugetPackageJson.data[0].version.ToString();
			CurrentVersion = SemanticVersion.Parse(versionString);
			var jsonVersions = _nugetPackageJson.data[0].versions as JArray;
			_versions = jsonVersions.Select(versionInfo =>
				SemanticVersion.Parse(versionInfo["version"].ToString())).ToList();
			return _versions;

		}

		public async Task<List<SemanticVersion>> GetPrereleaseVersionsToDelete()
		{
			var versions = await GetVersions();
			if (versions == null)
				return null;

			var prereleaseVersions = new List<SemanticVersion>();
			foreach (var semVer in versions)
			{
				if (!semVer.IsPrerelease)
					continue;

				if (semVer <= Minimum || semVer >= Maximum)
					continue;

				prereleaseVersions.Add(semVer);
			}

			return prereleaseVersions;
		}

		public async Task<string> DeletePackage(SemanticVersion version)
		{
			if (version == null)
				throw new ArgumentException("Invalid version", nameof(version));

			var request = new HttpRequestMessage(HttpMethod.Delete,
				$"https://www.nuget.org/api/v2/package/{_packageId}/{version}");
			request.Headers.Add("X-NuGet-ApiKey", _apiKey);

			var response = await HttpClient.SendAsync(request);
			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
				case HttpStatusCode.Accepted:
					return response.ReasonPhrase;
				case HttpStatusCode.Unauthorized:
					throw new UnauthorizedAccessException(response.ReasonPhrase);
				case HttpStatusCode.Forbidden:
					// https://docs.microsoft.com/en-us/nuget/api/rate-limits
					throw new QuotaExceededException(response.ReasonPhrase);
				case HttpStatusCode.NotFound:
					throw new VersionNotFoundException(response.ReasonPhrase);
				default:
					throw new HttpRequestException(
						$"{response.StatusCode}: {response.ReasonPhrase}");
			}
		}

		public SemanticVersion CurrentVersion { get; private set; }

		private SemanticVersion _latestRelease;
		public SemanticVersion LatestRelease
		{
			get
			{
				if (_latestRelease == null)
				{
					for (var i = _versions.Count - 1; i >= 0; i--)
					{
						var version = _versions[i];
						if (version.IsPrerelease)
							continue;

						_latestRelease = version;
						break;
					}
				}

				return _latestRelease;
			}
		}

		public SemanticVersion Minimum { get; }

		private SemanticVersion _maximum;
		public SemanticVersion Maximum
		{
			get
			{
				if (_maximum != null)
					return _maximum;

				if (_maxVersionString == "-1")
					_maximum = new SemanticVersion(int.MaxValue, int.MaxValue, int.MaxValue);
				else
				{
					_maximum = string.IsNullOrEmpty(_maxVersionString)
						? LatestRelease
						: SemanticVersion.Parse(_maxVersionString);
				}

				return _maximum;
			}
		}
	}
}
