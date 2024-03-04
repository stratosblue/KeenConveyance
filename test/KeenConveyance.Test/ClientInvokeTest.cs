using KeenConveyance.Client;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

namespace KeenConveyance;

#region Not PrePreparePayloadData

[TestClass]
public class PathBaseClientInvokeMemoryPackTest : ClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
        options.ObjectSerializer = new MemoryPackObjectSerializer();
        options.PrePreparePayloadData = false;
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
        options.PrePreparePayloadData = false;
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
        options.PrePreparePayloadData = false;
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
        options.PrePreparePayloadData = false;
    }

    #endregion Protected 方法
}

#endregion Not PrePreparePayloadData

#region PrePreparePayloadData

[TestClass]
public class PathBaseClientInvokeMemoryPackPrePrepareTest : ClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
        options.ObjectSerializer = new MemoryPackObjectSerializer();
        options.PrePreparePayloadData = true;
    }

    #endregion Protected 方法
}

[TestClass]
public class PathBaseClientInvokePrePrepareTest : ClientInvokeTestBase<PathBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new PathBaseHttpRequestMessageConstructor();
        options.PrePreparePayloadData = true;
    }

    #endregion Protected 方法
}

[TestClass]
public class QueryBaseClientInvokeMemoryPackPrePrepareTest : ClientInvokeTestBase<QueryBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new QueryBaseHttpRequestMessageConstructor();
        options.ObjectSerializer = new MemoryPackObjectSerializer();
        options.PrePreparePayloadData = true;
    }

    #endregion Protected 方法
}

[TestClass]
public class QueryBaseClientInvokePrePrepareTest : ClientInvokeTestBase<QueryBaseTestStartup>
{
    #region Protected 方法

    protected override void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        base.ConfigureWithTestServer(options);
        options.HttpRequestMessageConstructor = new QueryBaseHttpRequestMessageConstructor();
        options.PrePreparePayloadData = true;
    }

    #endregion Protected 方法
}

#endregion PrePreparePayloadData
