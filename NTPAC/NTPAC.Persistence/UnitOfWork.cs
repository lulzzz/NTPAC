using System;
using NTPAC.Common.Interfaces.Persistence;

namespace NTPAC.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {

        public Int32 Complete() => 0;

        public void Dispose()
        {
          
        }
    }
}