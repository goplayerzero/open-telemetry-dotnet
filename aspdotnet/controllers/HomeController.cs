using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Diagnostics;

namespace dotnet_simple.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly List<string> _products = new List<string>
        {
            "Laptop",
            "Mouse",
            "Keyboard"
        };
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("rolldice/{player?}")]
        public IActionResult RollDice(string? player)
        {
            var result = RollDice();

            if (string.IsNullOrEmpty(player))
            {
                _logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
            }
            else
            {
                _logger.LogInformation("{player} is rolling the dice: {result}", player, result);
            }

            var currentActivity = Activity.Current;
            if (currentActivity != null)
            {
                _logger.LogInformation("TraceId: {TraceId}, SpanId: {SpanId}", currentActivity.TraceId, currentActivity.SpanId);
                
                currentActivity.TraceStateString = $"rolldice = {result}";
            }

            return Ok(result.ToString(CultureInfo.InvariantCulture));
        }

        private int RollDice()
        {
            int dice = Random.Shared.Next(1, 7);
            if (dice == 4)
            {
                throw new Exception("Error");
            }
            return dice;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return _products;
        }

        [HttpGet("{id}")]
        public ActionResult<string> GetById(int id)
        {
            if (id < 0 || id >= _products.Count)
            {
                return NotFound();
            }

            return _products[id];
        }

        [HttpPost]
        public IActionResult Post([FromBody] string product)
        {
            _products.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = _products.Count - 1 }, product);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string product)
        {
            if (id < 0 || id >= _products.Count)
            {
                return NotFound();
            }

            _products[id] = product;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id < 0 || id >= _products.Count)
            {
                return NotFound();
            }

            _products.RemoveAt(id);
            return NoContent();
        }
    }
}