using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    interface IOperation
    {
        double Compute(double operand1, double operand2 = 0.0);
    }
}
