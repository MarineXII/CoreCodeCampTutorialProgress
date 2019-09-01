using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Cotrollers {
    [Route("api/[controller]")] //Handles <url>/api/
    public class CampsController : ControllerBase {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository repository, IMapper mapper) {
            _repository = repository;
            _mapper = mapper;
        }
        
        [HttpGet] //Specifies it is a get request
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false) {
            try {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                return Ok(_mapper.Map<CampModel[]>(results));
            } catch (Exception) {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker) {
            try {
                var result = await _repository.GetCampAsync(moniker);

                if (result == null)
                    return NotFound();

                return _mapper.Map<CampModel>(result);
            } catch (Exception) {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}
