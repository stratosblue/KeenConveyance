using KeenConveyance.TestWebAPI;

TestStartup.NoTestServer = true;
var webApplication = new QueryBaseTestStartup().Build(args);
webApplication.Run();
