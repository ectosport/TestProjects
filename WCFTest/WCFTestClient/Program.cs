using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFTestClient
{
   class Program
   {
      static void Main(string[] args)
      {
         BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
         EndpointAddress endpoint = new EndpointAddress("http://localhost:1972/Computer");
         ComputationClient client = new ComputationClient(binding, endpoint);

         double answer = client.Negate(-3.14159);
         double answer2 = client.StringToNumber("2.71828");
      }
   }
}
