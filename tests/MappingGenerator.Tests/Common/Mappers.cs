using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Talk2Bits.MappingGenerator.Abstractions;

namespace MappingGenerator.Tests.Common
{
    [MappingGenerator(typeof(SourceInner), typeof(DestinationInner))]
    public partial class InnerMapper
    { }
}
