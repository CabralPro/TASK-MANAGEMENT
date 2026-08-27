using System;

namespace TaskManagement.Domain.Entities;

public class User : Entity, IAggregateRoot
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    protected User() { }

    public User(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }

    public User(Guid id, string username, string email, string passwordHash) : base(id)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }
}
