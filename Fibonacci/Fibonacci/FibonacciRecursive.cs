namespace Fibonacci
{
    public class FibonacciRecursive : ISolveFibonacci
    {
        public int Calculate(int n)
        {
            if (n == 0) return 0;
            else if (n == 1) return 1;
            else return Calculate(n - 1) + Calculate(n - 2);
        }
    }
}