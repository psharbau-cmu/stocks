﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NNRunner
{
    public interface IProcessRepository<TSnapshot>
    {
        IEnumerable<Guid> GetIds();
        Guid CreateProcess(Action<Action<TSnapshot>> process);
        ProcessProgress<TSnapshot> GetProcessProgress(Guid id);
    }
}
