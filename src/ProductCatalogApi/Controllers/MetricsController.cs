using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi.Controllers;

[ApiController]
// Rule: This token dynamically converts 'MetricsController' into the URL path: 'api/metrics'
[Route("api/[controller]")] 
public class MetricsController : ControllerBase
{
    [HttpGet] // Binds this method to HTTP GET requests
    public IActionResult GetSystemMetrics()
    {
        // Define a structured, anonymous payload representing our microservice health
        var systemStats = new
        {
            CpuUsagePercentage = 14.5,
            MemoryAllocatedMb = 256,
            UptimeStatus = "Online",
            CurrentEngine = ".NET 10 Kestrel Server"
        };

        // Wrap the payload in a standard HTTP 200 OK container
        return Ok(systemStats);
    }
}