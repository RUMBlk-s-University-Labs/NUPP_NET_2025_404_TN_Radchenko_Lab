using Microsoft.AspNetCore.Mvc;
using Polls.REST.Models;
using Polls.Common;

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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonModel>> GetPersonById(Guid id)
        {
            var person = await _personService.ReadAsync(id);
            if (person == null) return NotFound();
            return Ok(MapToModel(person));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PersonModel>> CreatePerson([FromBody] CreatePersonModel _model)
        {
            try
            {
                var person = new Person(_model.Name);

                await _personService.CreateAsync(person);
            
                var model = MapToModel(person);
                return CreatedAtAction(nameof(GetPersonById), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePerson(Guid id, [FromBody] CreatePersonModel model)
        {
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