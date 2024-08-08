using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace API.Data.Repositories;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages
            .FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = context.Messages.AsQueryable();

        query = messageParams.Container switch
        {
            "Inbox" => query.Where(x => x.RecipientUsername == messageParams.Username && !x.HasRecipientDeleted),
            "Outbox" => query.Where(x => x.SenderUsername == messageParams.Username && !x.HasSenderDeleted),
            _ => query
                .Where(x => x.RecipientUsername == messageParams.Username
                 && x.MessageRead == null
                 && !x.HasRecipientDeleted),
        };

        var messageQuery = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        return await PagedList<MessageDto>
            .CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
    {
        var messages = await context.Messages
            .Include(x => x.Sender)
            .ThenInclude(x => x.Photos)
            .Include(x => x.Recipient)
            .ThenInclude(x => x.Photos)
            .Where(x =>
                x.RecipientUsername == currentUsername
                    && !x.HasRecipientDeleted
                    && x.SenderUsername == recipientUsername ||
                x.SenderUsername == currentUsername
                    && !x.HasSenderDeleted
                    && x.RecipientUsername == recipientUsername
            )
            .OrderBy(x => x.MessageSent)
            .ToListAsync();

        var unreadMessages = messages.Where(x => x.MessageRead == null && x.RecipientUsername == currentUsername).ToList();

        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(x => x.MessageRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
