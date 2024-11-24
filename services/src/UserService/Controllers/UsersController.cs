using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Dtos;
using UserService.Extensions;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();
        var result = users.Select(x => x.ToDto());

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        // Getting the user from the database.
        var user = await userManager.FindByIdAsync(id.ToString());

        // Checking if the user exists.
        if (user == null)
            return NotFound();

        // Mapping the user to the ReadDto object.

        return Ok(user.ToDto());
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid id)
    {
        // Getting the user from the database.
        var user = await userManager.FindByIdAsync(id.ToString());

        // Checking if the user exists.
        if (user == null)
            return NotFound();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewUser(UserCreateDto req)
    {
        // Checking the model state.
        if (ModelState.IsValid)
        {
            // Checking if the user exists.
            var existingUser = await userManager.FindByEmailAsync(req.Email);
            if (existingUser != null)
            {
                return BadRequest(new Response()
                {
                    Success = false,
                    Errors = new()
                    {
                        "Email already in use"
                    }
                });
            }

            // Creating a new user.
            var newUser = new ApplicationUser()
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                Email = req.Email,
                UserName = req.Email,
                PhoneNumber = req.PhoneNumber
            };

            var isCreated = await userManager.CreateAsync(newUser, req.Password);

            if (isCreated.Succeeded)
            {
                return Ok(new Response()
                {
                    Success = true,
                });
            }
            else
            {
                return BadRequest(new Response()
                {
                    Success = false,
                    Errors = new()
                    {
                        "An error orcurred while creating the user."
                    }
                });
            }
        }
        else
        {
            return BadRequest(new Response()
            {
                Success = false,
                Errors = new()
                {
                    "You need to provide email and password"
                }
            });
        }
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateUser(UserUpdateDto req)
    {
        // Finds the user by id.
        var user = await userManager.FindByIdAsync(req.Id.ToString());

        // Checks if the user is null.
        if (user == null)
            return NotFound();

        // Updating the properties of the user.
        user.FirstName = req.FirstName;
        user.LastName = req.LastName;
        user.PhoneNumber = req.PhoneNumber;
        user.UserName = req.Email;
        user.Email = req.Email;

        // Updating the user.
        var isUpdated = await userManager.UpdateAsync(user);

        // Checking of the updated was successful.
        if (isUpdated.Succeeded)
        {
            return Ok(new Response()
            {
                Success = true
            });
        }

        return BadRequest(new Response()
        {
            Success = false,
            Errors = new()
            {
                "Could not update user"
            }
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(UserPasswordChangeDto req)
    {
        var user = await userManager.FindByIdAsync(req.UserId.ToString());

        if (user == null)
        {
            return NotFound("User not found");
        }

        var isChanged = await userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);

        if (isChanged.Succeeded)
        {
            return Ok(new Response()
            {
                Success = true
            });
        }
        else
        {
            return BadRequest(new Response()
            {
                Success = false,
                Errors = new()
                {
                    "Could not update password"
                }
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Finding the user by id.
        var user = await userManager.FindByIdAsync(id.ToString());

        // Checking if the user is null.
        if (user == null)
        {
            return NotFound();
        }

        // Getting the roles the user is assigned.
        var roles = await userManager.GetRolesAsync(user);

        // Removing the user from the roles.
        if (roles.Count > 0)
        {
            foreach (var role in roles)
            {
                await userManager.RemoveFromRoleAsync(user, role);
            }
        }

        // Deleting the user.
        var isDeleted = await userManager.DeleteAsync(user);

        // Checking if the deletion was successful.
        if (isDeleted.Succeeded)
        {
            return Ok(new Response()
            {
                Success = true
            });
        }

        return BadRequest(new Response()
        {
            Success = false,
            Errors = new()
            {
                "Could not delete user"
            }
        });
    }
    
    [HttpPatch("assign-roles")]
    public async Task<IActionResult> UpdateUserRoles(AssignUserRoleDto req)
    {
        // Getting the user.
        var user = await userManager.FindByIdAsync(req.UserId.ToString());
			
        // Checking that the user exists.
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Getting the user's roles.
        var userRoles = await userManager.GetRolesAsync(user);

        // Removing user from all roles.
        var isRemoved = await userManager.RemoveFromRolesAsync(user, userRoles);

        if (!isRemoved.Succeeded)
        {
            return BadRequest("Could not remove user from roles");
        }

        var roles = await roleManager.Roles.ToListAsync();

        var rolesToAssign = new List<string>();

        foreach (var role in roles)
        {
            foreach(var reqRole in req.RoleIds)
            {
                if (role.Id.Equals(reqRole.ToString()))
                {
                    Console.WriteLine(role.Name);
                    rolesToAssign.Add(role.Name!);
                }
            }
        }

        var isAssigned = await userManager.AddToRolesAsync(user, rolesToAssign);

        if (isAssigned.Succeeded)
        {
            return Ok(new Response()
            {
                Success = true
            });
        }
        return BadRequest(new Response()
        {
            Success = false,
            Errors = new()
            {
                "Could not add user to roles."
            }
        });
    }
}