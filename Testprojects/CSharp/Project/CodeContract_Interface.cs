using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Test
{
    [ContractClass(typeof(CodeContract_Contract))]
    public interface CodeContract_Interface
    {
        int Calculate(int value);
    }
}
