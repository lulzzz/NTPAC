using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Nelibur.ObjectMapper;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.DTO.ConversationTracking;
using NTPAC.Persistence.Cassandra.Facades.Converters;
using NTPAC.Persistence.Cassandra.Facades.Mappers;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Interfaces;
using UnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Facades
{
  public class L7ConversationFacade : IL7ConversationFacade
  {
    private readonly IRepositoryReaderAsync<L7ConversationEntity> _repositoryReaderAsync;
    private readonly IRepositoryWriterAsync<L7ConversationEntity> _repositoryWriterAsync;
    private readonly IUnitOfWork _unitOfWork;

    public L7ConversationFacade(IUnitOfWork unitOfWork,
                                IRepositoryWriterAsync<L7ConversationEntity> repositoryWriterAsync,
                                IRepositoryReaderAsync<L7ConversationEntity> repositoryReaderAsync)
    {
      TypeDescriptor.AddAttributes(typeof(IPEndPointEntity), new TypeConverterAttribute(typeof(IPEndPointEntityConverter)));
      TinyMapper.Bind<L7ConversationEntity, L7ConversationListDTO>();
      TinyMapper.Bind<L7ConversationEntity, L7ConversationDetailDTO>(
        config => config.Ignore(l7ConversationEntity => l7ConversationEntity.Pdus));
      TinyMapper.Bind<L7PduEntity, L7PduDTO>();

      this._unitOfWork            = unitOfWork;
      this._repositoryWriterAsync = repositoryWriterAsync;
      this._repositoryReaderAsync = repositoryReaderAsync;
    }

    public Task Delete(Guid id) => this._repositoryWriterAsync.DeleteAsync(id);

    public async Task<IEnumerable<L7ConversationListDTO>> GetAllAsync() =>
      (await this._repositoryReaderAsync.GetAllAsync().ConfigureAwait(false)).Select(l7ConversationEntity =>
      {
        var l7ConversationListDto = TinyMapper.Map<L7ConversationListDTO>(l7ConversationEntity);
        l7ConversationListDto.PduCount = l7ConversationEntity.Pdus.Length;
        return l7ConversationListDto;
      });

    public async Task<IEnumerable<L7ConversationListDTO>> GetByCaptureIdAsync(Guid captureId) =>
      (await this._repositoryReaderAsync
                 .GetAllWhereAsync(l7ConversationEntity => l7ConversationEntity.CaptureId.Equals(captureId))
                 .ConfigureAwait(false)).Select(l7ConversationEntity =>
      {
        var l7ConversationListDto = TinyMapper.Map<L7ConversationListDTO>(l7ConversationEntity);
        l7ConversationListDto.PduCount = l7ConversationEntity.Pdus.Length;
        return l7ConversationListDto;
      });

    public async Task<L7ConversationDetailDTO> GetByIdAsync(Guid id)
    {
      var l7ConversationEntity = await this._repositoryReaderAsync.GetByIdAsync(id).ConfigureAwait(false);
      if (l7ConversationEntity == null)
      {
        return null;
      }

      var l7ConversationDetailDto = TinyMapper.Map<L7ConversationDetailDTO>(l7ConversationEntity);
      l7ConversationDetailDto.Pdus = l7ConversationEntity.Pdus.Select(TinyMapper.Map<L7PduDTO>);
      return l7ConversationDetailDto;
    }

    public async Task InsertAsync(L7Conversation l7Conversation)
    {
      await this._repositoryWriterAsync.InsertAsync(L7ConversationMapper.Map(l7Conversation)).ConfigureAwait(false);
      await this._unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}
