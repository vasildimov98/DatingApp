using System;
using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();

        var currentUser = Context.User?.GetUsername();
        var otherUser = httpContext?.Request.Query["user"];


        if (currentUser == null || string.IsNullOrWhiteSpace(otherUser))
        {
            throw new HubException("Cannot join group");
        }

        var groupName = GenerateGroupName(currentUser, otherUser!);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await unitOfWork.MessageRepository.GetMessageThread(currentUser, otherUser!);

        if (unitOfWork.HasChanges()) await unitOfWork.Complete();

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get user");

        if (username.Equals(createMessageDto.RecipientUsername, StringComparison.CurrentCultureIgnoreCase))
            throw new HubException("Cannot send message to yourself");

        var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null)
            throw new HubException("Cannot send message at the moment");

        var message = new Message
        {
            Sender = sender,
            SenderUsername = username,
            Recipient = recipient,
            RecipientUsername = createMessageDto.RecipientUsername,
            Content = createMessageDto.Content,
        };

        var groupName = GenerateGroupName(sender.UserName, recipient.UserName);
        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.MessageRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName!);

            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients
                    .Clients(connections)
                    .SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        unitOfWork.MessageRepository.AddMessage(message);

        if (await unitOfWork.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get user");

        var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);

        var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };

        if (group == null)
        {
            group = new Group { Name = groupName };
            unitOfWork.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (!await unitOfWork.Complete())
        {
            throw new HubException("Failed to join group");
        }

        return group;
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        if (connection != null)
        {
            unitOfWork.MessageRepository.RemoveConnection(connection);
        }

        if (!await unitOfWork.Complete() || group == null)
        {
            throw new HubException("Failed to remove group");
        }

        return group;
    }

    private static string GenerateGroupName(string? caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;

        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}
