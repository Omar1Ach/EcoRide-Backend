using EcoRide.Modules.Security.Application.Commands.AddFundsToWallet;
using EcoRide.Modules.Security.Application.Queries.GetWalletBalance;
using EcoRide.Modules.Security.Application.Queries.GetWalletTransactionHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoRide.Api.Controllers;

/// <summary>
/// API endpoints for wallet management
/// Implements US-008: Wallet Management
/// </summary>
[ApiController]
[Route("api/wallet")]
public class WalletController : ControllerBase
{
    private readonly ISender _sender;

    public WalletController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get user's wallet balance
    /// US-008: Display current balance
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetWalletBalanceQuery(userId);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Add funds to wallet (TC-070 to TC-073)
    /// US-008: Top-up wallet with validation (10-1000 MAD)
    /// </summary>
    [HttpPost("add-funds")]
    public async Task<IActionResult> AddFunds(
        [FromBody] AddFundsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddFundsToWalletCommand(
            request.UserId,
            request.Amount,
            request.PaymentMethodId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get wallet transaction history (TC-074)
    /// US-008: View all wallet top-ups
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWalletTransactionHistoryQuery(userId, pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error.Message, code = result.Error.Code });
        }

        return Ok(result.Value);
    }
}

/// <summary>
/// Request model for adding funds to wallet
/// </summary>
public sealed record AddFundsRequest(
    Guid UserId,
    decimal Amount,
    string PaymentMethodId);
