using Microsoft.AspNetCore.Mvc;
using Polls.Common;
using Polls.REST.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Polls.REST.Controllers
{
    [ApiController]
    public abstract class PollController<TEntity, TModel> : ControllerBase
        where TEntity : Poll
        where TModel : PollModel
    {
        protected readonly ICrudServiceAsync<TEntity> _crudService;
        protected readonly ICrudServiceAsync<Person> _personService;
        protected readonly ICrudServiceAsync<Option> _optionService;

        public PollController(
            ICrudServiceAsync<TEntity> crudService,
            ICrudServiceAsync<Person> personService,
            ICrudServiceAsync<Option> optionService
        )
        {
            _crudService = crudService;
            _personService = personService;
            _optionService = optionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TModel>>> GetAllPolls([FromQuery] int page = 1, [FromQuery] int amount = 50)
        {
            var polls = await _crudService.ReadAllAsync(page, amount);
            return Ok(polls.Select(MapToModel));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TModel>> GetPollById(Guid id)
        {
            var poll = await _crudService.ReadAsync(id);
            if (poll == null) return NotFound();
            return Ok(MapToModel(poll));
        }

        [HttpGet("{id}/options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TModel>> GetPollOptionsById(Guid id)
        {
            var poll = await _crudService.ReadAsync(id);
            if (poll == null) return NotFound();
            return Ok(poll.GetOptions());
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TModel>> CreatePoll(
            [FromBody] CreatePollModel createModel)
        {
            try
            {
                var poll = CreateNewPoll(createModel.Title);

                foreach (var createOptionModel in createModel.Options)
                {
                    var option = new Option(createOptionModel.Name);
                    await _optionService.CreateAsync(option);
                    poll.AddOption(option);
                }

                await _crudService.CreateAsync(poll);

                var model = MapToModel(poll);
                return CreatedAtAction(nameof(GetPollById), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Moderator")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePoll(Guid id)
        {
            var poll = await _crudService.ReadAsync(id);
            if (poll == null) return NotFound();
            
            await _crudService.RemoveAsync(poll);
            return NoContent();
        }


        [Authorize(Roles = "Admin,Moderators")]
        [HttpPost("{id}/start")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartPoll(Guid id)
        {
            var poll = await _crudService.ReadAsync(id);
            if (poll == null)
            {
                return NotFound();
            }

            try
            {
                poll.Start();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
    
            var success = await _crudService.UpdateAsync(poll);
            return success ? NoContent() : StatusCode(StatusCodes.Status304NotModified);
        }


        [Authorize(Roles = "Admin,Moderator")]
        [HttpPost("{id}/finish")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> FinishPoll(Guid id)
        {
            var poll = await _crudService.ReadAsync(id);
            if (poll == null)
            {
                return NotFound();
            }
            var result = poll.Finish();
            
            var success = await _crudService.UpdateAsync(poll);
            return success ? Ok(result) : StatusCode(StatusCodes.Status304NotModified);
        }

        [Authorize]
        [HttpPost("{id}/vote")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Vote(Guid id, [FromBody] VoteModel voteModel)
        {
            var person = await _personService.ReadAsync(voteModel.PersonId); 
            if (person == null)
            {
                // Повертаємо 401 з вашим повідомленням
                return Unauthorized("Не авторизовані!"); 
            }

            var poll = await _crudService.ReadAsync(id);
            if (poll == null || !poll.IsOngoing)
            {
                return BadRequest("Голосування не знайдено або ще не розпочалося!");
            }

            try
            {
                poll.Vote(person, voteModel.OptionId); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var success = await _crudService.UpdateAsync(poll);

            if (!success)
            {
                return BadRequest("Помилка голосування");
            }
            
            return NoContent();
        }



        protected abstract TEntity CreateNewPoll(string title);
        protected abstract TModel MapToModel(TEntity poll);
    }
}