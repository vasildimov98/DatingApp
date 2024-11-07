using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Data.Repositories;

public interface IMessageRepository
{
    void AddMessage(Message message);

    void AddGroup(Group group);

    void RemoveConnection(Connection connection);

    Task<Connection?> GetConnection(string connectionId);

    Task<Group?> GetMessageGroup(string groupName); 

    Task<Group?> GetGroupForConnection(string connectionId);

    void DeleteMessage(Message message);

    Task<Message?> GetMessage(int id);

    Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);

    Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername);
}
