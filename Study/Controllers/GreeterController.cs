using Auth;

namespace Study.Controllers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("grpc/[controller]")]
public class GreeterController
{
    private readonly Greeter.GreeterClient _client;

    public GreeterController(Greeter.GreeterClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Hello()
    {
        var reply = await _client.SayHelloAsync(new HelloRequest { Name = "leeka"});
        return reply.Message;
    }
}