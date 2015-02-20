namespace WpfApplication1
{
    class SubtractOperation : IOperation
    {
        public double Compute(double operand1, double operand2 = 0.0)
        {
            return operand1 - operand2;
        }
    }
}