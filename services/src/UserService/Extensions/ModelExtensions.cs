using Microsoft.AspNetCore.Identity;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Extensions;

public static class ModelExtensions
{
    public static RoleReadDto ToDto(this IdentityRole role)
        => new(Guid.Parse(role.Id), role.Name!);

    public static UserReadDto ToDto(this ApplicationUser user)
        => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
}