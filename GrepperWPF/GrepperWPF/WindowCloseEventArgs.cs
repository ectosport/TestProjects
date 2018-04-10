using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSearch
{
   internal class WindowCloseEventArgs : EventArgs
   {
      public bool IsCancelled { get; private set; }

      public WindowCloseEventArgs(bool isCancelled)
      {
         this.IsCancelled = isCancelled;
      }
   }
}
