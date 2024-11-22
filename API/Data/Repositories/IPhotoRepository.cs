using API.DTOs;
using API.Entities;

namespace API.Data.Repositories;

public interface IPhotoRepository
{
    Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync();

    Task<Photo?> GetPhotoByIdAsync(int id);

    void RemovePhoto(Photo photo);
}
