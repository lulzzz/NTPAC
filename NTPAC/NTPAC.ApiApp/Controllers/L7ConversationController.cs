using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NTPAC.DTO.ConversationTracking;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ApiApp.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class L7ConversationController : ControllerBase
  {
    private readonly ICaptureFacade _captureFacade;
    private readonly IL7ConversationFacade _conversationL7Facade;

    public L7ConversationController(IL7ConversationFacade conversationL7Facade, ICaptureFacade captureFacade)
    {
      this._conversationL7Facade = conversationL7Facade;
      this._captureFacade        = captureFacade;
    }

    [HttpDelete("{id}")]
    public async Task Delete(Guid id) { await this._conversationL7Facade.Delete(id).ConfigureAwait(false); }

    [HttpGet("{id}")]
    public async Task<ActionResult<L7ConversationDetailDTO>> Get(Guid id) =>
      new ActionResult<L7ConversationDetailDTO>(await this._conversationL7Facade.GetByIdAsync(id).ConfigureAwait(false));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<L7ConversationListDTO>>> GetAll() =>
      new ActionResult<IEnumerable<L7ConversationListDTO>>(await this._conversationL7Facade.GetAllAsync().ConfigureAwait(false));

    [HttpGet("capture/{captureId}")]
    public async Task<ActionResult<IEnumerable<L7ConversationListDTO>>> GetByCapture(Guid captureId) =>
      new ActionResult<IEnumerable<L7ConversationListDTO>>(
        (await this._captureFacade.GetByIdAsync(captureId).ConfigureAwait(false))?.L7Conversations);
  }
}
