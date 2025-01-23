using Microsoft.AspNetCore.Mvc;
using Back.Services.Interfaces;

namespace Back.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletTestController : ControllerBase
{
    private readonly IDatabaseService _databaseService;

    public WalletTestController(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoney([FromBody] AddMoneyRequest request)
    {
        try
        {
            // First check current balance
            var checkParams = new Dictionary<string, object>
            {
                { "@Username", request.Username }
            };

            const string checkQuery = @"
                SELECT w.amount 
                FROM api_schema.wallet w
                JOIN api_schema.user u ON w.user_id = u.id
                WHERE u.username = @Username";

            using var checkReader = await _databaseService.ExecuteQueryAsync(checkQuery, checkParams);
            if (!checkReader.Read())
            {
                return NotFound(new { success = false, message = "User wallet not found" });
            }

            var currentBalance = checkReader.GetInt32(0);

            // Then update balance
            var updateParams = new Dictionary<string, object>
            {
                { "@Username", request.Username },
                { "@Amount", request.Amount }
            };

            const string updateQuery = @"
                UPDATE api_schema.wallet w
                SET amount = amount + @Amount
                FROM api_schema.user u
                WHERE w.user_id = u.id AND u.username = @Username
                RETURNING w.amount as new_balance";

            using var updateReader = await _databaseService.ExecuteQueryAsync(updateQuery, updateParams);
            
            if (updateReader.Read())
            {
                var newBalance = updateReader.GetInt32(0);
                return Ok(new { 
                    success = true, 
                    oldBalance = currentBalance,
                    addedAmount = request.Amount,
                    newBalance = newBalance 
                });
            }
            
            return BadRequest(new { success = false, message = "Failed to update balance" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] string username)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@Username", username }
            };

            const string query = @"
                SELECT w.amount 
                FROM api_schema.wallet w
                JOIN api_schema.user u ON w.user_id = u.id
                WHERE u.username = @Username";

            using var reader = await _databaseService.ExecuteQueryAsync(query, parameters);
            
            if (reader.Read())
            {
                var balance = reader.GetInt32(0);
                return Ok(new { success = true, balance });
            }
            
            return NotFound(new { success = false, message = "User wallet not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class AddMoneyRequest
{
    public string Username { get; set; }
    public int Amount { get; set; }
}
