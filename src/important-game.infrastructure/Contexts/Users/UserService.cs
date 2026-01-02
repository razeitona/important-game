using important_game.infrastructure.Contexts.Users.Data;
using important_game.infrastructure.Contexts.Users.Data.Entities;
using important_game.infrastructure.Contexts.Users.Models;

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

    // Match Votes
    public async Task<MatchVoteDto?> GetUserVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        var vote = await _userRepository.GetUserVoteAsync(userId, matchId, cancellationToken);
        if (vote == null) return null;

        return new MatchVoteDto
        {
            UserId = vote.UserId,
            MatchId = vote.MatchId,
            VoteType = vote.VoteType,
            VotedAt = vote.VotedAt
        };
    }

    public async Task ToggleVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        var existingVote = await _userRepository.GetUserVoteAsync(userId, matchId, cancellationToken);

        if (existingVote != null)
        {
            // Remove vote (toggle off)
            await _userRepository.RemoveVoteAsync(userId, matchId, cancellationToken);
        }
        else
        {
            // Add vote (toggle on)
            await _userRepository.AddOrUpdateVoteAsync(userId, matchId, 1, cancellationToken);
        }
    }

    public async Task<MatchVoteStatsDto> GetMatchVoteStatsAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var totalVotes = await _userRepository.GetMatchVoteCountAsync(matchId, cancellationToken);

        return new MatchVoteStatsDto
        {
            MatchId = matchId,
            TotalVotes = totalVotes
        };
    }

    public async Task<Dictionary<int, bool>> GetUserVotesForMatchesAsync(int userId, List<int> matchIds, CancellationToken cancellationToken = default)
    {
        var votes = await _userRepository.GetUserVotesForMatchesAsync(userId, matchIds, cancellationToken);
        return votes.ToDictionary(kvp => kvp.Key, kvp => true);
    }

    public async Task AddOrUpdateVoteAsync(int userId, int matchId, int voteType, CancellationToken cancellationToken = default)
    {
        await _userRepository.AddOrUpdateVoteAsync(userId, matchId, voteType, cancellationToken);
    }

    public async Task RemoveVoteAsync(int userId, int matchId, CancellationToken cancellationToken = default)
    {
        await _userRepository.RemoveVoteAsync(userId, matchId, cancellationToken);
    }

    public async Task<int> GetMatchVoteCountAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var totalVotes = await _userRepository.GetMatchVoteCountAsync(matchId, cancellationToken);
        return totalVotes;
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
}
