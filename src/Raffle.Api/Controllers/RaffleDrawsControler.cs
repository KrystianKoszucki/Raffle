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
        public RaffleDrawsControler(IRaffleService raffleService)
        {
            _raffleService = raffleService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRaffleDrawRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name is required");
            try
            {
                await _raffleService.CreateRaffle(request.Name);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("{raffleDrawName}/member")]
        public async Task<IActionResult> EnterSingleMember(string raffleDrawName, [FromBody] EnterRaffleDrawRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Name is required");

            try
            {
                await _raffleService.EnterRaffle(raffleDrawName, request.Name, request.Email);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost("{raffleDrawName}/close")]
        public async Task<IActionResult> Close(string raffleDrawName)
        {
            try
            {
                var winner = await _raffleService.CloseRaffleDrawAndPickWinner(raffleDrawName);

                var response = new RaffleDrawWinnerResponse(
                    winner.Id,
                    winner.Name,
                    winner.Email
                    );

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("winners")]
        public async Task<IActionResult> Winners()
        {
            try
            {
                var winners = await _raffleService.GetPastWinners();
                return Ok(winners);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("{raffleDrawName}/members")]
        public async Task<IActionResult> EnterMembers(string raffleDrawName, [FromBody] List<EnterRaffleDrawRequest> raffleMembers)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (raffleMembers.Count == 0)
                return BadRequest("No members to add");

            try
            {
                await _raffleService.AddRaffleMembers(raffleDrawName, raffleMembers);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
