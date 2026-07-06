namespace Control.Infrastructure.IntegrationTests.Infrastructure;

[CollectionDefinition("IntegrationTestCollection")]
public class IntegrationTestCollection
    : ICollectionFixture<IntegrationTestWebAppFactory>
{
}
