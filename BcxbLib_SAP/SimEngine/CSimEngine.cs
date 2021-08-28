using BCX.BCXB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SimEngine {

   // Example: To do an at bat, you would say:

   //int n = sim.Model["AdBat"].DoIt();


   public class CSimEngine 
   {
   // It is up to the client to populate Version and Model.
   // Each model that gets loaded, the version and model name are concatenated to these props.

      public string Version { get; set; } = "";
      public string ModelName { get; set; } = "";
      public Dictionary<string, List<BaseSimAction>> Model { get; set; } = new();

      public event Action<BaseSimAction> RaiseHandler;
      public event Func<string, StreamReader> RequestEngineFile;
      public event Action<int, string> EEngineError; //New event #1706.20


      public void DoAction(BaseSimAction act) 
      {
      // This will be called by the various subclasses in their DoIt()'s.
      // This event will be handled by the client (eg, CGame).
         RaiseHandler(act);

      }

      public void DoNamedList(string listName) {

         try { 
            Debug.WriteLine($"DoNamedList: {listName}");
            Debug.Indent();
            DoList(Model[listName]);
            Debug.Unindent();
         }
         catch (Exception ex) {
            Debug.WriteLine($"Exception in DoNamedList: {listName}...\r\n{ex.Message}");
            throw;
         }

      }


      public void DoList(List<BaseSimAction> aList) 
      {
         foreach (BaseSimAction act in aList)
         {
            Debug.Indent();
            act.DoIt();
            Debug.Unindent();
         }

      }


      public void PrintModel()
      {
         Debug.WriteLine($"Version: {this.Version}");
         Debug.WriteLine($"Model:");
         foreach (var item in this.Model)
         {
            Debug.WriteLine("");
            Debug.WriteLine($"Tag: {item.Key}");
            PrintActionList(item.Value);

         }

      }


      public static void PrintActionList(List<BaseSimAction> AList)
      {
         Debug.Indent();
         foreach (BaseSimAction a in AList) {
            a.PrintIt();
         }
         Debug.Unindent();

      }


      public int SelectList(string listName, CGame g) {

         // The arg, listName, should point to a List that has a single action of
         // type 'Select'. It scans the 'SItem' sub-items, and returns the
         // 'Res' paramter of the chosen item. This is called separately
         // from DoList by the client app.

         double r = g.rn.NextDouble();
         double cum = 0.0;
         List<BaseSimAction> list = this.Model[listName];
         SelectAction act = (SelectAction)list[0];
         foreach (SItemAction item in act.AList) {
            if (r <= (cum += item.Prob)) return item.Res;
         }
         throw new Exception($"result not found in SelectList({listName}");
      } 


   }

}
