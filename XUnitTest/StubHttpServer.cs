using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XUnitTest;

/// <summary>本地 HTTP 桩服务。以确定性响应替代真实厂商服务，验证各提供者的 URL 构造与响应解析</summary>
/// <remarks>
/// 不依赖外部网络与真实厂商密钥，可在全部 RID（含 CI）稳定运行。
/// 各提供者通过 <c>Server</c> 属性指向本服务地址注入。
/// </remarks>
public sealed class StubHttpServer : IDisposable
{
    private sealed record Route(String PathPrefix, String QueryContains, String Body);

    private readonly List<Route> _routes = [];
    private readonly CancellationTokenSource _cts = new();
    private readonly HttpListener _listener = null;

    /// <summary>服务端根地址。如 http://127.0.0.1:23456</summary>
    public String Server { get; } = "";

    /// <summary>最后收到请求的原始地址（含查询字符串）</summary>
    public String LastRequest { get; private set; }

    /// <summary>未匹配到路由的请求地址。测试断言其为空可确认桩路由完整命中</summary>
    public IList<String> Unmatched { get; } = [];

    /// <summary>实例化并启动桩服务。随机端口，最多重试 20 次</summary>
    public StubHttpServer()
    {
        for (var i = 0; i < 20 && _listener == null; i++)
        {
            var port = Random.Shared.Next(20000, 60000);
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            try
            {
                listener.Start();
                _listener = listener;
                Server = $"http://127.0.0.1:{port}";
            }
            catch
            {
                listener.Close();
            }
        }

        if (_listener == null) throw new InvalidOperationException("无法启动本地 HTTP 桩服务");

        _ = Task.Run(LoopAsync);
    }

    /// <summary>映射路由。按路径前缀匹配，queryContains 非空时还要求原始地址包含指定内容（用于区分同路径不同接口）</summary>
    /// <param name="pathPrefix">路径前缀。如 /geocoding/v3/</param>
    /// <param name="body">响应体。JSON 或 XML 原文</param>
    /// <param name="queryContains">查询字符串包含内容，可空</param>
    public void Map(String pathPrefix, String body, String queryContains = null) => _routes.Add(new(pathPrefix, queryContains, body));

    private async Task LoopAsync()
    {
        var listener = _listener;
        while (!_cts.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch
            {
                break;
            }

            try
            {
                var raw = ctx.Request.RawUrl ?? "";
                LastRequest = raw;

                var body = "{}";
                var found = false;
                foreach (var route in _routes)
                {
                    if (raw.StartsWith(route.PathPrefix) && (route.QueryContains == null || raw.Contains(route.QueryContains)))
                    {
                        body = route.Body;
                        found = true;
                        break;
                    }
                }
                if (!found) Unmatched.Add(raw);

                var buf = Encoding.UTF8.GetBytes(body);
                ctx.Response.StatusCode = found ? 200 : 404;
                ctx.Response.ContentType = "application/json; charset=utf-8";
                ctx.Response.ContentLength64 = buf.Length;
                await ctx.Response.OutputStream.WriteAsync(buf).ConfigureAwait(false);
                ctx.Response.Close();
            }
            catch
            {
                // 桩服务仅用于测试，响应阶段异常直接忽略
            }
        }
    }

    /// <summary>销毁并停止服务</summary>
    public void Dispose()
    {
        _cts.Cancel();
        _listener?.Close();
        _cts.Dispose();
    }
}
