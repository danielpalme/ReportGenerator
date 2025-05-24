using System;
using System.Diagnostics.Contracts;

namespace Test
{
    [ContractClassFor(typeof(CodeContract_Interface))]
    internal abstract class CodeContract_Contract : CodeContract_Interface
    {
        public int Calculate(int value)
        {
            Contract.Requires<ArgumentException>(value != 0, "Value must not be zero");

            return default(int);
        }
    }
}
