using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Todo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    private async Task<WeatherForecast> WeatherForecastByIdAsync(int id)
    {
        try
        {
            WeatherForecast item = null;
            string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
    
            string selectCommand = "SELECT Date, Summary, TemperatureC FROM WeatherForecast WHERE id = @id";
    
            using SqlCommand command = new SqlCommand(selectCommand, connection);
            command.Parameters.AddWithValue("@id", id);
    
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                DateTime data = reader.GetDateTime(0);
                string summary = reader.GetString(1);
                int temperature = reader.GetInt32(2);
    
                item = new WeatherForecast { Date = DateOnly.FromDateTime(data), Summary = summary, TemperatureC = temperature };
            }
    
            return item;
        }
        catch (SqlException ex)
        {
            // Logar a exceção de forma segura
            // Logger.LogError(ex, "Erro ao acessar o banco de dados");
            throw new Exception("Erro ao buscar previsão do tempo por ID. Por favor, tente novamente mais tarde.");
        }
        catch (Exception ex)
        {
            // Logar a exceção de forma segura
            // Logger.LogError(ex, "Erro inesperado");
            throw new Exception("Erro inesperado. Por favor, tente novamente mais tarde.");
        }
    }
}
