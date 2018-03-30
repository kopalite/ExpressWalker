using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressWalker.Visitors
{
    public interface IDictionaryVisitor : IElementVisitor
    {
        ExpressAccessor KeyValueAccessor { get; }
    }
}
