namespace WCFTest
{
   public class FirstComputer : IComputation
   {

      public double Negate(double x)
      {
         return -x;
      }

      public double StringToNumber(string text)
      {
         return double.Parse(text);
      }
   }
}