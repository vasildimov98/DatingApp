namespace API.Data.Repositories;

public class UnitOfWork(DataContext context, IUserRepository userRepository, IUserLikesRepository likesRepository, IMessageRepository messageRepository, IPhotoRepository photoRepository) : IUnitOfWork
{
    public IUserRepository UserRepository => userRepository;

    public IUserLikesRepository LikesRepository => likesRepository;

    public IMessageRepository MessageRepository => messageRepository;
    
    public IPhotoRepository PhotoRepository => photoRepository;

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
