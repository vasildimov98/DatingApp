using API.DTOs;
using API.Entities;
using API.Helpers;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories;

public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
{
    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
        return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
        return await context.Groups
        .Include(x => x.Connections)
        .Where(x => x.Connections.Any(x => x.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
    {
        return await context.Messages
            .FindAsync(id);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
        return await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
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
        var messages = context.Messages
            .Where(x =>
                x.RecipientUsername == currentUsername
                    && !x.HasRecipientDeleted
                    && x.SenderUsername == recipientUsername ||
                x.SenderUsername == currentUsername
                    && !x.HasSenderDeleted
                    && x.RecipientUsername == recipientUsername
            )
            .OrderBy(x => x.MessageSent)
            .Select(x => new MessageDto
            {
                Id = x.Id,
                SenderId = x.SenderId,
                SenderUsername = x.Sender.UserName!,
                SenderPhotoUrl = x.Sender.Photos.FirstOrDefault(x => x.IsMain)!.Url,
                RecipientId = x.RecipientId,
                RecipientUsername = x.Recipient.UserName!,
                RecipientPhotoUrl = x.Recipient.Photos.FirstOrDefault(x => x.IsMain)!.Url,
                Content = x.Content,
                MessageSent = x.MessageSent,
                MessageRead = x.MessageRead,
            })
            .ToList();

        var unreadMessages = messages.Where(x => x.MessageRead == null && x.RecipientUsername == currentUsername).ToList();

        if (unreadMessages.Count != 0)
        {
            unreadMessages.ForEach(x => x.MessageRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        return messages;
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
