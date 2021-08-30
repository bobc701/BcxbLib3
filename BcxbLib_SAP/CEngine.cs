using System;
using System.IO;
using System.Diagnostics;
using BCX.BCXCommon;

namespace BCX.SimEngine {

public delegate void DDoAction(ref int id, int ix, string sList, ref int p, string lvl);

public class CEngine {

   public event DDoAction EDoAction;
   public event Func<string, StreamReader> RequestEngineFile;
   public event Action<int, string> EEngineError; //New event #1706.20
   public string[] aActions;
   public int[] atLen;
   //private StreamReader fEngine = null;
   public int DoOneIndex;
      
// These keep track of lists that have been DoList'd...   
   private int[] lists = new int[25];
   private int listLim = -1;

   /// <summary>
   /// All this does is set listlim to -1.
   /// </summary>
   /// 
   public void ClearLists() {listLim = -1;}      
      

   /// <summary>
   /// This just returns a string that holds all the ints in Lists[],
   /// which is an array of the list numbers taht have been DiList'ed 
   /// on this at bat.
   /// </summary>
   /// 
   public string ShowLists() {
      string s = "";
      for (int i=0; i<=listLim; i++) s += lists[i].ToString() + " ";
      return(s);
	}
	   

	/// <summary>
   /// Contructor -- arg is file name. 
   /// GFileAccess class will supply the path.
   /// </summary>
	/// <param name="engineFileName"></param>
   /// 
   public CEngine(StreamReader fEngine1) {
   // --------------------------------------
      //fEngine = fEngine1;
      ReadEngine(fEngine1);
	}

   /// <summary>
   /// Scan CFActions, convert it to array, aAction
   /// ------------------------------------------------------
   /// StreamReader based on CFEng1 is passed in via constructor.
   /// Read util eof, get n and the string, and
   /// fill aActions. Upper bound to be found at: #RECCNT: 139.
   /// rec looks like this:
   /// 0x00000000,0x0000,1,"25?Y4025?]4055?^40450;40O5A;403P"
   /// </summary>
   /// 
   public void ReadEngine(StreamReader fEngine) {

      string rec; int n;
      
      while ((rec = fEngine.ReadLine()) != null) {
         if (rec.Substring(0, 7) == "#RECCNT") {
            n = int.Parse(rec.Substring(9));
            aActions = new string[n+1]; //So that last elt is #n
         }
         if (rec[0]=='#') continue;
         //rec = rec.Remove(0,18);  //delete(rec, 1, 18);
         n = rec.IndexOf(",");
         if (n == 0) {
            throw new Exception ("Invalid format in CFEng1.bcx");
         }
         int ix = int.Parse(rec.Substring(0,n));
         string s = rec.Substring(n+1);
         CBCXCommon.DeQuote(ref s);
         aActions[ix] = s;
      }
      fEngine.Dispose();
      fEngine = null;

   }


   /// <summary>
   /// It is used in unpacking action lists. It converts a character into an
   /// int by subtracting 48.
   /// </summary>
   /// 
   public static int Decoded(char c) {
      return (int)c-48;
   }

   /// <summary>
   /// It is used in unpacking action lists. It converts 2 characters into an
   /// int by subtracting 48 from both and multiplying the first by 64.
   /// </summary>
   /// 
   public static int Decoded(char c1, char c2) {
      return 64*((int)c1-48) + ((int)c2-48);
   }

   // Appears to have no references...
   //public void DoList (int n, string lvl) {
   //// --------------------------------------------------------     
   //// Error handling added #1706.20...
   //   const int atEndListDef = 31; //This is universal, others can vary between apps.
   //   int p = 0;  //was 1 in Delphi
   //   string sList = "";
      
   //   try {
   //      if (listLim < 24) lists [++listLim] = n;

   //      sList = aActions [n] + (char)(48 + atEndListDef);
   //      Debug.WriteLine (lvl + "DoList({0}, sList={1})", n, sList);
   //      int at = Decoded (sList [0]);
   //      while (at != atEndListDef) {
   //         EDoAction (ref at, n, sList, ref p, lvl + "  ");
   //         p += atLen [at];
   //         at = Decoded (sList [p]);
   //      }

   //   } 
   //   catch (Exception ex) {
   //      Debug.WriteLine ("Exception in DoList(" + n + ":" + sList);
   //      Debug.WriteLine (ex.Message);
   //      EEngineError (n, sList);
   //   }
      
   //}
     
}  

} 
