using KeenConveyance.Client;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

namespace KeenConveyance.Test;

#region Not PrePreparePayloadData

[TestClass]
public class PathBaseMultiClientInvokeMemoryPackTest : MultiClientInvokeTestBase<PathBaseTestStartup>
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
public class PathBaseMultiClientInvokeTest : MultiClientInvokeTestBase<PathBaseTestStartup>
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
public class QueryBaseMultiClientInvokeMemoryPackTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
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
public class QueryBaseMultiClientInvokeTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
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
public class PathBaseMultiClientInvokeMemoryPackPrePrepareTest : MultiClientInvokeTestBase<PathBaseTestStartup>
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
public class PathBaseMultiClientInvokePrePrepareTest : MultiClientInvokeTestBase<PathBaseTestStartup>
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
public class QueryBaseMultiClientInvokeMemoryPackPrePrepareTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
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
public class QueryBaseMultiClientInvokePrePrepareTest : MultiClientInvokeTestBase<QueryBaseTestStartup>
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
