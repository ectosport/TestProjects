using System.ServiceModel;

namespace WCFTest
{
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
   public class FirstComputer : IComputation
   {
      public int Multipler { get; set; }

      public double Negate(double x)
      {
         return -x * Multipler;
      }

      public double StringToNumber(string text)
      {
         return double.Parse(text);
      }
   }
}