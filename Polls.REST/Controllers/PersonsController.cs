using Microsoft.AspNetCore.Mvc;
using Polls.REST.Models;
using Polls.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Polls.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly ICrudServiceAsync<Person> _personService;

        public PersonsController(ICrudServiceAsync<Person> personService)
        {
            _personService = personService;
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonModel>> GetPersonById(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!User.IsInRole("Admin") && currentUserId != id.ToString())
            {
                return Forbid();
            }

            var person = await _personService.ReadAsync(id);
            if (person == null) return NotFound();
            return Ok(MapToModel(person));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Polls.REST.Models.PersonModel>> CreatePerson(
            [FromBody] CreatePersonModel _model,
            [FromServices] UserManager<Polls.Infrastructure.Models.PersonModel> userManager) 
        {
            try
            {
                var person = new Person(_model.Name);
                await _personService.CreateAsync(person);

                var identityUser = await userManager.FindByIdAsync(person.Id.ToString());
                
                if (identityUser != null)
                {
                    identityUser.UserName = _model.Email;
                    identityUser.Email = _model.Email;
                    identityUser.Name = _model.Name;
                    identityUser.EmailConfirmed = true;

                    await userManager.UpdateSecurityStampAsync(identityUser);

                    var passResult = await userManager.AddPasswordAsync(identityUser, _model.Password);
                    
                    if (!passResult.Succeeded)
                    {
                        return BadRequest(passResult.Errors.Select(e => e.Description));
                    }

                    await userManager.UpdateAsync(identityUser);

                    if (identityUser.Name != null && identityUser.Name.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        await userManager.AddToRoleAsync(identityUser, "Admin");
                    }
                    else if (identityUser.Name != null && identityUser.Name.Equals("moderator", StringComparison.OrdinalIgnoreCase))
                    {
                        await userManager.AddToRoleAsync(identityUser, "Moderator");
                    }

                    var roles = await userManager.GetRolesAsync(identityUser);
                    if (roles.Count == 0) 
                    {
                        await userManager.AddToRoleAsync(identityUser, "Voter");
                    }
                }
            
                var resultModel = MapToModel(person); 
                return CreatedAtAction(nameof(GetPersonById), new { id = resultModel.Id }, resultModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] CreatePersonModel model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && currentUserId != id.ToString())
            {
                return Forbid();
            }

            var person = await _personService.ReadAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            try
            {
                person.SetName(model.Name);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            await _personService.UpdateAsync(person);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePerson(Guid id)
        {
            var person = await _personService.ReadAsync(id);
            if (person == null) return NotFound();
            
            await _personService.RemoveAsync(person);
            return NoContent();
        }

        private PersonModel MapToModel(Person person)
        {
            return new PersonModel
            {
                Id = person.Id,
                Name = person.Name
            };
        }
    }
}