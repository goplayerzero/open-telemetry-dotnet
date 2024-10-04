using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Diagnostics;
using dotnet_simple.model;
using Microsoft.EntityFrameworkCore;

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
        private readonly ApplicationDbContext _context;

        // public HomeController(ApplicationDbContext context)
        // {
        //     _context = context;
        // }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers()
        {
            _logger.LogWarning("Get Players");
            var players = await _context.Players.FromSqlRaw("EXEC GetAllPlayers").ToListAsync();
            return Ok(players);
        }

        [HttpGet("player/{id}")]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            _logger.LogWarning("Get Player details"+id);
            // Call the stored procedure to get player details by ID
            var playerDetails = await _context.Players
                .FromSqlRaw("EXEC GetPlayerDetails @PlayerId = {0}", id)
                .ToListAsync();

            if (playerDetails == null || playerDetails.Count == 0)
            {
                _logger.LogError("Player not found "+id);
                return NotFound(); // Return 404 if no player found
            }

            return Ok(playerDetails);
        }
    }
}