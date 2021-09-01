using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCX.BCXB {

/// <summary>
/// Class to store the linescore
/// This allows for an unlimited number of innings!
/// </summary>
/// 
public class CLineScore {

   /// <summary>
   /// Internal storage is a List of 2-element arrays of int.
   /// </summary>
   private List<int[]> lineScore = new List<int[]>();


   /// <summary>
   /// Constructor: We start off with one element, for inning 1.
   /// </summary>
   /// 
   public CLineScore() {

      lineScore.Add(new int[2] {0,0}); //Start with elt #0 as inning 1.

   }


   /// <summary>
   /// Indexer to set or retrieve a half inning value
   /// </summary>
   /// <remarks>
   /// There is an offset of 1 -- Inning 1 is element #0
   /// </remarks>
   /// 
   public int this[int ab, int inn] {
		   get { 
            if (inn > lineScore.Count) return 0;
            else return lineScore[inn-1][ab];
         }
		   set { 
            while (inn > lineScore.Count) {
               lineScore.Add(new int[2] {0,0});
            }
            lineScore[inn-1][ab] = value;         
         }
	   }

   }

}
