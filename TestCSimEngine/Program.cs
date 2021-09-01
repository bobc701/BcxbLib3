using System;
using System.Collections.Generic;
using System.Diagnostics;
using SimEngine;

namespace TestCSimEngine
{
   class Program
   {
      static void Main(string[] args) {
         Console.WriteLine("Hello World!");

         string jsonString1 = FileHandler.GetTextFileOnDisk("TestCSimEngine.Resources.Model.model1.json");
         string jsonString2 = FileHandler.GetTextFileOnDisk("TestCSimEngine.Resources.Model.model2.json");

         CSimEngine mSim = new();
         CModelBldr.LoadModel(jsonString1, mSim);

         mSim.PrintModel();
         mSim.RaiseHandler += e => {
            Debug.WriteLine($"Debug: AType={e.AType}");
         };

         mSim.DoNamedList("AtBat");

         
      }
   }
}
