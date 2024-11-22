namespace API.Data.Repositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }

    IUserLikesRepository LikesRepository { get; }

    IMessageRepository MessageRepository { get; }

    IPhotoRepository PhotoRepository { get; }

    Task<bool> Complete();

    bool HasChanges();
}
