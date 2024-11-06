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

    /// <summary>
    /// Retrieves a WeatherForecast by its ID from the database.
    /// </summary>
    /// <param name="id">The ID of the WeatherForecast to retrieve.</param>
    /// <returns>The WeatherForecast object if found; otherwise, null.</returns>
    /// <exception cref="Exception">Thrown when the database connection string is not found or an error occurs while retrieving the weather forecast.</exception>
    private WeatherForecast WeatherForecastById(int id)
    {
        try
        {
            WeatherForecast item = null;
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("A string de conexão com o banco de dados não foi encontrada.");
            }

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            string selectCommand = "SELECT Date, Summary, TemperatureC FROM WeatherForecast WHERE id = @id";

            using SqlCommand command = new SqlCommand(selectCommand, connection);
            command.Parameters.AddWithValue("@id", id);

            using SqlDataReader reader = command.ExecuteReader();
            
            if (reader.Read())
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
            _logger.LogError(ex, "An error occurred while retrieving the weather forecast.");
            throw new Exception("An error occurred while retrieving the weather forecast.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            throw;
        }
    }
}
