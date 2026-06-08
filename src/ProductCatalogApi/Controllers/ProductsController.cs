using Microsoft.AspNetCore.Mvc;

namespace ProductCatalogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllProducts()
    {
        var items = new[] { "It's me this time around", "Cloud Storage Engine", "Automated Test Runner", "GitOps Synchronizer" };
        return Ok(items);
    }
}
