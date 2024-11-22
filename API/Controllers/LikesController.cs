﻿using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId)
            return this.BadRequest("You cannot like yourself!");

        var existingLike = await unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);

        if (existingLike == null)
        {
            var userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId,
            };

            unitOfWork.LikesRepository.AddLike(userLike);
        }
        else
        {
            unitOfWork.LikesRepository.DeleteLike(existingLike);
        }

        if (await unitOfWork.Complete()) return Ok();

        return this.BadRequest("Fail to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        return Ok(await unitOfWork.LikesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await unitOfWork.LikesRepository.GetUserLikes(likesParams);


        Response.AddPaginationHeader(users);

        return this.Ok(users);
    }
}
