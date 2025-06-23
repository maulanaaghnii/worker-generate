using Microsoft.Data.SqlClient;
using System;

namespace AlertGenerator;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _connectionString;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private async Task GenerateAlert()
    {
        try
        {
            string alertId = Guid.NewGuid().ToString();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GenerateAlert", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@alert_id", alertId);

                    await command.ExecuteNonQueryAsync();
                    _logger.LogInformation("Alert berhasil dibuat dengan ID: {AlertId}", alertId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saat membuat alert");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GenerateAlert();
            await Task.Delay(2000, stoppingToken);
        }
    }
}
