﻿using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Cotrollers {
    [Route("api/[controller]")] //Handles <url>/api/camps
    [ApiController]
    public class CampsController : ControllerBase {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator) {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
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

                return Ok(_mapper.Map<CampModel>(result));
            } catch (Exception) {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false) {
            try {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any())
                    return NotFound();

                return Ok(_mapper.Map<CampModel[]>(results));
            } catch (Exception) {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model) {
            try {
                var tempModel = _repository.GetCampAsync(model.Moniker);

                if (tempModel != null) {
                    return BadRequest("Moniker in use!");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new {moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location)) {
                    return BadRequest("Could not use current moniker!");
                }

                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);

                if (await _repository.SaveChangesAsync()) {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }

            } catch (Exception) {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}
