using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NNRunner
{
    public interface IProcessRepository<TSnapshot>
    {
        Guid CreateProcess(Action<Action<TSnapshot>> process);
        ProcessProgress GetProcessProgress(Guid id);
    }
}
