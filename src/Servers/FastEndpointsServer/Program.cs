using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://+:8080");
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(GetRoot).Assembly]);

var app = builder.Build();

app.UseFastEndpoints();

app.Run();

// ── GET / ──────────────────────────────────────────────────────

sealed class GetRoot : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await HttpContext.Response.WriteAsync("OK", ct);
    }
}

// ── POST / ─────────────────────────────────────────────────────

sealed class PostRoot : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        await HttpContext.Response.WriteAsync(body, ct);
    }
}

// ── GET/POST /cookie ──────────────────────────────────────────

sealed class CookieEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Verbs("GET", "POST");
        Routes("/cookie");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var cookie in HttpContext.Request.Cookies)
            sb.AppendLine($"{cookie.Key}={cookie.Value}");
        await HttpContext.Response.WriteAsync(sb.ToString(), ct);
    }
}

// ── GET/POST /echo ────────────────────────────────────────────

sealed class EchoEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Verbs("GET", "POST");
        Routes("/echo");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var h in HttpContext.Request.Headers)
            foreach (var v in h.Value)
                sb.AppendLine($"{h.Key}: {v}");
        await HttpContext.Response.WriteAsync(sb.ToString(), ct);
    }
}
