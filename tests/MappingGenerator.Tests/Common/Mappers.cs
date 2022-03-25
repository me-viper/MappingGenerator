using MappingGenerator.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MappingGenerator.Tests.Common
{
    [MappingGenerator(typeof(SourceInner), typeof(DestinationInner))]
    public partial class InnerMapper
    { }
}
