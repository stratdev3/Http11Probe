using System.Text;
using Http11Probe.Client;
using Http11Probe.Response;

namespace Http11Probe.TestCases.Suites;

public static class WebSocketsSuite
{
    public static IEnumerable<TestCase> GetTestCases()
    {
        yield return new TestCase
        {
            Id = "WS-UPGRADE-POST",
            Description = "WebSocket upgrade via POST must not be accepted",
            Category = TestCategory.WebSockets,
            RfcReference = "RFC 6455 §4.1",
            PayloadFactory = ctx => MakeRequest(
                $"POST / HTTP/1.1\r\nHost: {ctx.HostHeader}\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\nSec-WebSocket-Version: 13\r\n\r\n"),
            Expected = new ExpectedBehavior
            {
                Description = "!101",
                CustomValidator = (response, state) =>
                {
                    if (response is null)
                        return state == ConnectionState.ClosedByServer ? TestVerdict.Pass : TestVerdict.Fail;
                    return response.StatusCode == 101 ? TestVerdict.Fail : TestVerdict.Pass;
                }
            }
        };

        yield return new TestCase
        {
            Id = "WS-UPGRADE-MISSING-CONN",
            Description = "Upgrade header without Connection: Upgrade must not trigger protocol switch",
            Category = TestCategory.WebSockets,
            RfcReference = "RFC 9110 §7.8",
            PayloadFactory = ctx => MakeRequest(
                $"GET / HTTP/1.1\r\nHost: {ctx.HostHeader}\r\nUpgrade: websocket\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\nSec-WebSocket-Version: 13\r\n\r\n"),
            Expected = new ExpectedBehavior
            {
                Description = "!101",
                CustomValidator = (response, state) =>
                {
                    if (response is null)
                        return state == ConnectionState.ClosedByServer ? TestVerdict.Pass : TestVerdict.Fail;
                    return response.StatusCode == 101 ? TestVerdict.Fail : TestVerdict.Pass;
                }
            }
        };

        yield return new TestCase
        {
            Id = "WS-UPGRADE-UNKNOWN",
            Description = "Upgrade to unknown protocol must not return 101",
            Category = TestCategory.WebSockets,
            RfcReference = "RFC 9110 §7.8",
            PayloadFactory = ctx => MakeRequest(
                $"GET / HTTP/1.1\r\nHost: {ctx.HostHeader}\r\nConnection: Upgrade\r\nUpgrade: totally-made-up/1.0\r\n\r\n"),
            Expected = new ExpectedBehavior
            {
                Description = "!101",
                CustomValidator = (response, state) =>
                {
                    if (response is null)
                        return state == ConnectionState.ClosedByServer ? TestVerdict.Pass : TestVerdict.Fail;
                    return response.StatusCode == 101 ? TestVerdict.Fail : TestVerdict.Pass;
                }
            }
        };

        yield return new TestCase
        {
            Id = "WS-UPGRADE-INVALID-VER",
            Description = "WebSocket upgrade with unsupported version — should return 426",
            Category = TestCategory.WebSockets,
            RfcReference = "RFC 6455 §4.4",
            PayloadFactory = ctx => MakeRequest(
                $"GET / HTTP/1.1\r\nHost: {ctx.HostHeader}\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\nSec-WebSocket-Version: 99\r\n\r\n"),
            Expected = new ExpectedBehavior
            {
                Description = "non-101 (426 preferred)",
                CustomValidator = (response, state) =>
                {
                    if (response is null)
                        return state == ConnectionState.ClosedByServer ? TestVerdict.Pass : TestVerdict.Fail;
                    if (response.StatusCode == 101)
                        return TestVerdict.Fail;
                    if (response.StatusCode == 426)
                        return TestVerdict.Pass;
                    // Some servers ignore Upgrade entirely and process the GET.
                    if (response.StatusCode is >= 200 and < 300)
                        return TestVerdict.Warn;
                    return TestVerdict.Pass;
                }
            }
        };

        yield return new TestCase
        {
            Id = "WS-UPGRADE-HTTP10",
            Description = "Upgrade header in HTTP/1.0 request must be ignored",
            Category = TestCategory.WebSockets,
            RfcReference = "RFC 9110 §7.8",
            PayloadFactory = ctx => MakeRequest(
                $"GET / HTTP/1.0\r\nHost: {ctx.HostHeader}\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\nSec-WebSocket-Version: 13\r\n\r\n"),
            Expected = new ExpectedBehavior
            {
                Description = "!101",
                CustomValidator = (response, state) =>
                {
                    if (response is null)
                        return state == ConnectionState.ClosedByServer ? TestVerdict.Pass : TestVerdict.Fail;
                    if (response.StatusCode == 101)
                        return TestVerdict.Fail;
                    return TestVerdict.Pass;
                }
            }
        };
    }

    private static byte[] MakeRequest(string request) => Encoding.ASCII.GetBytes(request);
}
