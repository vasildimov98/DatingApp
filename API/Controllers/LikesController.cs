using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUserLikesRepository userLikesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId)
            return this.BadRequest("You cannot like yourself!");

        var existingLike = await userLikesRepository.GetUserLike(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId,
            };

            userLikesRepository.AddLike(userLike);
        }
        else
        {
            userLikesRepository.DeleteLike(existingLike);
        }

        if (await userLikesRepository.SaveChanges()) return Ok();

        return this.BadRequest("Fail to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await userLikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users  = await userLikesRepository.GetUserLikes(likesParams);


        Response.AddPaginationHeader(users);

        return this.Ok(users);
    }
}
