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
                var talks = await _repository.GetTalksByMonikerAsync(moniker, true);
                return _mapper.Map<TalkModel[]>(talks);
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }

        [HttpGet("id={id:int}")]
        public async Task<ActionResult<TalkModel>> specificGet(string moniker, int id) {
            try {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true);
                return _mapper.Map<TalkModel>(talk);
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks");
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<TalkModel>> create(string moniker, TalkModel newTalk) {
            try {
                var camp = await _repository.GetCampAsync(moniker, true);

                if (camp == null) {
                    return BadRequest("Camp does not exist");
                }

                var talk = _mapper.Map<Talk>(newTalk);

                talk.Camp = camp;


                if (newTalk.Speaker == null) {
                    return BadRequest("Speaker ID is not specified!");
                }

                var speaker = await _repository.GetSpeakerAsync(newTalk.Speaker.SpeakerId);

                if (speaker == null) {
                    return BadRequest("Could not find speaker!");
                }

                talk.Speaker = speaker;

                _repository.Add(talk);

                if (await _repository.SaveChangesAsync()) {
                    var location = _linkGenerator.GetPathByAction(
                                        HttpContext,
                                        "specificGet",
                                        values: new { moniker = moniker, id = talk.TalkId });

                    return Created(location, _mapper.Map<TalkModel>(talk));
                } else {
                    return BadRequest("Failed to save talk");
                }
            
            } catch (Exception) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            } 
        }
    }
}