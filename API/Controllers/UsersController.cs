using System.Security.Claims;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();

        return this.Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);

        if (user == null) return this.NotFound();

        return user;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await userRepository.GetUserByIdAsync(id);

        if (user == null) return this.NotFound();

        return user;
    }

    [HttpPut]
    public async Task<ActionResult> Update(MemberUpdateDto memberUpdateDto)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return this.BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);

        if (await userRepository.SaveAllAsync()) return NoContent();

        return this.BadRequest("Fail to update the user!");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return this.BadRequest("Cannot update user!");

        var result = await photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {

            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };

        user.Photos.Add(photo);

        if (!await userRepository.SaveAllAsync()) return this.BadRequest("Problem adding photo!");

        return CreatedAtAction(
        nameof(GetUser),
        new
        {
            username = user.UserName
        },
        mapper.Map<PhotoDto>(photo));
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Cannot use this photo as main");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        if (currentMain != null)
        {
            currentMain.IsMain = false;
            photo.IsMain = true;
        }

        if (!await userRepository.SaveAllAsync()) return this.BadRequest("Problem setting main photo");

        return this.NoContent();
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("User cannot be null");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);


        if (photo == null || photo.IsMain) return this.BadRequest("This photo cannot be deleted");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null)
                return this.BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (!await userRepository.SaveAllAsync())
            return this.BadRequest("Problem occur when deleting a photo");

        return this.Ok();
    }
}
