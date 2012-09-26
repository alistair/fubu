using System;
using System.Collections.Generic;
using FubuMVC.Core;

namespace Fubu.Applications
{
    public interface IApplicationSourceTypeFinder
    {
        IEnumerable<Type> FindApplicationSourceTypes(ApplicationSettings settings);
    }
}