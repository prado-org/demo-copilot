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

    private WeatherForecast WeatherForecastById(int id)
    {
        try
        {
            WeatherForecast item = null;
            using SqlConnection connection = new SqlConnection("Server=localhost;Database=Todo;User Id=sa;Password=Password123;");
            connection.Open();

            string selectCommand = "SELECT Date, Summary, TemperatureC FROM WeatherForecast WHERE id = @id";

            SqlCommand command = new SqlCommand(selectCommand, connection);
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
            _logger.LogError(ex, "A SQL error occurred while fetching the weather forecast by id.");
            throw new Exception("A database error occurred. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching the weather forecast by id.");
            throw;
        }
    }
}
