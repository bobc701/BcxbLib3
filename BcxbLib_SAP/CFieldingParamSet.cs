//#define IOS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if WINDOWS
   using System.Drawing;
#elif IOS
   using CoreGraphics;
   using UIKit;
#endif

namespace BCX.BCXB {

   public class CFieldingParamSet : CParamSet {

      public double goodPlayProb, badPlayProb;
      public string description;
      public string fielderName;
      public string fielderSkill;
      
      private int skill;
      private string [] aDescrip;

   // aPct[0] and aPct[6] are set half way to 0 & 1, repectively.
   // This is arbitrary -- so Fielding bar always shows 2 colors.
      private double[] aPct = { 0.0833, 0.1667, 0.3333, 0.5, 0.6667, 0.8333, 0.9167 };
      

      /// <summary>
      /// This constructor takes a skill value (0..6)
      /// </summary>
      /// 
      public CFieldingParamSet(int _skill) {
      
         skill = _skill;
         if (skill < 0) skill = 0;
         if (skill > 6) skill = 6;
         goodPlayProb = aPct[skill];
         badPlayProb = 1.0 - goodPlayProb;

         SegmentCount = 2;

#if WINDOWS
            SegmentColors = new Color[] { Color.White, Color.LightGreen, Color.Red };
#elif IOS
            SegmentColors = new UIColor[] { UIColor.White, UIColor.Green, UIColor.Red };
#elif XF
         SegmentColors = new uint[] { 0xFFFFFFFF, 0xFF008000, 0xFFFF0000 };
#endif
         SegmentLabels = new string[] { "X", "1", "2" };
      }


      /// <summary>
      /// Parameterless constructor, Probably not used.
      /// </summary>
      /// 
      public CFieldingParamSet() {
      
         goodPlayProb = 1.0; badPlayProb = 0.0;
         SegmentCount = 2;
#if WINDOWS
         SegmentColors = new Color[] { Color.White, Color.LightGreen, Color.Red };
#elif IOS
         SegmentColors = new UIColor[] { UIColor.White, UIColor.Green, UIColor.Red }; 
#elif XF
         SegmentColors = new uint[] { 0xFFFFFFFF, 0xFF008000, 0xFFFF0000 };
#endif
         SegmentLabels = new string[] { "X", "1", "2" };
     }

     
      public string Description {
         set {
            description = value;
            string [] a = description.Split ('/');
            if (a.Length >= 2) {
               SegmentLabels [1] = a [0];
               SegmentLabels [2] = a [1];
            }
         }
      }


      /// <summary>
      /// This converts good &/ bad to an array to be used by classses that 
      /// are application-specific-agnostic
      /// </summary>
      /// 
      public override double[] GetWidthArray() {

         const double virtualWidth = 1.0;
         double[] widths = new double[3];

         widths[1] = goodPlayProb * virtualWidth;
         widths[2] = badPlayProb * virtualWidth;

         return widths;

      }


      /// <summary>
      /// This 'rolls the dice' for a fielding play, returning a CDiceRoll object.
      /// This implements the abstract method.
      /// </summary>
      /// 
      public override CDiceRoll RollTheDice(Random rn) {

         TLR tlr; double pib;
         double r = rn.NextDouble();
         if (r <= goodPlayProb) {
            tlr = TLR.GoodPlay; 
            pib = r / goodPlayProb;
         }
         else {
            tlr = TLR.BadPlay; 
            pib = (r-goodPlayProb) / badPlayProb;
         }
         return new CDiceRoll(tlr, pib, r);
      }

      public override CDiceRoll GetTlr(TLR tlr, Random rn1) {
      // ----------------------------------------------------------------
      // This is intended to be called from the action, <CalcTlr>, handler.

         double pib = rn1.NextDouble(); 
         return new CDiceRoll(tlr, pib, 0.0);
      }
   }
}
