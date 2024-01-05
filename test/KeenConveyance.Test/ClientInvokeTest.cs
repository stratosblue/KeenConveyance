using KeenConveyance.Client;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

namespace KeenConveyance;

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

[TestClass]
public class QueryBaseClientInvokeMemoryPackTest : ClientInvokeTestBase<QueryBaseTestStartup>
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
public class PathBaseClientInvokeMemoryPackTest : ClientInvokeTestBase<PathBaseTestStartup>
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
