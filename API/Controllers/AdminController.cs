using API.Data.Repositories;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork) : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var usersWithRoles = await userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.Id,
                    Username = x.UserName,
                    Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
                }).ToListAsync();

            return Ok(usersWithRoles);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
                return this.BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null)
                return this.BadRequest("User not found");

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded)
                return this.BadRequest("Failed to add roles to user");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded)
                return this.BadRequest("Failed to remove from roles");

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForApproval()
        {
            return Ok(await unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync());
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);

            if (photo == null)
                return BadRequest("Photo could not be found");

            var user = await unitOfWork.UserRepository.GetUserByPhotoIdAsync(photoId);
            if (user == null)
                return BadRequest("Photo is not associated to any user");

            photo.IsApproved = true;

            if (!user.Photos.Any(x => x.IsMain))
                photo.IsMain = true;

            if (!await unitOfWork.Complete())
                return BadRequest("Photo could not be approved");

            return Ok(await unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync());
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);

            if (photo == null)
                return BadRequest("Photo could not be found");

            unitOfWork.PhotoRepository.RemovePhoto(photo);

            if (!await unitOfWork.Complete())
                return BadRequest("Photo could not be rejected");

            return Ok(await unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync());
        }
    }
}
