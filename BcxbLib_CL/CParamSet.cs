//#define IOS

using System;
using static System.Math;
#if WINDOWS
   using System.Drawing;
#elif IOS
using UIKit;
#endif


namespace BCX.BCXB {

   public class BarSegment { public double left, width; }
   //public enum TLR { none = 0, hr = 1, b3 = 2, b2 = 3, b1 = 4, bb = 5, oth = 6, so = 7, GoodPlay = 1, BadPlay = 2 }
      public enum TLR { none = 0, 
         hr = 1, b3 = 2, b2 = 3, b1 = 4, bb = 5,
         fo = 6, ld = 7, pu = 8, gr = 9, so = 10,
         GoodPlay = 1, BadPlay = 2 }

   
   public abstract class CParamSet {

      private double[] barWidths = new double[7];
      public int SegmentCount;
      public string[] SegmentLabels;
#if WINDOWS
      public Color[] SegmentColors;
#elif IOS
      public UIColor[] SegmentColors;  
#elif XF
      public uint[] SegmentColors;
#endif     
      public abstract double[] GetWidthArray();
      public abstract CDiceRoll RollTheDice(Random rn1);
      public abstract CDiceRoll GetTlr(TLR tlr, Random rn1);
   
   }


   /// <summary>
   /// This is the core parameter calculator for batters.
   /// For input it uses the 2 args, the batter's actual stats (batStat), and
   /// the league-level CParamSet (lgParam).
   /// It's output is the parameters, hr, b3, b2, ...
   /// </summary>
   public class CHittingParamSet : CParamSet {

      public double h, hr, b3, b2, bb, hbp=0.008, oth, fo, ld, pu, gr, so, sb;
      public double fPitBase;     //Scales 3ip+h+w to bfp
      public double fSacBunt;     //Converts sac bunts to failed sac bunts
      public static double[] othSplits = { .370, .180, .200, .250 };
      public int complPct; // Pct of season. New 1907.03

      //public double[] barWidths = new double[7];


      // Factors for missing stats (based on 1970 data)...
      private const double FAC_SF = 0.0075;       //Apply to AB
      private const double FAC_IBB = 0.1067;      //Apply to BB
      private const double FAC_CS = 0.565;        //Apply to SB
      private const double FAC_SO = 0.1693;       //Apply to AB
      private const double FAC_SACBUNT = 0.3333;  //Ratio of failed sac bunts to sac bunts
      private const double FAC_HBP = 0.0062;      //Apply tp AB
      private const double FAC_SH = 0.0071;       //Apply to AB (before FAC_SACBUNT) (Excludes P's)
      private const double ADJ_FOR_OTHER_HITS = 0.015;
      private const double FAC_SH2 = 0.0077;      //Ratio of Sac bunt attempts to BFP


      public CHittingParamSet(
         double _h, double _hr, double _b3, double _b2,
         double _bb, double _so, double _oth, double _sb,
         double _fo, double _ld, double _pu, double _gr) {
      // ------------------------------------------------------------------------
         h = _h; hr = _hr; b3 = _b3; b2 = _b2; bb = _bb; so = _so;
         oth = _oth;
         sb = _sb;
         fo = _fo; ld = _ld; pu = _pu; gr = _gr;
         SetupSegments();
      }


      public CHittingParamSet() {
      // ------------------------------------------------------------------------
         h = 0.0; hr = 0.0; b3 = 0.0; b2 = 0.0; bb = 0.0; so = 0.0;
         oth = 0.0; sb = 0.0;

         fo = 0.0; ld = 0.0; pu = 0.0; gr = 0.0;
         SetupSegments();
      }


      private void SetupSegments() {
      // -----------------------------------
         SegmentCount = 10; //They are numbered 1 to SegmentCount.
         SegmentLabels = new string[] { 
            "n/a", "hr", "3b", "2b", "1b", "bb", "fl", "ld", "pu", "gr", "so" };
#if WINDOWS
            SegmentColors = new Color[] {
               Color.White,
               Color.Red, Color.Yellow, Color.Blue, Color.LightGreen,
               Color.Brown, Color.LightGray, Color.LightGray, Color.LightGray, Color.LightGray, Color.Black};
#elif IOS
            SegmentColors = new UIColor[] {
               UIColor.White,
               UIColor.Red, UIColor.Yellow, UIColor.Blue, UIColor.Green,
               UIColor.Brown, UIColor.LightGray, UIColor.LightGray, UIColor.LightGray, 
               UIColor.LightGray, UIColor.Black};
#elif XF
            SegmentColors = new uint[] {
               0xFFFFFFFF,
               0xFFFF0000, 0xFFFFFF00, 0xFF0000FF, 0xFF008000,
               0xFFA52A2A, 0xFFD3D3D3, 0xFFD3D3D3, 0xFFD3D3D3,
               0xFFD3D3D3, 0xFF000000};
#endif

      }

      public void CombineParameters(
         CHittingParamSet bpar, CHittingParamSet bLgMean, 
         CHittingParamSet ppar, CHittingParamSet pLgMean, 
         CHittingParamSet cmean) {
      // --------------------------------------------------------------------
      // #2101.01 - This was overhauled to use Meld.
      // This is where batter & pitcher parameters are combined.
      // bpar is batter's parameters, ppar is pitcher's.
      // Also uses bLgMean & pLgMean, which which are b's and p's league mean.
      // --------------------------------------------------------------------
         //this.h = Round(bpar.h * ppar.h / mean.h, 4);
         //this.bb = Round(bpar.bb * ppar.bb / mean.bb, 4);
         //this.so = Round(bpar.so * ppar.so / mean.so, 4);

         this.h = Meld1(bpar.h, bLgMean.h, ppar.h, pLgMean.h);
         this.bb = Meld1(bpar.bb, bLgMean.bb, ppar.bb, pLgMean.bb);
         this.so = Meld1(bpar.so, bLgMean.so, ppar.so, pLgMean.h);

         this.b2 = bpar.b2;
         this.b3 = bpar.b3;

         //this.hr = Round(bpar.hr * ppar.hr / mean.hr, 4);
         this.hr = Meld1(bpar.hr, bLgMean.hr, ppar.hr, pLgMean.hr);


         this.oth = Round(1.0 - (this.h + this.bb + this.so), 4);
         this.sb = bpar.sb;

      // Split oth into the various 'out' components...
         this.fo = Round(this.oth * CHittingParamSet.othSplits[0], 4);
         this.ld = Round(this.oth * CHittingParamSet.othSplits[1], 4);
         this.pu = Round(this.oth * CHittingParamSet.othSplits[2], 4);
         this.gr = Round(this.oth * CHittingParamSet.othSplits[3], 4);

      // Need to compute 'cmean' for backward compat for drawing disk purpose.
      // Otherwise 'cmean' is no longer used, replaced by 'Meld'.
         cmean.CombineLeagueMeans(bLgMean, pLgMean);

      }


      public double Meld1(double B, double BL, double P, double PL) {
      // -----------------------------------------------------------
         /* This method compares batter (B) & pitcher (P) to their own league 
          * norms (BL & PL), then multiplies the combined ratio (X) times the combine
          * league norn (CL)
          */
         if (B <= BL || P <= PL) {
            double X = (B / BL) * (P / PL);
            double CL = 0.5 * (BL + PL);
            double ans = X * CL;
            if (ans < 1.0) return ans;
         }
         return 1.0 - Meld1(1.0 - B, 1.0 - BL, 1.0 - P, 1.0 - PL);
      }


      public double Meld2(double B, double BL, double P, double PL) {
      // ------------------------------------------------------------
         /* This method compares batter (B) & pitcher (P) to the
          * combined league norm (CL), then multiplies the combined ratio (X) times the combine
          * league norn (CL)
          */
         double CL = 0.5 * (BL + PL);
         if (B <= CL || P <= PL) {
            double X = (B / CL) * (P / CL);
            double ans = X * CL;
            if (ans < 1.0) return ans;
         }
         return 1.0 - Meld2(1.0 - B, 1.0 - BL, 1.0 - P, 1.0 - PL);
      }


      public void CombineLeagueMeans(CHittingParamSet mean0, CHittingParamSet mean1) {
         // -------------------------------------------------------------
         // #2101.01: This appears to be obs, no refs
         // Combine the means for the 2 teams into a single set...
         this.h = (mean0.h + mean1.h) / 2.0;
         this.b2 = (mean0.b2 + mean1.b2) / 2.0;
         this.b3 = (mean0.b3 + mean1.b3) / 2.0;
         this.hr = (mean0.hr + mean1.hr) / 2.0;
         this.so = (mean0.so + mean1.so) / 2.0;
         this.bb = (mean0.bb + mean1.bb) / 2.0;
         this.oth = 1.0 - (this.h + this.so + this.bb);
         this.sb = (mean0.sb + mean1.sb) / 2.0;
         this.fPitBase = (mean0.fPitBase + mean1.fPitBase) / 2.0;
         this.fSacBunt = (mean0.fSacBunt + mean1.fSacBunt) / 2.0;

      }



      public void FillBatParas(CBatRealSet batStat, CHittingParamSet lgParam, char pType) {
         // ---------------------------------------------------------------
         // This is the core parameter calculator for batters.
         // 6/5'19: Added pType arg, '2'=pitcher, for doing pitchers a little diff,
         // mainly using separate league params, and different cred factor (k1)
         // ---------------------------------------------------------------

         double baseSB;
         int sf1, ibb1, cs1, so1, hbp1, sh1;

         // Values used for credibility... 1907.02
         const double CRED_MIN_1 = 200; //Fully credible pa's for batters
         const double CRED_MIN_2 = 35;  //Fully credible pa's for pitchers (as batters)
         const double CRED_MIN_SB = 25; //Fully credible steal attempts

         // ----------------------------------------------------------
         // pitParam:
         // This will be in lieu of lgParam, for pitchers. 
         // It is used only for low credibility pitchers.
         // We meat-ax this here, and possibly in future, more exact
         // methodology will be developed.
         // ----------------------------------------------------------
         var pitParam = new CHittingParamSet { h = 0.102, b2 = 0.146, b3 = 0.006, hr = 0.044, bb = 0.029, so = 0.422 };

         // Substitute for missing data. All other stats are
         // required...
         sf1 = batStat.sf >= 0 ? batStat.sf : (int)(FAC_SF * batStat.ab);
         ibb1 = batStat.ibb >= 0 ? batStat.ibb : (int)(FAC_IBB * batStat.bb);
         cs1 = batStat.cs >= 0 ? batStat.cs : (int)(FAC_CS * batStat.sb);
         so1 = batStat.so >= 0 ? batStat.so : (int)(FAC_SO * batStat.ab);
         hbp1 = batStat.hbp >= 0 ? batStat.hbp : (int)(FAC_HBP * batStat.ab);
         sh1 = batStat.sh >= 0 ? batStat.sh : (int)(FAC_SH * batStat.ab);


         // Compute plate appearances. Also baseSB...
         // Also compute credibility factors, k1 and k2...
         // complPct aded for partial seasons. --1907.03
         // --------------------------------------------------------
         var pa = batStat.pa switch {
            -1 => batStat.ab + batStat.bb + hbp1 + sf1 + sh1,
            _ => batStat.pa
         };

         // Remove ibb, hbp, sac bunts (successful + failed) from pa... 
         double adjPa = pa - ibb1 - sh1 * (1.0 + FAC_SACBUNT);

         // credMin is the pa threshhold for 100% credibility...
         double credMin = pType switch {
            '1' => (0.01 * lgParam.complPct) * CRED_MIN_1,
            '2' => (0.01 * lgParam.complPct) * CRED_MIN_2
         };

         // k1 is primary credibility factor
         // k2 is credibility factor for SB's...
         double k1 = adjPa < credMin ? adjPa / credMin: 1.0;

         baseSB = batStat.sb + cs1;
         credMin = (0.01 * lgParam.complPct) * CRED_MIN_SB;
         double k2 = baseSB < credMin ? baseSB / credMin : 1.0;

      // Now compute the parameters...
         switch (pType) {
            case '1': //batter
               h = Math.Round(k1 * (div0(batStat.h, adjPa) - ADJ_FOR_OTHER_HITS) + (1 - k1) * lgParam.h, 4);
               b2 = Math.Round(k1 * div0(batStat.b2, batStat.h) + (1 - k1) * lgParam.b2, 4);
               b3 = Math.Round(k1 * div0(batStat.b3, batStat.h) + (1 - k1) * lgParam.b3, 4);
               hr = Math.Round(k1 * div0(batStat.hr, batStat.h) + (1 - k1) * lgParam.hr, 4);
               bb = Math.Round(k1 * div0(batStat.bb - ibb1, adjPa) + (1 - k1) * lgParam.bb, 4);
               so = Math.Round(k1 * div0(so1, adjPa) + (1 - k1) * lgParam.so, 4);
               break;
            case '2': //pitcher (as batter)
               h = Math.Round(k1 * (div0(batStat.h, adjPa) - ADJ_FOR_OTHER_HITS) + (1 - k1) * pitParam.h, 4);
               b2 = Math.Round(k1 * div0(batStat.b2, batStat.h) + (1 - k1) * pitParam.b2, 4);
               b3 = Math.Round(k1 * div0(batStat.b3, batStat.h) + (1 - k1) * pitParam.b3, 4);
               hr = Math.Round(k1 * div0(batStat.hr, batStat.h) + (1 - k1) * pitParam.hr, 4);
               bb = Math.Round(k1 * div0(batStat.bb - ibb1, adjPa) + (1 - k1) * pitParam.bb, 4);
               so = Math.Round(k1 * div0(so1, adjPa) + (1 - k1) * pitParam.so, 4);
               break;
         }
         oth = 1.0 - (h + bb + so);
         sb = Math.Round(k2 * div0(batStat.sb, baseSB, lgParam.sb) + (1 - k2) * lgParam.sb, 4);

      // Break oth into 'out' categories..
         fo = oth * othSplits[0];
         ld = oth * othSplits[1]; 
         pu = oth * othSplits[2];
         gr = oth * othSplits[3];

      }


      public void FillLgParas(CBatRealSet batStat) {
      // ----------------------------------------------------------
      // This fills mLgPara.
         double adjPa, baseSB;
         int sf1, ibb1, cs1, so1, hbp1, sh1;

      // With mBatStat
      // Substitute for missing data. All other stats are
      // required...
         sf1 = batStat.sf >= 0 ? batStat.sf : (int)(FAC_SF * batStat.ab);
         ibb1 = batStat.ibb >= 0 ? batStat.ibb : (int)(FAC_IBB * batStat.bb);
         cs1 = batStat.cs >= 0 ? batStat.cs : (int)(FAC_CS * batStat.sb);
         so1 = batStat.so >= 0 ? batStat.so : (int)(FAC_SO * batStat.ab);
         hbp1 = batStat.hbp >= 0 ? batStat.hbp : (int)(FAC_HBP * batStat.ab);
         sh1 = batStat.sh >= 0 ? batStat.sh : (int)(FAC_SH * batStat.ab);

         // Comput plate appearances, ie, the 'pa'. Also baseSB...
         // Also compute credibility factors, k1 and k2...
         //pa = batStat.ab + batStat.bb - ibb1 + hbp1 + sf1 - (int)(sh1 * FAC_SACBUNT);
         adjPa = batStat.pa - batStat.ibb - batStat.sh * (1.0 + FAC_SACBUNT);  //Adjusted PA --1907.04
         baseSB = batStat.sb + cs1;

      // Now comput the parameters..
         h = Math.Round(div0(batStat.h, adjPa), 4) - ADJ_FOR_OTHER_HITS;
         b2 = Math.Round(div0(batStat.b2, batStat.h), 4);
         b3 = Math.Round(div0(batStat.b3, batStat.h), 4);
         hr = Math.Round(div0(batStat.hr, batStat.h), 4);
         bb = Math.Round(div0(batStat.bb, adjPa), 4);
         so = Math.Round(div0(so1, adjPa), 4);
         oth = 1.0 - (h + bb + so);
         sb = Math.Round(div0(batStat.sb, baseSB), 4);
 
      // Break oth into 'out' categories..
         fo = oth * othSplits[0];
         ld = oth * othSplits[1]; 
         pu = oth * othSplits[2];
         gr = oth * othSplits[3];

         // These are publicly exposed return values...
         fSacBunt = FAC_SACBUNT;
         fPitBase = Math.Round(adjPa / (batStat.ip3 + batStat.h + batStat.bb), 4);

         this.complPct = batStat.complPct; //1907.03
      }


      public void FillPitParas(CPitRealSet pitStat, CHittingParamSet lgPara) {
      // -------------------------------------------------------------------------
      // This is the core parameter calculator for pitchers.
         double hr1, adjBfp;

         const double CRED_MIN_BFP = 200; //--1907.02

         // With mPitStat
         hr1 = pitStat.hr >= 0 ? pitStat.hr : (int)(lgPara.hr * pitStat.h);

         // complPct added for partial seasons... --1907.03
         //bfp = (int)(lgPara.fPitBase * (pitStat.ip3 + pitStat.h + pitStat.bb));
         adjBfp = pitStat.bfp - pitStat.ibb - FAC_SH2 * pitStat.bfp; 
         double credMin = (0.01 * lgPara.complPct) * CRED_MIN_BFP;
         double k1 = adjBfp < credMin ? adjBfp / credMin : 1.0;

         h = Math.Round(k1 * (div0(pitStat.h, adjBfp) - ADJ_FOR_OTHER_HITS) + (1 - k1) * lgPara.h, 4);
         b2 = lgPara.b2;
         b3 = lgPara.b3;
         hr = Math.Round(k1 * div0(hr1, pitStat.h) + (1 - k1) * lgPara.hr, 4);
         bb = Math.Round(k1 * div0(pitStat.bb, adjBfp) + (1 - k1) * lgPara.bb, 4);
         so = Math.Round(k1 * div0(pitStat.so, adjBfp) + (1 - k1) * lgPara.so, 4);
         oth = 1.0 - (h + bb + so);

      // Break oth into 'out' categories..
         fo = oth * othSplits[0];
         ld = oth * othSplits[1]; 
         pu = oth * othSplits[2];
         gr = oth * othSplits[3];

      }

      /// <summary>
      /// This converts hr, be, b2, etc., to an array to be used by classses that 
      /// are application-specific-agnostic
      /// </summary>
      /// <remarks>
      /// This impliments an abstract method in CParamSet.
      /// This s/b a method not persisted storage, because cpara combines 
      /// values outide the classs.
      /// </remarks>
      /// ----------------------------------------------------------------
      public override double[] GetWidthArray() {

         const double virtualWidth = 1.0;
         double w1 = 0.0;
         double[] widths = new double[11];

         // First, compute the widths...
         w1 += widths[(int)TLR.hr] = h * hr * virtualWidth;
         widths[(int)TLR.b3] = h * b3 * virtualWidth;
         //if (widths[(int)TLR.b3] < 1) widths[(int)TLR.b3] = 1;
         w1 += widths[(int)TLR.b3];
         w1 += widths[(int)TLR.b2] = h * b2 * virtualWidth;
         w1 += widths[(int)TLR.b1] =
            h * virtualWidth
            - widths[(int)TLR.b2] - widths[(int)TLR.b3] - widths[(int)TLR.hr];
         w1 += widths[(int)TLR.bb] = (bb + hbp) * virtualWidth;
         w1 += widths[(int)TLR.so] = so * virtualWidth;

         var oth = virtualWidth - w1;
         w1 += widths[(int)TLR.fo] = othSplits[0] * oth;
         w1 += widths[(int)TLR.ld] = othSplits[1] * oth;
         w1 += widths[(int)TLR.pu] = othSplits[2] * oth;
         w1 += widths[(int)TLR.gr] = othSplits[3] * oth;          

         return widths;

      }


      /// <summary>
      /// Return a CDiceRoll containing the tLR and point-in-bracket.
      /// </summary>
      /// <remarks>This impliments an abstract method in CParamSet</remarks>
      /// -----------------------------------------------------------------
      public override CDiceRoll RollTheDice(Random rn1) {
         
         TLR tlr;
         double pib;

         double b1 = h * (1.0 - b2 - b3 - hr);
         double cum = 0.0;
         int result = 0;
       //double[] probs = {0.0, hr*h, b3*h, b2*h, b1, bb, oth, so};
         double[] probs = {0.0, hr*h, b3*h, b2*h, b1, bb, fo, ld, pu, gr, so};
         double r = rn1.NextDouble();

         for (int i = 1; i<=10; i++) {
            cum += probs[i];
            if (r <= cum) {result = i; break;}
         }
         if (result == 0) throw new Exception("Result not found in RollTheDice()");
         tlr = (TLR)(result);
         pib = (r - (cum - probs[result])) / probs[result];
         
         return new CDiceRoll(tlr, pib, r);
      }


      /// <summary>
      /// This is intended to be called from the action, <GetTlr>, handler.
      /// It must return a new CDiceRoll with its 3 elements: TLR, pib, and 
      /// point overall.
      /// </summary>
      /// ------------------------------------------------------------------
      public override CDiceRoll GetTlr(TLR tlr, Random rn1) {

         double[] probs = GetWidthArray();
         if ((int)tlr > probs.Length-1) throw new Exception("TLR too large: " + tlr.ToString());
         double overall = 0.0;
         for (int i=1; i<(int)tlr; i++) overall += probs[i];
         double pib = rn1.NextDouble();
         overall += probs[(int)tlr] * pib;
         return new CDiceRoll(tlr, pib, overall);

      }


      /// <summary>
      /// Return a Tuple containing the tLR and point-in-bracket.
      /// For stealing we must return 1,2, 3, or 4.
      /// </summary>
      /// <remarks>
      /// 1 and 2 are for errors, 3 and 4 are safe / out respectively.
      /// Probabilities differ depending on whether this is steal of home or not. 
      /// </remarks>
      /// 
      public CDiceRoll RollTheDice_Steal(Random rn1, bool home) {

         TLR tlr;
         double pib;

         int result = 0;
         double cum = 0.0;
         double prob = home ? sb / 5.0 : sb;
         double[] probs;
         double r = rn1.NextDouble();

         if (home)
         // Use these prob's for stealing home. Very crude, we devide the 'sb'
         // parameter by 5...
            probs = new double[] { 0.0, 0.04, 0.04, sb / 5.0, 1.0 - 0.08 - sb / 5.0 };
         else
         // Other than home, just use sb...
            probs = new double[] { 0.0, 0.025, 0.025, sb, 1.0 - 0.05 - sb };

         for (int i = 1; i <= 4; i++) {
            cum += probs[i];
            if (r <= cum) {result = i; break; }
         }
         if (result == 0) throw new Exception("Result not found in RollTheDice_Steal()");
         tlr = (TLR)(result);
         pib = (r - (cum - probs[result])) / probs[result]; 

         return new CDiceRoll(tlr, pib, cum);
      }


      public double div0(double n, double d) {
      // -----------------------------------------------------
         if (d == 0.0) return 0.0;
         else return n / d;
      }


      public double div0(double n, double d, double def) {
      // -----------------------------------------------------
         if (d == 0.0) return def;
         else return n / d;
      }

   }   
      


}


