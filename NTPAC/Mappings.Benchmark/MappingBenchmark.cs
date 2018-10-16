using System;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using Nelibur.ObjectMapper;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Persistence.Cassandra.Facades.Mappers;
using NTPAC.Persistence.Entities;

namespace Mappings.Benchmark
{
  public class MappingBenchmark
  {
    private readonly L7Pdu _l7Pdu;

    public MappingBenchmark()
    {
      Mapper.Initialize(cfg => cfg.CreateMap<L7Pdu, L7PduEntity>());
      TinyMapper.Bind<L7Pdu, L7PduEntity>();

      this._l7Pdu = new L7Pdu(new Frame {L7Payload = new Byte[0]}, FlowDirection.Up);
    }

    [Benchmark]
    public void AutoMapperBenchmark()
    {
      var l7Pdu = Mapper.Map<L7PduEntity>(this._l7Pdu);
    }

    [Benchmark]
    public void ManualMapperBenchmark()
    {
      var l7Pdu = L7PduMapper.Map(this._l7Pdu);
    }

    [Benchmark]
    public void TinyMapperBenchmark()
    {
      var l7Pdu = TinyMapper.Map<L7PduEntity>(this._l7Pdu);
    }
  }
}
