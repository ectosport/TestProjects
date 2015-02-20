using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class AddOperation : IOperation
    {
        public double Compute(double operand1, double operand2 = 0.0)
        {
            return operand1 + operand2;
        }
    }
}
