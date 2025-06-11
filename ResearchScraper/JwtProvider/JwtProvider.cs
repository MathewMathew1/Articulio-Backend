using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ResearchScrapper.Api.Models;

namespace ResearchScrapper.Api.Service;

public class JwtService
{
    private readonly byte[] _key;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration config, ILogger<JwtService> logger)
    {
        var key = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt not configured");
        _key = Encoding.UTF8.GetBytes(key);
        _logger = logger;
    }

    public string CreateToken(User user)
    {
        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new
        {
            sub = user.Id,
            email = user.Email,
            name = user.Name ?? "",
            exp = DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds()
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);

        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        var unsignedToken = $"{headerBase64}.{payloadBase64}";
        var signature = Sign(unsignedToken);

        return $"{unsignedToken}.{signature}";
    }

    public UserIdentity? ParseToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        var id = principal.Id;
        var email = principal.Email;
        var name = principal.Name;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
            return null;

        return new UserIdentity(id, email, name ?? string.Empty);
    }

    public JwtPayloadPrincipal? ValidateToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            var unsignedToken = $"{header}.{payload}";
            var computedSignature = Sign(unsignedToken);

            if (!ConstantTimeEquals(signature, computedSignature))
                return null;

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var payloadData = JsonSerializer.Deserialize<JwtPayloadPrincipal>(payloadJson);

            if (payloadData == null || payloadData.exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return null;

            return payloadData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return null;
        }
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(_key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string padded = input
            .Replace('-', '+')
            .Replace('_', '/')
            .PadRight(input.Length + (4 - input.Length % 4) % 4, '=');

        return Convert.FromBase64String(padded);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        if (aBytes.Length != bBytes.Length)
            return false;

        int result = 0;
        for (int i = 0; i < aBytes.Length; i++)
            result |= aBytes[i] ^ bBytes[i];

        return result == 0;
    }
}
