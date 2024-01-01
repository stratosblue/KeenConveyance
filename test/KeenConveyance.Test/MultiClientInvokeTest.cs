using KeenConveyance.TestWebAPI;

namespace KeenConveyance;

[TestClass]
public class QueryBaseMultiClientInvokeTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new QueryBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}

[TestClass]
public class PathBaseMultiClientInvokeTest : MultiClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}
