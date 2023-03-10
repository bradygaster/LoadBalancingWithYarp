int port = (args.Length == 1) ? Convert.ToInt16(args[0]) : 80;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel((ctx, kestrelOptions) => {
    kestrelOptions.ListenLocalhost(port);
});

builder.Services.AddSingleton<Server>(new Server(Environment.MachineName, Guid.NewGuid()));

var app = builder.Build();

app.MapGet("/servers", (Server server) => server);
app.MapGet("/servers/mine", (Server server) => server);

app.Run();

internal record Server(string MachineName, Guid Identifier) {
    public override string ToString() => $"{MachineName} ({Identifier})";
}
