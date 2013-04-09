using System;
using System.Collections.Generic;

namespace Fubu.Running
{
    public interface IApplicationUnderTestFinder
    {
        IEnumerable<Type> Find();

    }
}