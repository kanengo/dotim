using System.Net.Mime;
using ApiGate.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ApiGate.Controllers;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v1/[controller]")]
public class LoginController
{
    private readonly ILogger<LoginController> _logger;
    
    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiReply<LoginReply>>> Login(LoginRequest request)
    {
        var loginReply = new LoginReply();
        var reply = new ApiReply<LoginReply>(loginReply);
        
        return reply;
    }
}