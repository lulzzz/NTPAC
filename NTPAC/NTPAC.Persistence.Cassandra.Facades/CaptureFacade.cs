using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nelibur.ObjectMapper;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.DTO.ConversationTracking;
using NTPAC.Persistence.Cassandra.Facades.Mappers;
using NTPAC.Persistence.Entities;
using NTPAC.Persistence.Interfaces;
using UnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Facades
{
  public class CaptureFacade : ICaptureFacade
  {
    private readonly IRepositoryReaderAsync<CaptureEntity> _captureRepositoryReaderAsync;
    private readonly IRepositoryWriterAsync<CaptureEntity> _captureRepositoryWriterAsync;

    private readonly IL7ConversationFacade _l7ConversationFacade;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

    public CaptureFacade(IUnitOfWork unitOfWork,
                         IRepositoryWriterAsync<CaptureEntity> captureRepositoryWriterAsync,
                         IRepositoryReaderAsync<CaptureEntity> captureRepositoryReaderAsync,
                         IL7ConversationFacade l7ConversationFacade)
    {
      TinyMapper.Bind<CaptureEntity, CaptureListDTO>();
      TinyMapper.Bind<CaptureEntity, CaptureDetailDTO>();

      this._unitOfWork                   = unitOfWork;
      this._captureRepositoryWriterAsync = captureRepositoryWriterAsync;
      this._captureRepositoryReaderAsync = captureRepositoryReaderAsync;
      this._l7ConversationFacade         = l7ConversationFacade;
    }

    public async Task DeleteAsync(Guid id)
    {
      await this._sem.WaitAsync().ConfigureAwait(false);
      try
      {
        await this._deleteAsync(id).ConfigureAwait(false);
      }
      finally
      {
        this._sem.Release(1);
      }
    }

    private async Task _deleteAsync(Guid id)
    {
      await this._captureRepositoryWriterAsync.DeleteAsync(id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<CaptureListDTO>> GetAllAsync()
    {
      await this._sem.WaitAsync().ConfigureAwait(false);
      try
      {
        var captureEntities = await this._captureRepositoryReaderAsync.GetAllAsync().ConfigureAwait(false);
        return captureEntities.Select(TinyMapper.Map<CaptureListDTO>);
      }
      finally
      {
        this._sem.Release(1);
      }      
    }

    public async Task<CaptureDetailDTO> GetByIdAsync(Guid id)
    {
      CaptureEntity captureEntity;
      
      await this._sem.WaitAsync().ConfigureAwait(false);
      try
      {
        captureEntity = await this._captureRepositoryReaderAsync.GetByIdAsync(id).ConfigureAwait(false);
        //return captureEntity == null ? null : TinyMapper.Map<CaptureDetailDTO>(captureEntity);
        if (captureEntity == null)
        {
          return null;
        }
      }
      finally
      {
        this._sem.Release(1);
      }      
      
      var captureDetailDto = TinyMapper.Map<CaptureDetailDTO>(captureEntity);
      captureDetailDto.L7Conversations =
        await this._l7ConversationFacade.GetByCaptureIdAsync(captureDetailDto.Id).ConfigureAwait(false);
      return captureDetailDto;
    }

    public async Task InsertAsync(Capture capture)
    {
      await this._sem.WaitAsync().ConfigureAwait(false);
      try
      {
        await this._insertAsync(capture).ConfigureAwait(false);
      }
      finally
      {
        this._sem.Release(1);
      }      
    }

    public async Task UpdateAsync(Capture capture)
    {
      await this._sem.WaitAsync().ConfigureAwait(false);
      try
      {
        await this._deleteAsync(capture.Id).ConfigureAwait(false);
        await this._insertAsync(capture).ConfigureAwait(false);
      }
      finally
      {
        this._sem.Release(1);
      }    
    }

    private async Task _insertAsync(Capture capture)
    {
      await this._captureRepositoryWriterAsync.InsertAsync(CaptureMapper.Map(capture)).ConfigureAwait(false);
      await this._unitOfWork.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}
