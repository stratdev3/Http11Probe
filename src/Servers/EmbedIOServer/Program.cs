using EmbedIO;
using EmbedIO.Actions;

var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 8080;
var url = $"http://*:{port}/";
var utf8NoBom = new System.Text.UTF8Encoding(false);

using var server = new WebServer(o => o
    .WithUrlPrefix(url)
    .WithMode(HttpListenerMode.EmbedIO))
    .WithModule(new ActionModule("/cookie", HttpVerbs.Any, async ctx =>
    {
        var sb = new System.Text.StringBuilder();
        foreach (System.Net.Cookie cookie in ctx.Request.Cookies)
            sb.AppendLine($"{cookie.Name}={cookie.Value}");
        await ctx.SendStringAsync(sb.ToString(), "text/plain", utf8NoBom);
    }))
    .WithModule(new ActionModule("/echo", HttpVerbs.Any, async ctx =>
    {
        var sb = new System.Text.StringBuilder();
        foreach (var key in ctx.Request.Headers.AllKeys)
            foreach (var val in ctx.Request.Headers.GetValues(key)!)
                sb.AppendLine($"{key}: {val}");
        await ctx.SendStringAsync(sb.ToString(), "text/plain", utf8NoBom);
    }))
    .WithModule(new ActionModule("/", HttpVerbs.Any, async ctx =>
    {
        ctx.Response.ContentType = "text/plain";
        if (ctx.Request.HttpVerb == HttpVerbs.Post)
        {
            using var ms = new System.IO.MemoryStream();
            await ctx.Request.InputStream.CopyToAsync(ms);
            var body = utf8NoBom.GetString(ms.ToArray());
            await ctx.SendStringAsync(body, "text/plain", utf8NoBom);
        }
        else
        {
            await ctx.SendStringAsync("OK", "text/plain", utf8NoBom);
        }
    }));

Console.WriteLine($"EmbedIO listening on http://localhost:{port}");
await server.RunAsync();
