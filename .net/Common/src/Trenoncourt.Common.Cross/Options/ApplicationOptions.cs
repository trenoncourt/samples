using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Trenoncourt.Common.Cross.Options
{
    public class ApplicationOptions
    {
        public WebHostOptions WebHost { get; set; }
        
        public KestrelServerOptions Kestrel { get; set; }
    }
}