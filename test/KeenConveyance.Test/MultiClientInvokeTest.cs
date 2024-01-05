using KeenConveyance.Client;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

namespace KeenConveyance;

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

[TestClass]
public class QueryBaseMultiClientInvokeMemoryPackTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new QueryBaseHttpRequestMessageConstructor();
        options.ObjectSerializer = new MemoryPackObjectSerializer();
    }

    #endregion Protected 方法
}

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
public class PathBaseMultiClientInvokeMemoryPackTest : MultiClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
        options.ObjectSerializer = new MemoryPackObjectSerializer();
    }

    #endregion Protected 方法
}
