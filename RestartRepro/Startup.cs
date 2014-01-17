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
            // This token represents the lifetime of the application. It 
            // should fire if there's an App pool recycle or app domain restart.

            // It doesn't seem to fire if there's a long running request and
            // the website is stopped in the IIS manager.
            CancellationToken token = GetShutdownToken(app.Properties);

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
