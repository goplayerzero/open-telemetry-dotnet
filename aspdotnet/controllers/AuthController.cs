using System.Diagnostics;
using dotnet_simple.model;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_simple.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth auth)
        {
            if (auth == null)
            {
                _logger.LogWarning("Authentication request is null.");
                return BadRequest("Invalid client request.");
            }

            var user = new User();

            if (auth.UserName == "foo" && auth.Password == "bar")
            {
                user.UserId = 123456;
                user.Email = "foo@bar.com";
                user.Name = "Foo Bar";

                _logger.LogInformation("User logged in: {UserId}", user.UserId);
            }
            else
            {
                _logger.LogWarning("Invalid username or password for user: {UserName}", auth.UserName);
                return Unauthorized();
            }

            var currentActivity = Activity.Current;
            
            if (currentActivity != null)
            {
                // Add TraceStateString if user is valid
                currentActivity.TraceStateString = $"userid={user.UserId}";

                // Log TraceId and SpanId
                _logger.LogInformation("TraceId: {TraceId}, SpanId: {SpanId}", currentActivity.TraceId, currentActivity.SpanId);

                // Add tags and baggage for distributed tracing
                currentActivity.AddTag("myTag", "myValue");
                currentActivity.AddBaggage("myBaggage", "myBaggageValue");
            }

            return Ok(user);
        }
    }
}
