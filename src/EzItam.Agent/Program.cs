using EzItam.Agent;
using EzItam.Agent.Collectors;
using EzItam.Agent.Transport;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(o => o.ServiceName = "EzItamAgent");
builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection("Agent"));
builder.Services.AddSingleton<AgentStateStore>();
builder.Services.AddHttpClient<AgentApiClient>();

if (OperatingSystem.IsWindows() && !builder.Configuration.GetValue<bool>("Agent:UseFakeCollector"))
    builder.Services.AddSingleton<IInventoryCollector, WindowsInventoryCollector>();
else
    builder.Services.AddSingleton<IInventoryCollector, FakeInventoryCollector>();

builder.Services.AddSingleton<AgentUpdater>();
builder.Services.AddHostedService<AgentWorker>();

var host = builder.Build();
host.Run();
