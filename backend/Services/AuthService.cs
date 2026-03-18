using CineTrack.API.DTOs;
using CineTrack.API.Models;
using MongoDB.Driver;

namespace CineTrack.API.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly TokenService _tokenService;

    public AuthService(MongoDbService mongoDb, TokenService tokenService)
    {
        _users = mongoDb.GetCollection<User>("users");
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existing is not null)
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

        await _users.InsertOneAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponse(
            accessToken,
            user.RefreshToken,
            _tokenService.GetAccessTokenExpiry(),
            new UserDto(user.Id!, user.Name, user.Email, user.CreatedAt, user.AvatarUrl, user.Timezone)
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.Find(u => u.Email == request.Email).FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var update = Builders<User>.Update
            .Set(u => u.RefreshToken, refreshToken)
            .Set(u => u.RefreshTokenExpiry, _tokenService.GetRefreshTokenExpiry());

        await _users.UpdateOneAsync(u => u.Id == user.Id, update);

        return new AuthResponse(
            accessToken,
            refreshToken,
            _tokenService.GetAccessTokenExpiry(),
            new UserDto(user.Id!, user.Name, user.Email, user.CreatedAt, user.AvatarUrl, user.Timezone)
        );
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var user = await _users.Find(u =>
            u.RefreshToken == request.RefreshToken &&
            u.RefreshTokenExpiry > DateTime.UtcNow
        ).FirstOrDefaultAsync() ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var update = Builders<User>.Update
            .Set(u => u.RefreshToken, newRefreshToken)
            .Set(u => u.RefreshTokenExpiry, _tokenService.GetRefreshTokenExpiry());

        await _users.UpdateOneAsync(u => u.Id == user.Id, update);

        return new AuthResponse(
            accessToken,
            newRefreshToken,
            _tokenService.GetAccessTokenExpiry(),
            new UserDto(user.Id!, user.Name, user.Email, user.CreatedAt, user.AvatarUrl, user.Timezone)
        );
    }

    public async Task<UserDto> GetMeAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("User not found.");

        return new UserDto(user.Id!, user.Name, user.Email, user.CreatedAt, user.AvatarUrl, user.Timezone);
    }

    public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("User not found.");

        var updateDef = Builders<User>.Update
            .Set(u => u.Name, request.Name ?? user.Name)
            .Set(u => u.Email, request.Email ?? user.Email)
            .Set(u => u.Timezone, request.Timezone ?? user.Timezone)
            .Set(u => u.AvatarUrl, request.AvatarUrl ?? user.AvatarUrl);

        await _users.UpdateOneAsync(u => u.Id == userId, updateDef);

        var updated = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return new UserDto(updated.Id!, updated.Name, updated.Email, updated.CreatedAt, updated.AvatarUrl, updated.Timezone);
    }

    public async Task RevokeAsync(string userId)
    {
        var update = Builders<User>.Update
            .Set(u => u.RefreshToken, null)
            .Set(u => u.RefreshTokenExpiry, null);

        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }
}
