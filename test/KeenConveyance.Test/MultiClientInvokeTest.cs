using KeenConveyance.TestWebAPI;

namespace KeenConveyance;

[TestClass]
public class QueryBaseMultiClientInvokeTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
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
public class PathBaseMultiClientInvokeTest : MultiClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer<T>(IKeenConveyanceHttpClientBuilder<T> httpClientBuilder)
    {
        base.ConfigureWithTestServer(httpClientBuilder);
        httpClientBuilder.ConfigurePathBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}
