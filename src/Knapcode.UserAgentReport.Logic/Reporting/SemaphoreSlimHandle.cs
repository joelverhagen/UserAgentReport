using System;
using System.Threading;

namespace Knapcode.UserAgentReport.Reporting
{
    public class SemaphoreSlimHandle : IDisposable
    {
        private readonly SemaphoreSlim _semaphoreSlim;

        public SemaphoreSlimHandle(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        public void Dispose()
        {
            _semaphoreSlim.Release();
        }
    }
}