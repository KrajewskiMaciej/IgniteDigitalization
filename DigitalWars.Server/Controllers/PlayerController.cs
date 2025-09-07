using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/player")]
    public class PlayerController : BaseApiController
    {
        private readonly IPlayerQueryService _queryService;
        private readonly IPlayerActionService _actionService;

        public PlayerController(IPlayerQueryService queryService, IPlayerActionService actionService)
        {
            _queryService = queryService;
            _actionService = actionService;
        }

        [HttpGet("deck/{deckId}/unified-cards")]
        public async Task<IActionResult> GetUnifiedCardsForDeck(int deckId, [FromQuery] int gameId, [FromQuery] int teamId)
        {
            try
            {
                var cards = await _queryService.GetCategorizedCardsForDeckAsync(deckId, gameId, teamId);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("team/{teamToken}")]
        public async Task<IActionResult> GetPlayerSessionData(string teamToken)
        {
            try
            {
                var sessionData = await _queryService.GetPlayerSessionDataAsync(teamToken);
                return Ok(sessionData);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("team-board")]
        public async Task<IActionResult> GetTeamBoardData([FromQuery] int gameId, [FromQuery] int teamId, [FromQuery] int boardId)
        {
            var pawns = await _queryService.GetProcessPawnsForBoardAsync(gameId, teamId, boardId);
            return Ok(pawns);
        }

        [HttpGet("rival-board")]
        public async Task<IActionResult> GetRivalBoardData([FromQuery] int gameId, [FromQuery] int boardId)
        {
            var pawns = await _queryService.GetRivalPawnsForBoardAsync(gameId, boardId);
            return Ok(pawns);
        }

        [HttpPost("success/{cardId}")]
        public async Task<IActionResult> SendSuccess(int cardId, [FromBody] CardDataDTO cardData)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("failure/{cardId}")]
        public async Task<IActionResult> SendFailure(int cardId, [FromBody] CardDataDTO cardData)
        {
            try
            {
                var result = await _actionService.PlayCardAsync(cardId, cardData, wasSuccess: false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("approve-log/{logId}")]
        public async Task<IActionResult> ApproveLog(int logId)
        {
            try
            {
                await _actionService.ApproveLogAsync(logId);
                return Ok(new { message = "Sugestia została zatwierdzona." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("reject-log/{logId}")]
        public async Task<IActionResult> RejectLog(int logId)
        {
            try
            {
                await _actionService.RejectLogAsync(logId);
                return Ok(new { message = "Sugestia została odrzucona." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}