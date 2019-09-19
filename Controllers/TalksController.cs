using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/[controller]")]
    [ApiController]
    public class TalksController : ControllerBase {

        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository icampRepository, IMapper mapper, LinkGenerator linkGenerator) {
            _repository = icampRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet("general")]
        public async Task<ActionResult<TalkModel[]>> generalGet(string moniker) {
            try {
                var talks = await _repository.GetTalksByMonikerAsync(moniker);
                return _mapper.Map<TalkModel[]>(talks);
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }

        [HttpGet("id={id:int}")]
        public async Task<ActionResult<TalkModel>> specificGet(string moniker, int id) {
            try {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id);
                return _mapper.Map<TalkModel>(talk);
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }
    }
}