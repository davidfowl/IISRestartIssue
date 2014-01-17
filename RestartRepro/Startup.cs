using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(RestartRepro.Startup))]

namespace RestartRepro
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var token = GetShutdownToken(app.Properties);

            token.Register(OnShutdown);

            app.Map("/long", map =>
            {
                map.Run(async context =>
                {
                    context.Response.ContentType = "text/plain";

                    var tcs = new TaskCompletionSource<object>();

                    token.Register(() => tcs.TrySetResult(null));

                    await tcs.Task;
                    await context.Response.WriteAsync("Request completed");
                });
            });
        }

        private void OnShutdown()
        {
            Debugger.Launch();
        }

        internal static CancellationToken GetShutdownToken(IDictionary<string, object> env)
        {
            object value;
            return env.TryGetValue("host.OnAppDisposing", out value)
                && value is CancellationToken
                ? (CancellationToken)value
                : default(CancellationToken);
        }
    }
}
