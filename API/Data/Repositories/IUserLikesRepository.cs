using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Data.Repositories;

public interface IUserLikesRepository
{
    Task<UserLike?> GetUserLike(int sourceUserId, int targetUserId);

    Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);

    Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);

    void DeleteLike(UserLike like);

    void AddLike(UserLike like);
}
