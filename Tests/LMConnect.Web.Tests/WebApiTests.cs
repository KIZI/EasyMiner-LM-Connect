using System;
using LMConnect.Client;
using NUnit.Framework;

namespace LMConnect.Web.Tests
{
	[TestFixture]
	public class WebApiTests
	{
		private const string LMcloudServer = "http://localhost";
		private const string TestCasesPath = "TestCases";

		private LMConnect.Client.Client _client;

		[SetUp]
		public void Init()
		{
			this._client = new Client.Client(LMcloudServer);
		}

		[TearDown]
		public void Cleanup()
		{
		}

		[Test]
		public async void RegisterMiner()
		{
			try
			{
				Miner miner = await _client.RegisterAsync(@"<RegistrationRequest sharedBinaries=""true"">
	<Connection type=""Access"">
		<File>Barbora.mdb</File>
	</Connection>
	<Metabase type=""Access"">
		<File>LM Barbora.mdb</File>
	</Metabase>
</RegistrationRequest>");

				Assert.IsNotNull(miner);
			}
			catch (Exception exception)
			{
				Assert.Fail(exception.Message);
			}
		}
	}
}
