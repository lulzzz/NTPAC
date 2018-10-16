using System;
using System.ComponentModel;
using System.Globalization;
using NTPAC.DTO.ConversationTracking;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Cassandra.Facades.Converters
{
  internal sealed class IPEndPointEntityConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type dstType) => dstType == typeof(IPEndPointEntity);
    public override bool CanConvertTo(ITypeDescriptorContext context, Type dstType) => dstType   == typeof(IPEndPointDTO);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object obj, Type dstType)
    {
      var ipEndPointEntity = (IPEndPointEntity) obj;
      return new IPEndPointDTO
             {
               Port          = ipEndPointEntity.Port,
               Address       = ipEndPointEntity.Address.ToString(),
               AddressFamily = ipEndPointEntity.AddressFamily
             };
    }
  }
}
