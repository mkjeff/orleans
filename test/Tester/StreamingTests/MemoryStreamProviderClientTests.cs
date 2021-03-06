
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Providers.Streams.Generator;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.TestingHost;
using TestExtensions;
using Xunit;
using Xunit.Abstractions;

namespace Tester.StreamingTests
{
    public class MemoryStreamProviderClientTests : OrleansTestingBase, IClassFixture<MemoryStreamProviderClientTests.Fixture>
    {
        public class Fixture : BaseTestClusterFixture
        {
            public const string StreamProviderName = "MemoryStreamProvider";
            public const string StreamNamespace = "StreamNamespace";
            private const int partitionCount = 8;
            protected override void ConfigureTestCluster(TestClusterBuilder builder)
            {
                builder.AddSiloBuilderConfigurator<MySiloBuilderConfigurator>();
                builder.AddClientBuilderConfigurator<MyClientBuilderConfigurator>();
            }

            private class MyClientBuilderConfigurator : IClientBuilderConfigurator
            {
                public void Configure(IConfiguration configuration, IClientBuilder clientBuilder) => clientBuilder
                        .AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviderName, b=>b
                    .ConfigurePartitioning(partitionCount));
            }

            private class MySiloBuilderConfigurator : ISiloBuilderConfigurator
            {
                public void Configure(ISiloHostBuilder hostBuilder)=> hostBuilder.AddMemoryGrainStorage("PubSubStore")
                        .AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviderName, b=>b
                    .ConfigurePartitioning(partitionCount))
                    .Configure<SiloMessagingOptions>(options => options.ClientDropTimeout = TimeSpan.FromSeconds(5));
            }
        }

        private readonly ITestOutputHelper output = null;
        private readonly ClientStreamTestRunner runner;

        private Fixture fixture;

        public MemoryStreamProviderClientTests(Fixture fixture)
        {
            this.fixture = fixture;
            runner = new ClientStreamTestRunner(fixture.HostedCluster);
        }

        [Fact, TestCategory("Functional"), TestCategory("Streaming")]
        public async Task MemoryStreamProducerOnDroppedClientTest()
        {
            this.fixture.Logger.Info("************************ MemoryStreamProducerOnDroppedClientTest *********************************");
            await runner.StreamProducerOnDroppedClientTest(Fixture.StreamProviderName, Fixture.StreamNamespace);
        }

        [Fact, TestCategory("Functional"), TestCategory("Streaming")]
        public async Task MemoryStreamConsumerOnDroppedClientTest()
        {
            this.fixture.Logger.Info("************************ MemoryStreamConsumerOnDroppedClientTest *********************************");
            await runner.StreamConsumerOnDroppedClientTest(Fixture.StreamProviderName, Fixture.StreamNamespace, output,
                    null, true);
        }
    }
}
