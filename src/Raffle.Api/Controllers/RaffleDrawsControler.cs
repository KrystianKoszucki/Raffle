using Microsoft.AspNetCore.Mvc;
using Raffle.Api.Contracts;
using Raffle.Api.Services;

namespace Raffle.Api.Controllers
{
    [ApiController]
    [Route("RaffleDraws")]
    public class RaffleDrawsControler : ControllerBase
    {
        private readonly IRaffleService _raffleService;
        private readonly ILogger<RaffleDrawsControler> _logger;
        public RaffleDrawsControler(IRaffleService raffleService, ILogger<RaffleDrawsControler> logger)
        {
            _raffleService = raffleService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRaffleDrawRequest request)
        {

            _logger.LogInformation("Creating RaffleDraw with name {RaffleDrawName}", request.Name);

            await _raffleService.CreateRaffle(request.Name);

            _logger.LogInformation("RaffleDraw {RaffleDrawName} created successfully", request.Name);

            return NoContent();
        }

        [HttpPost("{raffleDrawName}/member")]
        public async Task<IActionResult> EnterSingleMember(string raffleDrawName, [FromBody] EnterRaffleDrawRequest request)
        {
            _logger.LogInformation("Adding member {Email} to RaffleDraw {RaffleDrawName}", request.Email, raffleDrawName);

            await _raffleService.EnterRaffle(raffleDrawName, request.Name, request.Email);

            return NoContent();
        }

        [HttpPost("{raffleDrawName}/close")]
        public async Task<IActionResult> Close(string raffleDrawName)
        {
            _logger.LogInformation("Closing RaffleDraw {RaffleDrawName}", raffleDrawName);

            var winner = await _raffleService.CloseRaffleDrawAndPickWinner(raffleDrawName);

            var response = new RaffleDrawWinnerResponse(
                winner.Id,
                winner.Name,
                winner.Email
                );

            _logger.LogInformation("RaffleDraw {RaffleDrawName} closed. Winner: {WinnerEmail}", raffleDrawName, winner.Email);

            return Ok(response);
        }

        [HttpGet("winners")]
        public async Task<IActionResult> Winners()
        {
            _logger.LogInformation("Fetching winners from all closed RaffleDraws");

            var winners = await _raffleService.GetPastWinners();

            return Ok(winners);
        }

        [HttpPost("{raffleDrawName}/members")]
        public async Task<IActionResult> EnterMembers(string raffleDrawName, [FromBody] List<EnterRaffleDrawRequest> raffleMembers)
        {
            _logger.LogInformation("Adding {Count} members to RaffleDraw {RaffleDrawName}", raffleMembers.Count, raffleDrawName);

            await _raffleService.AddRaffleMembers(raffleDrawName, raffleMembers);
            
            return NoContent();
        }
    }
}
