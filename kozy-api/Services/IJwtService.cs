namespace kozy_api.Services;

public interface IJwtService
{
    string GenerateToken(string userId);
}