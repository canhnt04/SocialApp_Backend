using System;

namespace SocialApp.ChatService.Application.Interfaces;

public interface IMessagePublisher
{
    void Publish(string routingKey, string message);
    void Subscribe(string routingKey, Func<string, bool> handler);
}
