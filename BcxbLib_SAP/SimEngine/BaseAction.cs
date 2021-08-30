using BCX.BCXB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SimEngine
{
   public abstract class BaseSimAction {
      public TAction AType { get; set; }
      public abstract int DoIt();
      public abstract void PrintIt();
      public CSimEngine mSim { get; set; } 

   }

}


