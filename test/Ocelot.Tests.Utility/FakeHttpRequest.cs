using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Ocelot.Tests.Utility
{
    public class FakeHttpRequest : HttpRequest
    {
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; } = null;
        public override string Method { get; set; } = HttpMethod.Get.Method;
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; } = false; // jjj
        public override HostString Host { get; set; } = new HostString(string.Empty);
        public override PathString PathBase { get; set; } = new PathString("/");
        public override PathString Path { get; set; } = new PathString("/");
        public override QueryString QueryString { get; set; } = new QueryString();
        public override IQueryCollection Query { get; set; } = new QueryCollection();
        public override string Protocol { get; set; } = "http";
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; } = new RequestCookieCollection();
        public override long? ContentLength { get; set; } = 0;
        public override string ContentType { get; set; } = "text/html";
        public override Stream Body { get; set; } = null;
        public override bool HasFormContentType { get; } = false;
        public override IFormCollection Form { get; set; } = null;
    }
}
