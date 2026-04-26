namespace Creomobile.Data.EFCore.IntegrationTests;

public sealed class UnitTest1(PostgresFixture postgresFixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public void Test1()
    {
        var i1 = 123;
        var i2 = 321;

        var sum = i1 + i2;

        sum.Should().Be(444);
    }

    [Fact]
    public void Test2()
    {
        var cs = postgresFixture.Container.GetConnectionString();

    }
}
