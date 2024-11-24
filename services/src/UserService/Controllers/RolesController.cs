using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Dtos;
using UserService.Extensions;
using UserService.Models;

namespace UserService.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize(Roles = "SystemAdmin")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var result = roles.Select(x => x.ToDto());

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        // Getting the role from the database.
        var role = await _roleManager.FindByIdAsync(id.ToString());

        // Checking if the role exists.
        if (role == null)
            return NotFound();

        // Mapping the role to the ReadDto object.
        var dto = role.ToDto();
        
        var users = await _userManager.GetUsersInRoleAsync(dto.Name);
        var usersInRole = users.Select(x => x.ToDto());

        return Ok(new
        {
            success = true,
            role = dto,
            users = usersInRole
        });
    }

    [HttpGet("users-in-roles/{id:guid}")]
    public async Task<IActionResult> GetUsersInRole(Guid id)
    {
        // Getting the role from the database.
        var role = await _roleManager.FindByIdAsync(id.ToString());

        // Checking if the role exists.
        if (role == null)
            return NotFound();

        var users = await _userManager.GetUsersInRoleAsync(role.Name);
        var usersInRole = users.Select(x => x.ToDto());

        return Ok(usersInRole);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto req)
    {
        // Checking the model state.
        if (ModelState.IsValid)
        {
            // Checking if the role exists.
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                if (role.Name.ToLower().Equals(req.Name.ToLower()))
                {
                    return BadRequest(new Response()
                    {
                        Success = false,
                        Errors = new()
                        {
                            "There is already a role with that name."
                        }
                    });
                }
            }

            // Defining the role object.
            var newRole = new IdentityRole(req.Name);

            // Creating the role.
            var isCreated = await _roleManager.CreateAsync(newRole);

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
                        "An error orcurred while creating the role."
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
                    "You need to provide a name for the role."
                }
            });
        }
    }
    
    [HttpPatch]
    public async Task<IActionResult> UpdateRole(RoleUpdateDto req)
    {
        // Finds the role by id.
        var role = await _roleManager.FindByIdAsync(req.Id.ToString());

        // Checking if the role exists.
        if (role == null)
        {
            return NotFound();
        }

        // Updating the properties of the role.
        role.Name = req.Name;

        // Updating the role.
        var isUpdated = await _roleManager.UpdateAsync(role);

        // Checking if the role update was successful.
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
                "Could not update the role"
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        // Finds the role by id.
        var role = await _roleManager.FindByIdAsync(id.ToString());

        // Checking if the role exists.
        if (role == null)
        {
            return NotFound();
        }

        // Getting the users in the role.
        var users = await _userManager.GetUsersInRoleAsync(role.Name);

        // Removing users from the role.
        foreach (var user in users)
        {
            await _userManager.RemoveFromRoleAsync(user, role.Name);
        }

        // Deleting the role.
        var isDeleted = await _roleManager.DeleteAsync(role);

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
                "Could not delete role"
            }
        });
    }
}