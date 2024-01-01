using KeenConveyance.TestWebAPI;

namespace KeenConveyance;

[TestClass]
public class QueryBaseClientInvokeTest : ClientInvokeTestBase<QueryBaseTestStartup>
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
public class PathBaseClientInvokeTest : ClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
    }

    #endregion Protected 方法
}
