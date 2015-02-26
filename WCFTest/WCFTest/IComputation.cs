using System.ServiceModel;

namespace WCFTest
{
   [ServiceContract]
   public interface IComputation
   {
      [OperationContract]
      double Negate(double x);

      [OperationContract]
      double StringToNumber(string text);
   }
}