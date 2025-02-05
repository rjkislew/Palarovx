public class APIService
{
    private readonly IConfiguration _configuration;

    public APIService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ApiUrl => _configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl is not configured.");

    public string Palaro2026API => $"{ApiUrl}api";
}