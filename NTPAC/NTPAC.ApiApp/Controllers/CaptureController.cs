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
  public class CaptureController : ControllerBase
  {
    private readonly ICaptureFacade _captureFacade;

    public CaptureController(ICaptureFacade captureFacade) => this._captureFacade = captureFacade;

    [HttpDelete("{id}")]
    public async Task Delete(Guid id) => await this._captureFacade.DeleteAsync(id).ConfigureAwait(false);

    [HttpGet("{id}")]
    public async Task<ActionResult<CaptureDetailDTO>> Get(Guid id) =>
      new ActionResult<CaptureDetailDTO>(await this._captureFacade.GetByIdAsync(id).ConfigureAwait(false));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CaptureListDTO>>> GetAll() =>
      new ActionResult<IEnumerable<CaptureListDTO>>(await this._captureFacade.GetAllAsync().ConfigureAwait(false));
  }
}
