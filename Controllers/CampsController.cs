using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")] //Handles <url>/api/camps
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet("general")] //Specifies it is a get request
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return Ok(_mapper.Map<CampModel[]>(results));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("specific/{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null)
                    return NotFound();

                return Ok(_mapper.Map<CampModel>(result));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any())
                    return NotFound();

                return Ok(_mapper.Map<CampModel[]>(results));
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<CampModel>> add(CampModel model)
        {
            try
            {
                var tempModel = await _repository.GetCampAsync(model.Moniker);

                if (tempModel != null)
                {
                    return BadRequest("Moniker in use!");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker!");
                }

                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);

                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }

            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("update")]
        public async Task<ActionResult<CampModel>> update(Tuple<string, CampModel> para_tuple)
        {
            string moniker = para_tuple.Item1;
            CampModel model = para_tuple.Item2;

            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);

                if (oldCamp == null)
                {
                    return NotFound($"Camp not found with moniker {moniker}");
                }

                _mapper.Map(model, oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return _mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
            return BadRequest();
        }

        [HttpDelete("delete/{moniker}")]
        public async Task<IActionResult> deleteMoniker(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);

                if (oldCamp == null)
                {
                    return NotFound();
                }

                _repository.Delete(oldCamp);

                if (await _repository.SaveChangesAsync())
                {
                    return Ok();
                }

            } 
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}