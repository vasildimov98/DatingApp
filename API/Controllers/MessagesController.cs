using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper) : BaseApiController
{
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();

        if (username == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("Cannot send message to yourself");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (sender == null || recipient == null)
            return BadRequest("Cannot send message at the moment");

        var message = new Message
        {
            Sender = sender,
            SenderUsername = username,
            Recipient = recipient,
            RecipientUsername = createMessageDto.RecipientUsername,
            Content = createMessageDto.Content,
        };

        messageRepository.AddMessage(message);

        if (!await messageRepository.SaveAllAsync())
            return BadRequest("Failed to save messages");

        return Ok(mapper.Map<MessageDto>(message));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await messageRepository.GetMessagesForUser(messageParams);

        Response.AddPaginationHeader(messages);

        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();

        return Ok(await messageRepository.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();

        var message = await messageRepository.GetMessage(id);

        if (message == null) return BadRequest("Cannot delete this message");

        if (message.SenderUsername != username &&
        message.RecipientUsername != username)
            return Forbid();

        if (message.SenderUsername == username)
            message.HasSenderDeleted = true;
        if (message.RecipientUsername == username)
            message.HasRecipientDeleted = true;

        if (message is {HasSenderDeleted: true, HasRecipientDeleted: true})
            messageRepository.DeleteMessage(message);

        if (!await messageRepository.SaveAllAsync())
            return BadRequest("Problem deleting the message");
        
        return Ok();
    }
}
