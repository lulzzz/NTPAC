using System.Collections.Generic;

namespace NTPAC.Common.Interfaces
{
  public interface IMaybeMultipleValues<T> where T : class
  {
  }

  public interface ISingleValue<T> : IMaybeMultipleValues<T> where T : class
  {
  }

  public interface IMultipleValues<T> : IMaybeMultipleValues<T> where T : class
  {
    IEnumerable<T> Values { get; }
  }
}
