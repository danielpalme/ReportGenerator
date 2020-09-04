using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class AsyncClass : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Properties.Add("hello", true);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
