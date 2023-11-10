using KeenConveyance.TestWebAPI;

namespace KeenConveyance;

[TestClass]
public class QueryBaseClientInvokeTest : ClientInvokeTestBase<QueryBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer<T>(IKeenConveyanceHttpClientBuilder<T> httpClientBuilder)
    {
        base.ConfigureWithTestServer(httpClientBuilder);
        httpClientBuilder.ConfigureQueryBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}

[TestClass]
public class PathBaseClientInvokeTest : ClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer<T>(IKeenConveyanceHttpClientBuilder<T> httpClientBuilder)
    {
        base.ConfigureWithTestServer(httpClientBuilder);
        httpClientBuilder.ConfigurePathBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}
