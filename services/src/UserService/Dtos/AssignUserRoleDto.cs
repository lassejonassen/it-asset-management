namespace UserService.Dtos;

public record AssignUserRoleDto(Guid UserId, ICollection<Guid> RoleIds);