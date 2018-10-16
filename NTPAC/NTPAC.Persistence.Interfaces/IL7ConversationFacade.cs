using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.DTO.ConversationTracking;

namespace NTPAC.Persistence.Interfaces
{
  public interface IL7ConversationFacade
  {
    Task Delete(Guid id);
    Task<IEnumerable<L7ConversationListDTO>> GetAllAsync();
    Task<IEnumerable<L7ConversationListDTO>> GetByCaptureIdAsync(Guid captureId);
    Task<L7ConversationDetailDTO> GetByIdAsync(Guid id);
    Task InsertAsync(L7Conversation l7Conversation);
  }
}
