using _2PC.Coordinator.Services;
using Microsoft.AspNetCore.Mvc;

namespace _2PC.Coordinator.Controllers;

[ApiController]
public class WeatherForecastController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public WeatherForecastController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("/start")]
    public async Task<IActionResult> StartTransaction(List<int>? nodes)
    {
        var transactionId = await _transactionService.CreateTransactionAsync(nodes);
        await _transactionService.SendControlRequestAsync(transactionId);
        bool controlState = await _transactionService.CheckServicesControlStatusAsync(transactionId);

        if (!controlState) return BadRequest();

        await _transactionService.CommitAsync(transactionId);
        bool transactionState = await _transactionService.CheckServicesTransactionStatusAsync(transactionId);

        if (!transactionState)
        {
            await _transactionService.RollBackAsync(transactionId);
            return BadRequest();
        }

        return Ok();
    }
}

