using System.Net;
using SimpleW;
using SimpleW.Observability;

// logs
Log.SetSink(Log.ConsoleWriteLine, LogLevel.Trace);

var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 8080;
var server = new SimpleWServer(IPAddress.Any, port);

// default
server.MapGet("/", () => "OK");
server.Map("HEAD", "/", () => "OK");
server.Map("OPTIONS", "/", (HttpSession session) => {
    return session.Response.AddHeader("Allow", "GET, HEAD, POST, OPTIONS")
                           .Text("OK");
});

// post
server.MapPost("/", (HttpSession session) => session.Response.Text(session.Request.BodyString));

// echo
server.MapGet("/echo", (HttpSession session) => Echo(session));
server.MapPost("/echo", (HttpSession session) => Echo(session));

// cookie
server.MapGet("/cookie", (HttpSession session) => ParseCookies(session));
server.MapPost("/cookie", (HttpSession session) => ParseCookies(session));

static HttpResponse Echo(HttpSession session) {
    var sb = new System.Text.StringBuilder();
    foreach (var h in session.Request.Headers.EnumerateAll()) {
        sb.AppendLine($"{h.Key}: {h.Value}");
    }
    return session.Response.Text(sb.ToString());
}

static HttpResponse ParseCookies(HttpSession session) {
    var sb = new System.Text.StringBuilder();
    foreach (var pair in session.Request.Headers.EnumerateCookies()) {
        sb.AppendLine($"{pair.Key}={pair.Value}");
    }
    return session.Response.Text(sb.ToString());
}

await server.RunAsync();
