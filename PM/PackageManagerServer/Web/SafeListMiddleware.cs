using System.Net;

namespace PackageManagerServer.Web
{
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        private readonly List<string> _safelist;

        public AdminSafeListMiddleware(
            RequestDelegate next,
            ILogger<AdminSafeListMiddleware> logger,
            List<string> safelist)
        {
            _safelist = safelist;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (true)//context.Request.Method != HttpMethod.Get.Method)
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogInformation("Request from Remote IP address: {RemoteIp}", remoteIp);
                var badIp = true;
                foreach (var address in _safelist)
                {
                    if (address == remoteIp?.ToString())
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
                {
                    _logger.LogWarning(
                        "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
