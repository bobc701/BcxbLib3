namespace BCX.BCXB {

   public struct TextToSay {
   // -----------------------
      public char action;
      public string msg;
      public string delim;
      public bool delay;
      public TextToSay(char action1, string msg1, string delim1, bool delay1) {
         action = action1;
         msg = msg1;
         delim = delim1;
         delay = delay1;
      }
   }

   public struct StatToUpdate {
   // -----------------------
   // #1604.02
      public char action;
      public int newValue;
      public StatCat sc;
      public int ab;
      public int ix;

      public StatToUpdate(char action1, int newValue1, StatCat sc1, int ab1, int ix1) {
         action = action1; //B or P
         newValue = newValue1;
         sc = sc1;
         ab = ab1;
         ix = ix1;
      }
   }


   public enum TAction
   {
      listdef = 1,
      DoOne = 2,
      Select = 3,
      Do = 4,
      DItem = 5,
      Say = 6,
      Say1 = 7,
      Adv = 8,
      BatDis = 9,
      Err = 10,
      Pos = 11,
      GPlay = 12,
      GPlayS = 13,
      Choose = 14,
      Same = 15,
      SacBunt = 16,
      SSqueeze = 17,
      Homer = 18,
      GRes = 19,
      SItem = 20,
      DoOneIx = 21,
      CalcTlr = 22,
      CalcTlrSteal = 23,
      CalcTlrStealHome = 24,
      GetTlr = 25,
      Endlistdef = 31,
      EndDItem = 35,
      EndDoOne = 32,
      EndSelect = 33,
      EndDoOneIx = 36,
   };


   public enum side {vis=0, home=1};

   public enum PLAY_STATE {
      START = 0,
      NEXT = 1,
      PLAY = 2,
      NONE = -1,
      OVER = -2
   }

   // Special play lists stored in GTAB:
   //    8-Steal, 9-Steal Home, 10-Sac Bunt, 11-Squeeze, 12-Walk(IP)

   public enum SPECIAL_PLAY {
      AtBat = 0,
      Steal = 1,
      Bunt = 2,
      IP = 3
   }


   public enum StatCat {
      ab, h, rbi, r, b2, b3, hr, k, bb, ip, er
   };


   public struct CRunner {
   // -----------------
      public int ix, resp;
      public string name;
      public char stat;

      public void Copy(CRunner r) {
      // ---------------------------------------------------
         ix = r.ix;
         resp = r.resp;
         name = r.name;
         stat = r.stat;
      }

      public void Clear() {
      // ---------------------------------------------------
         ix = 0;
         resp = 0;
         name = "";
         stat = ' ';
      }
   }

   public struct CBatRealSet {
   // ====================================================================
   // ip included here so CBatRealSet can be reused for league-level stats.      
      public int ab, hr, bi, sb, cs, h, b2, b3, bb, so;
      public int sf, ibb, hbp, sh, pa, ip3;
      public double ave;
      public int complPct; //new 1907.03

      //public int pa => pa == 0 ? ab + bb + hbp + sh + sf; }
   }

   public struct CBatBoxSet {
      // -----------------------------------------------------------------

      public string boxName;
      public int bx; //1906.02
      public int ab, r, h, bi, b2, b3, hr, so, bb, sb, cs;

   // These property gets are needed for binding in Xamarin Forms...
      public string BoxName { get { return boxName; } }
      public int Ab { get { return ab; } }
      public int R { get { return r; } }
      public int H { get { return h; } }
      public int Bi { get { return bi; } }
      public int B2 { get { return b2; } }
      public int B3 { get { return b3; } }
      public int Hr { get { return hr; } }
      public int So { get { return so; } }
      public int Bb { get { return bb; } }
      public int Sb { get { return sb; } }
      public int Cs { get { return cs; } }


      public static CBatBoxSet operator+(CBatBoxSet bs1, CBatBoxSet bs2) {
      // ------------------------------------------------------
         bs1.ab += bs2.ab;
         bs1.r += bs2.r;
         bs1.h += bs2.h;
         bs1.bi += bs2.bi;
         bs1.hr += bs2.hr;
         bs1.b2 += bs2.b2;
         bs1.b3 += bs2.b3;
         bs1.bb += bs2.bb;
         bs1.so += bs2.so;
         bs1.sb += bs2.sb;
         bs1.cs += bs2.cs;
         return bs1;
      }

   }


   public struct CPitRealSet {
   // -----------------------------------------------------------------
      public int g, gs, w, l, sv, bfp, er, h, hr, so, bb, ibb, ip3;
      public double era, whip, ip;
   }


   public struct CPitBoxSet {
   // -----------------------------------------------------------------
      public string boxName;
      public int bx, px; //1906.02
      public int ip3, r, h, er, so, bb, hr;

   // These property gets are needed for binding in Xamarin Forms...
      public string BoxName { get { return boxName; } }
      //public string Ip3 { get { return IpDisplay(ip3); } } //Use 'IpDisplay' for this...
      public int R { get { return r; } }
      public int H { get { return h; } }
      public int Er { get { return er; } }
      public int So { get { return so; } }
      public int Bb { get { return bb; } }
      public int Hr { get { return hr; } }

      public static CPitBoxSet operator+(CPitBoxSet ps1, CPitBoxSet ps2) {
      // -----------------------------------------------------------------
         ps1.ip3 += ps2.ip3;
         ps1.r += ps2.r;
         ps1.h += ps2.h;
         ps1.er += ps2.er;
         ps1.so += ps2.so;
         ps1.bb += ps2.bb;
         ps1.hr += ps2.hr;
         return ps1;
      }

      public string IpDisplay {
         // ------------------------------------------------------
         // Converts ip3 to display value with .1 for 1/3 inn, etc.
         get {
            int n = ip3 % 3;
            int ip = ip3 / 3;
            switch (n) {
               case 0: return ip.ToString();
               case 1: return ip.ToString() + ".1";
               case 2: return ip.ToString() + ".2";
               default: return ip.ToString();
            }
         }

      }

   }


   public struct CDiceRoll {
       public TLR topLevelResult;
       public double pointInBracket;
       public double pointOverall;

       public CDiceRoll(TLR tlr, double pib, double po) {
          topLevelResult = tlr;
          pointInBracket = pib;
          pointOverall = po;
       }
   }


   public class CBatter {
   /* -----------------------------------------------------------------------
    * I tried to decouple CBatter from CGame, and in a few places I was able to 
    * remove the dependancy. However, it is the set of properties, like OnWhichBase, 
    * DisplayPos, IsAtBat, etc., that depends on CGame -- and you can't replace
    * them with method calls that supply an arg, because of how these are used
    * for binding in forms. (As far as I know... they just have to be properties 
    * with no args.
    * But if your not using CBatter in a binding context, you can just ignore
    * the CGame, let it be null, and just don't use those properties.
    * -----------------------------------------------------------------------
    */
  
      public CGame g;
      public string bname, bname2;
      public CBatRealSet br;
      public CBatRealSet lgBr; //new #2101.01
      public CBatBoxSet bs;
      public int when, where;
      public bool used = false;
      public int bbox;
      public CHittingParamSet par = new();
      public CHittingParamSet lgPar = new(); //new #2101.01
      public string skillStr = "---------";
      public int bx; //Should match index in bat
      public int px; //Index of this player in pitcher arrays
      public side sidex;

      public CBatter(CGame g1) { 
      // -----------------------------------------------------------------
         g = g1;
      }

      public static string[] posAbbr =
         {"", "p", "c", "1b", "2b", "3b", "ss", "lf", "cf", "rf", "dh"};
      public static string[] baseName = { "ab", "1st", "2nd", "3rd" };

   // The following properties just present data available in other ways as 
   // properties, which is usefull in front ends that use properties for data
   // binding, like DataGridView in VS.

      
      private int OnWhichBase {
      // --------------------------------------------------------------------
         get {
            if (g == null) return 0;
            if ((int)sidex == g.ab) {
            // The side he's on is up...
               if (bx == g.r[1].ix) return 1;
               else if (bx == g.r[2].ix) return 2;
               else if (bx == g.r[3].ix) return 3;
               else return 0;
            }
            else { 
            // Th side he's on is not up, so he can't be on base...
               return 0;
            }
         }
      }


      private bool IsAtBat {
      // ------------------------------------------------------------------
         get {
            if (g == null) return false;
            if ((int)sidex == g.ab) {
            // The side he's on is up...
               CTeam t1 = g.t[g.ab];
               return (bx == t1.linup[t1.slot]);
            }
            else {
            // The side he's on is not up, so he can't be up...
               return false;
            }
         }
      }
      
      public string DisplayName {
         get {return bname; } 
      }

      public string DisplaySlot {
         get {return when == 0? "": when.ToString(); }
      }

      public string DisplayPos {
         get { return posAbbr[where]; }
      }

      public string DisplayBase {
         get { 
            if (IsAtBat) return "ab";
            else if (OnWhichBase != 0) return baseName[OnWhichBase];
            else return "";
         }
      }
      
      public string DisplaySkill {
         get {
            string s = this.skillStr;
            string s1 = "";
            string delim = "";
            for (int i=0; i<=8; i++) 
               if (s[i] != '-') {
                  s1 += delim + posAbbr[i+1] + ':' + s[i];
                  delim = ", ";
               }
            return s1;
         }
      }

      public string DisplayEligiblePosns {
         get {
            string s = this.skillStr;
            string s1 = "";
            string delim = "";
            for (int i=0; i<=8; i++) 
               if (s[i] != '-') {
                  s1 += delim + posAbbr[i+1];
                  delim = ", ";
               }
            return s1;
         }
      }


      public void Copy(CBatter b1)
      {
         // --------------------------------------------------------------
         bname = b1.bname;
         bname2 = b1.bname2;
         br = b1.br; //Note: we can use a shallow copy here.
         bs = b1.bs; //Shallow copy
         when = b1.when;
         where = b1.where;
         used = b1.used;
         bbox = b1.bbox;
         par = b1.par; //Shallow copy;
         skillStr = b1.skillStr; //BTW, does this work in Java?
         px = b1.px;
         bx = b1.bx;
         sidex = b1.sidex;
      }

   }


   public class CPitcher {
      // -----------------------------------------------------------------
      public string pname, pname2;
      public CPitRealSet pr;
      public CPitRealSet lgPr; //New #2101.01
      public CPitBoxSet ps;
      public int pbox;
      public CHittingParamSet par = new();
      public CHittingParamSet lgPar = new(); //New #2101.01

      public int px; //Should match index in pit.
      public side sidex; //Tells  whichteam he's on, 0 or 1

      public void Copy(CPitcher p1) {
      // --------------------------------------------------------------
         pname = p1.pname;
         pname2 = p1.pname2;
         pr = p1.pr;   //Note: we can use a shallow copy here.
         ps = p1.ps;   //Shallow copy
         pbox = p1.pbox;
         par = p1.par; //Shallow copy;
         px = p1.px;
         sidex = p1.sidex;
      }
   }


   // These delegates will be used for screen updating events...  
      //public delegate void DShowState1();
      //public delegate void DShowState2(int ab);
      //public delegate void DShowState3(int n, StatCat sc, int ab, int i);
      //public delegate void DShowState4(int skill, string labels);
      //public delegate void DMovePointer(double r, TLR res, bool visible);
      //public delegate void DNotifyUser(string s);


//public interface IGameController {
//// -----------------------------------------------------------------

//    void ShowResults();
//    void ShowRunners();
//    void ShowFielders(int fl);
//    void PostOuts();
//    void UpdateBBox(int n, StatCat sc, int ab, int i);
//    void UpdatePBox(int n, StatCat sc, int ab, int i);
//    void RefreshBBox(int ab);
//    void RefreshPBox(int ab);
//    void ShowLinescore();
//    void ShowLinescoreFull();
//    void InitLinescore();
//    void ShowRHE();
//    void FmtParamBar();
//    void MoveDicePointer(double r, bool visible);

//}

}