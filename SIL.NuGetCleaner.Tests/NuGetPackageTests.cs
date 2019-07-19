// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NuGet.Versioning;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace SIL.NuGetCleaner.Tests
{
	public class NuGetPackageTests
	{
		private const string responseJsonBegin = @"{
			""@context"": {
				""@vocab"": ""http://schema.nuget.org/schema#"",
				""@base"": ""https://api.nuget.org/v3/registration3/""
			},
			""totalHits"": 1,
			""lastReopen"": ""2019-07-10T08:50:51.6155640Z"",
			""index"": ""v3-lucene2-v2v3-20171018"",
			""data"": [
				{
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/index.json"",
					""@type"": ""Package"",
					""registration"": ""https://api.nuget.org/v3/registration3/l10nsharp/index.json"",
					""id"": ""L10NSharp"",
					""version"": """;
			private const string responseJsonMiddle = @""",
					""description"": ""L10NSharp is a .NET localization library"",
					""summary"": """",
					""title"": ""L10NSharp"",
					""licenseUrl"": ""https://www.nuget.org/packages/L10NSharp/4.0.2/license"",
					""projectUrl"": ""https://github.com/sillsdev/l10nsharp"",
					""tags"": [],
					""authors"": [
						""SIL International""
					],
					""totalDownloads"": 1445,
					""verified"": false,
					""versions"": [";
		private const string responseJsonEnd = @"]
				}
			]
		}";

		private const string allVersions = @"
			{
				""version"": ""4.0.0"",
				""downloads"": 73,
				""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.0.json""
			},
			{
				""version"": ""4.0.1"",
				""downloads"": 28,
				""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.1.json""
			},
			{
				""version"": ""4.0.2-beta0003"",
				""downloads"": 0,
				""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2-beta0003.json""
			},
			{
				""version"": ""4.0.2"",
				""downloads"": 0,
				""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
			}";

		private MockHttpMessageHandler _mockHttp;

		[SetUp]
		public void SetUp()
		{
			_mockHttp = new MockHttpMessageHandler();
		}

		[TearDown]
		public void TearDown()
		{
			_mockHttp.Dispose();
		}

		[Test]
		public async Task GetVersions()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.2" + responseJsonMiddle +
				allVersions + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetVersions(),
				Is.EquivalentTo(new [] {
					SemanticVersion.Parse("4.0.0"),
					SemanticVersion.Parse("4.0.1"),
					SemanticVersion.Parse("4.0.2-beta0003"),
					SemanticVersion.Parse("4.0.2")
				}));
		}

		[Test]
		public async Task GetPrereleaseVersionsToDelete()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.2" + responseJsonMiddle +
				allVersions + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetPrereleaseVersionsToDelete(),
				Is.EquivalentTo(new [] { SemanticVersion.Parse("4.0.2-beta0003") }));
		}

		[Test]
		public async Task GetPrereleaseVersionsToDelete_TipIsUnreleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.2"",
					""downloads"": 28,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
				},
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetPrereleaseVersionsToDelete(), Is.Empty);
		}

		[Test]
		public async Task GetPrereleaseVersionsToDelete_TipIsUnreleased_MultiplePrereleaseVersions()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.2"",
					""downloads"": 28,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
				},
				{
					""version"": ""4.0.3-beta0002"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0002.json""
				},
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetPrereleaseVersionsToDelete(), Is.Empty);
		}

		[Test]
		public async Task GetPrereleaseVersionsToDelete_TipIsUnreleased_PreviousUnreleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.2-beta0002"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2-beta0002.json""
				},
				{
					""version"": ""4.0.2"",
					""downloads"": 28,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
				},
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetPrereleaseVersionsToDelete(),
				Is.EquivalentTo(new [] { SemanticVersion.Parse("4.0.2-beta0002") }));
		}

		[Test]
		public async Task GetPrereleaseVersionsToDelete_TipIsUnreleased_OnlyUnreleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			Assert.That(await sut.GetPrereleaseVersionsToDelete(), Is.Empty);
		}

		[TestCase(HttpStatusCode.Unauthorized)]
		[TestCase(HttpStatusCode.Forbidden)]
		public void DeletePackage_Unauthorized(HttpStatusCode statusCode)
		{
			_mockHttp.Expect(HttpMethod.Delete, "https://www.nuget.org/api/v2/package/L10NSharp/4.0.3-beta0003")
				.WithHeaders("X-NuGet-ApiKey", "apikey12345").Respond(statusCode);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp", "apikey12345");
			Assert.That(async () => await sut.DeletePackage(SemanticVersion.Parse("4.0.3-beta0003")),
				Throws.Exception.TypeOf<UnauthorizedAccessException>());
		}

		[Test]
		public void DeletePackage_IllegalVersion()
		{
			_mockHttp.Expect(HttpMethod.Delete, "https://www.nuget.org/api/v2/package/L10NSharp/4.0.3-beta0003")
				.WithHeaders("X-NuGet-ApiKey", "apikey12345");
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp", "apikey12345");
			Assert.That(async () => await sut.DeletePackage(null),
				Throws.Exception.TypeOf<ArgumentException>());
		}

		[TestCase(HttpStatusCode.Accepted)]
		[TestCase(HttpStatusCode.OK)]
		public void DeletePackage_HappyPath(HttpStatusCode statusCode)
		{
			_mockHttp.Expect(HttpMethod.Delete, "https://www.nuget.org/api/v2/package/L10NSharp/4.0.3-beta0003")
				.WithHeaders("X-NuGet-ApiKey", "apikey12345").Respond(statusCode);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp", "apikey12345");
			Assert.That(async () => await sut.DeletePackage(SemanticVersion.Parse("4.0.3-beta0003")),
				Throws.Nothing);
		}

		[Test]
		public async Task CurrentVersion_TipIsReleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.2" + responseJsonMiddle +
				allVersions + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			await sut.GetVersions();
			Assert.That(sut.CurrentVersion, Is.EqualTo(SemanticVersion.Parse("4.0.2")));
		}

		[Test]
		public async Task CurrentVersion_TipIsUnreleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.2"",
					""downloads"": 28,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
				},
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			await sut.GetVersions();
			Assert.That(sut.CurrentVersion, Is.EqualTo(SemanticVersion.Parse("4.0.3-beta0003")));
		}

		[Test]
		public async Task LatestRelease_TipIsReleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.2" + responseJsonMiddle +
				allVersions + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			await sut.GetVersions();
			Assert.That(sut.LatestRelease, Is.EqualTo(SemanticVersion.Parse("4.0.2")));
		}

		[Test]
		public async Task LatestRelease_TipIsUnreleased()
		{
			_mockHttp.When("https://api-v2v3search-0.nuget.org/query?q=packageid:L10NSharp&prerelease=true")
				.Respond("application/json", responseJsonBegin + "4.0.3-beta0003" +
				responseJsonMiddle + @"
				{
					""version"": ""4.0.2"",
					""downloads"": 28,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.2.json""
				},
				{
					""version"": ""4.0.3-beta0003"",
					""downloads"": 0,
					""@id"": ""https://api.nuget.org/v3/registration3/l10nsharp/4.0.3-beta0003.json""
				}" + responseJsonEnd);
			NuGetPackage.HttpClient = _mockHttp.ToHttpClient();

			var sut = new NuGetPackage("L10NSharp");
			await sut.GetVersions();
			Assert.That(sut.LatestRelease, Is.EqualTo(SemanticVersion.Parse("4.0.2")));
		}
	}
}
