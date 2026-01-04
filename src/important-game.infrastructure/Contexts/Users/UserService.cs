using important_game.infrastructure.Contexts.Users.Data;
using important_game.infrastructure.Contexts.Users.Data.Entities;
using important_game.infrastructure.Contexts.Users.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace important_game.infrastructure.Contexts.Users;

public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public async Task<UserEntity?> GetOrCreateUserFromGoogleAsync(string googleId, string email, string? name, string? pictureUrl, CancellationToken cancellationToken = default)
    {
        // Try to get existing user
        var existingUser = await _userRepository.GetUserByGoogleIdAsync(googleId, cancellationToken);

        if (existingUser != null)
        {
            // Update last login
            await _userRepository.UpdateLastLoginAsync(existingUser.UserId, DateTime.UtcNow, cancellationToken);
            return existingUser;
        }

        // Create new user
        var newUser = new UserEntity
        {
            GoogleId = googleId,
            Email = email,
            Name = name,
            ProfilePictureUrl = pictureUrl,
            PreferredTimezone = "UTC",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateUserAsync(newUser, cancellationToken);
        newUser.UserId = userId;

        return newUser;
    }

    public async Task<UserEntity?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserByIdAsync(userId, cancellationToken);
    }

    public async Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateLastLoginAsync(userId, DateTime.UtcNow, cancellationToken);
    }

    public async Task UpdateUserPreferencesAsync(int userId, UserPreferencesDto preferences, CancellationToken cancellationToken = default)
    {
        await _userRepository.UpdateUserPreferencesAsync(
            userId,
            preferences.PreferredTimezone,
            preferences.Name,
            preferences.ProfilePictureUrl,
            cancellationToken);
    }

    // Favorite Matches
    public async Task<List<int>> GetUserFavoriteMatchIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var favorites = await _userRepository.GetUserFavoriteMatchesAsync(userId, cancellationToken);
        return favorites.Select(f => f.MatchId).ToList();
    }

    public async Task AddFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        await _userRepository.AddFavoriteMatchAsync(userId, matchId, cancellationToken);
    }

    public async Task RemoveFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        await _userRepository.RemoveFavoriteMatchAsync(userId, matchId, cancellationToken);
    }

    public async Task<bool> IsFavoriteMatchAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.IsFavoriteMatchAsync(userId, matchId, cancellationToken);
    }

    public async Task<int> GetFavoriteMatchCountAsync(int matchId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetFavoriteMatchCountAsync(matchId, cancellationToken);
    }

    public async Task<UserEntity?> GetUserByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserByGoogleIdAsync(googleId, cancellationToken);
    }

    // Favorite Teams
    public async Task<List<int>> GetUserFavoriteTeamIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserFavoriteTeamIdsAsync(userId, cancellationToken);
    }

    public async Task AddFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default)
    {
        await _userRepository.AddFavoriteTeamAsync(userId, teamId, cancellationToken);
    }

    public async Task RemoveFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default)
    {
        await _userRepository.RemoveFavoriteTeamAsync(userId, teamId, cancellationToken);
    }

    public async Task<bool> IsFavoriteTeamAsync(int userId, int teamId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.IsFavoriteTeamAsync(userId, teamId, cancellationToken);
    }

    public async Task DeleteUserAccountAsync(int userId, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteUserAsync(userId, cancellationToken);
    }

    public async Task<int?> GetUserId(ClaimsPrincipal user)
    {
        var googleId = user.FindFirst("GoogleId")?.Value;
        if (string.IsNullOrWhiteSpace(googleId))
            return null;

        var existingUser = await _userRepository.GetUserByGoogleIdAsync(googleId);
        if (existingUser == null)
            return null;

        return existingUser.UserId;
    }
}
