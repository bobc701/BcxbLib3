using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using BCX.SimEngine;
using BCX.BCXCommon;


namespace BCX.BCXB {
   
/// <summary>
/// This encapsulates the state of a specific game, including 
/// outs, on-base situation, score, line-ups, box score stats
/// etc.ƒ
/// </summary>
/// 
public class CGame {

      /// <summary>
      /// The whole model shares this random number generator object...
      /// It is instantiated in the CGame's constructor.
      /// </summary>
      public System.Random rn;

      public CTeam[] t; // = new CTeam[2]; 

      /// <summary>
      /// CGame manages the contents of this list, and then on the
      /// client side, the client uses it to post to the play-by-play box.
      /// </summary>
      public List<TextToSay> lstResults = new List<TextToSay>();
      public List<StatToUpdate> lstBoxUpdates = new List<StatToUpdate>(); //#1604.02

      public enum RunMode {Normal=1, Auto=2, Fast=3, FastEog=4, FastEOP=5} //(Fast:SAY_INTERVAL=0)

      public event Func<short, StreamReader> ERequestModelFile;
      public event Func<string, StreamReader> ERequestEngineFile;
      public event Func<string, TextReader> ERequestTeamFileReader;
      public event Func<string, StreamWriter> ERequestTeamFileWriter;
      public event Func<string, StreamWriter> ERequestBoxFileWriter;

      //public double topLevelProb; // The top level result of the dice roll determining 1b, 2b, etc.
      //public double pointInBracket; // Percent didtance into the top level bracket
      public CDiceRoll diceRollBatting;    // The top level result, 1..10, was 1..7.
      public CDiceRoll diceRollFielding; // TopLevelResult = 1 (TLR.GoodPlay)..2 (TLR.BadPlay)
      public int genericResult; // The index, 1..100, in grid, GTab.
      //public IGameController gc;

   // These are the screen updating events...
      public event Action<int> EShowResults;
      public event Action EClearResults;
      public event Action EUpdateBoxes;

      public event Action EShowRunners;
      public event Action EShowRunnersOnly;
      public event Action<int> EShowFielders;
      public event Action<int> ESelectBoxTabs;
      public event Action EPostOuts;
      //public event Action<int, StatCat, int, int> EUpdateBBox; //Out #1604.02
      //public event Action<int, StatCat, int, int> EUpdatePBox;
      public event Action<int> ERefreshBBox;
      public event Action<int> ERefreshPBox;
      public event Action EShowLinescore;
      public event Action EShowLinescoreFull; //Don't think this is actually used.
      public event Action EInitLinescore;
      public event Action EShowRHE;

      public event Action EFmtParamBar;
      public event Action<CFieldingParamSet, string, string> EFmtFieldingBar;

      public event Action<CDiceRoll, bool> EPlaceDicePointer;
      public event Action<CDiceRoll, bool> EPlaceFldgPointer;

      public event Action EHideDicePointer;
      public event Action<int, int> EHighlightBBox; //#1506.01
      public event Action<int, int> EHighlightPBox; //#1506.01
      public event Action<string> ENotifyUser;
       
      public SPECIAL_PLAY specialPlay = SPECIAL_PLAY.AtBat; //1:=Steal, 2:=Bunt, 3:=IP

      public const int SZ_BAT = 26; //We use 1..25, 0 is unused.
      public const int SZ_PIT = 12; //We use 1.11, 0 is unused.
      const int SZ_AB = 2;    
      const int SZ_POS = 10;   //1..9;
      const int SZ_SLOT = 10;  //1..9;
      const int SZ_LINESCORE = 31;    //1..30; //index in line score

      public CBatRealSet[] lgStats = new CBatRealSet[2];
      
   // Added in V1 to replace CASL screen objects
      public string results = ""; //This might be used for whole-game scroll. 

      public bool UsingDh = false;
      int ixList, ixRow, icCol;
      int limLists, limActions;   
      int go;
      int sit, up, posn, gplay;
      bool scoringPlay = false;

      public int PosLim => UsingDh ? 10 : 9;

      public static string[] PosName = {"field",
         "pitcher", "catcher", "first", "second", "third", "short",
         "left", "center", "right", "DH"};
      public static string[] PosNameLong = {"as fielder",
         "as pitcher", "as catcher", "at first base", "at second base", 
         "at third base", "at shortstop", "in left field", "in center field", 
         "in right field", "as DH"};
      public static string[] posAbbr =
         {"", "p", "c", "1b", "2b", "3b", "ss", "lf", "cf", "rf", "dh"};
      //string[] ordSuffix =
      //   {"th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th"};
      string[] aPosName =
         {"","p","c","1b","2b","3b","ss","lf","cf","rf","dh"};
      public static string[] baseName = { "ab", "1st", "2nd", "3rd" };
         
      int po;

      public string ordSuffix(int inn) {
         
         if (inn == 10) return "th";
         if (inn == 11) return "th";
         if (inn == 12) return "th";
         if (inn == 13) return "th";
         switch (inn % 10) {
            case 0: return "th"; 
            case 1: return "st"; 
            case 2: return "nd";
            case 3: return "rd"; 
            default: return "th"; 
         }

      }

      public CRunner[] 
         r = new CRunner[4],
         s = new CRunner[4];
      
   // "mean" is 2-dim array of CParamSet...
   // There are 3 CParamset objects: The 2-dim array mean holds vis & home, and cmean holds the 
   // average ofthe two.
      public CHittingParamSet cmean  = 
         new CHittingParamSet(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
      //public CHittingParamSet[] lgMean =
      //   {new CHittingParamSet(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0),
      //    new CHittingParamSet(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)};
      double[] fPitBase = new double[SZ_AB], fSacBunt = new double[SZ_AB];
      public const double fMean0 = 1.00; //Fudge factor for hits

   // Combined parameters -- computed at atbat time.    
   // These initial values are for testing only!
      public CHittingParamSet cpara = 
         new CHittingParamSet (0.250, 0.2, 0.05, 0.4, 0.1, 0.2, 0.55, 0.67, 0.0, 0.0, 0.0, 0.0); 

   // Foelding parameters -- computed at Choose time...
      public CFieldingParamSet fpara = null;

   // game control
   // ------------
      public int inn=1, ok=0, eok=0; 
      public int[,] rk = new int[SZ_AB, 3];
      //public int[,] lines = new int[SZ_AB, SZ_LINESCORE];
      public CLineScore lines; //= new CLineScore();
      string team1;
      string team2;
      bool eos = false;
      public bool eog = false;
      bool newgame = false;
      bool sameguy = false;
      bool homer = false;
      public int ab, fl;    
      

      public string CurrentBatterName {
      // ----------------------------------
         get {
            return t[ab].bat[t[ab].linup[t[ab].slot]].bname;
         }
      }


      public string CurrentPitcherName {
      // ----------------------------------
         get {
            return t[fl].pit[t[fl].curp].pname;
         }
      }

  
      string line1;
      public PLAY_STATE PlayState = PLAY_STATE.NONE;     
      bool NoPauseBeforePlay =false; //User sets & clears in menus
      string sym = "pk123slcr";

   // #1601.01: Added GetTlr, #37...
      string[] atName ={ "",
        "Listdef", "DoOne", "Select", "Do", "DItem",  
        "Say", "Say1", "Adv", "BatDis", "Err", 
        "Pos", "GPlay", "GPlayS", "Choose", "Same", 
        "SacBunt",  "SSqueeze",  "Homer", "GRes", "SItem", 
        "DoOneIx", "CalcTlr", "CalcTlrSteal", "CalcTlrStealHome", "25", 
        "26", "27", "28", "29", "30", 
        "Endlistdef", "EndDoOne", "EndSelect", "34", "EndDItem", 
        "EndDoOneIx", "GetTlr"};
      
      string[] aSay;
      int[,] gres = new int[101,16];  // array[1..100, 0..15] of integer;
      
   // #1601.01: Added GetTlr, #25, len=2...
      int[] atLen = {0,
         0, 1, 1, 3, 3,   3, 3, 3, 2, 3,
         1, 2, 3, 4, 1,   1, 1, 1, 3, 4,
         1, 1, 1, 1, 2,   0, 0, 0, 0, 0,
         1, 1, 1, 0, 1,   1};

   // These were in CFMain...
      const double SAY_INTERVAL = 1.0; //Number of seconds between 'say's'.   
      double timeOfPriorPost = 0.0;

      public RunMode runMode = RunMode.Normal;
      public bool IsFastRunMode {
         get { return runMode == RunMode.Fast || runMode == RunMode.FastEog; }
      }

      char[] aArg2 = {'e','u','d'};
      char[] aArg3 = {'r','d','x'};

      string[] halfInn = {"Top of ", "Bottom of "};
      CEngine mEng;


      /// <summary>
      /// Constructor
      /// Main thing it doeis instantiate Cengine object, mEng, by
      /// reading model files. And assigns mEng's EDoAction event handler.
      /// </summary>
      /// 
      public CGame() {
      // ------------------------------------------------

      // The whole model will share this instance of the random number 
      // generator. This form of the constructor uses a time-dependant
      // seed value, accoring to the docs.
         rn = new Random();         

         PlayState = PLAY_STATE.NONE;
         //InitParambar;
         //InitParamBar15;
         if (PlayState == PLAY_STATE.NONE) {
            //Remove this msg because it appears before the form is displayed...
            //MessageBox.Show("Click 'New Game' to select home & visiting teams.");
            //EShowResults(); You can't raise event in the constructor!
         }
         
	   }




      public void SetupEngineAndModel() {
      // -----------------------------------------------------------------------
      // Can't do this in the constructor because the event handlers need to
      // be instantiated first.

      // Instantiate the engine object, mEng,  
         var fEngine = ERequestEngineFile("cfeng1");
         mEng = new CEngine(fEngine);
         mEng.EDoAction += new DDoAction(DoAction);
         mEng.EEngineError += delegate (int n, string list) {
            ENotifyUser?.Invoke("Exception in DoList in CEngine: " + n + ": " + list);
         };
         
         mEng.atLen = atLen;
      // Read the model files (CFEng1 and CFEng3)...
         ReadModel();

      }


      public void ResetData() {

         
      }


      public int BBoxLim(int ab) {
      // ---------------------------
      // Returns the highest (used) element in batters' box score...
         int ix;
         for (ix=9; ix<CGame.SZ_BAT && t[ab].xbox[ix]!= 0; ix++);
         return ix-1;
      }


      public int PBoxLim(int fl) {
      // ---------------------------
      // Returns the highest (used) element in pitchers' box score...
         int ix;
         for (ix=1; ix<CGame.SZ_PIT && t[fl].ybox[ix]!= 0; ix++);
         return ix-1;
      }


      void flushrnrs(CRunner[] rnr) {
      // ----------------------------------------------
         for (int i=0; i<=3; i++) rnr[i].Clear();
      }


      /// <summary>
      /// This is the event handler for the CEngine's EDoAction event.
      /// </summary>
      /// 
      private void DoAction (ref int at, int n, string sList, ref int p, string lvl) {
      // --------------------------------------------------------------------------
      // This used to be inside the DoList loop!
      // This is the event handler for the EDoAction event.
      // Error handling added. #1706.20

         Debug.WriteLine (lvl + "DoAction(at={0}({1}), n={2}, p={3}", at, atName [at], n, p);

         int a, a0, a1, a2, a3;
         int choice;
         double r, cum;
         bool done = false;
         double prob = 0.0;

         // These masks fetch the right-most 3-bit numbers of a numeric value...
         const int MASK_1 = 7;
         const int MASK_2 = 56;
         const int MASK_3 = 448;
         const int MASK_4 = 3584;

         try {

            //MessageBox.Show("EHDoAction " + at.ToString() + ", " + sList);
            //TAction at = (TAction)at0;
            switch ((TAction)at) {

            case TAction.GetTlr:
               a = CEngine.Decoded (sList [p + 1]);
               diceRollBatting = cpara.GetTlr ((TLR)a, rn);
               Debug.WriteLine (
                  lvl + "  GetTlr: pointInBracket={0:#0.0000}, topLevelResult={1}",
                  diceRollBatting.pointInBracket, diceRollBatting.topLevelResult);
               break;

            case TAction.CalcTlr:
               diceRollBatting = cpara.RollTheDice (rn);
               mEng.DoOneIndex = (int)diceRollBatting.topLevelResult;
               Debug.WriteLine (
                  lvl + "  CalcTlr: pointInBracket={0:#0.0000}, topLevelResult={1}",
                  diceRollBatting.pointInBracket, diceRollBatting.topLevelResult);
               break;


            case TAction.CalcTlrSteal:
               diceRollBatting = cpara.RollTheDice_Steal (rn, home: false);
               mEng.DoOneIndex = (int)diceRollBatting.topLevelResult;
               Debug.WriteLine (
                  lvl + "  CalcTlrSteal: pointInBracket={0:#0.0000}, topLevelResult={1}",
                  diceRollBatting.pointInBracket, diceRollBatting.topLevelResult);
               break;

            case TAction.CalcTlrStealHome:
               diceRollBatting = cpara.RollTheDice_Steal (rn, home: true);
               mEng.DoOneIndex = (int)diceRollBatting.topLevelResult;
               Debug.WriteLine (
                  lvl + "  CalcTlrStealHome: pointInBracket={0:#0.0000}, topLevelResult={1}",
                  diceRollBatting.pointInBracket, diceRollBatting.topLevelResult);
               break;

            case TAction.DoOneIx:
               Debug.WriteLine (lvl + "  at=DoOneIx: n={0}, DoOneIndex={1:#0.0000}", n, mEng.DoOneIndex);
               while (!done) {
                  while ((TAction)at != TAction.DItem) {
                     p += atLen [at];
                     at = CEngine.Decoded (sList [p]);
                  }
                  a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);
                  if (a0 == mEng.DoOneIndex) {
                     done = true;
                     Debug.WriteLine (lvl + "  Done: p = " + mEng.DoOneIndex);
                  } else {
                     p += atLen [(int)TAction.DItem];
                     at = CEngine.Decoded (sList [p]);
                     Debug.WriteLine (lvl + "  Not Done: p={0}, at={1}({2})", p, at, atName [at]);
                  }
               }
               break;

            case TAction.DoOne:
               // Scan for the qualifying DItem...
               r = rn.NextDouble ();
               Debug.WriteLine (lvl + "  at=DoOne: n={0}, r={1:#0.0000}", n, r);

               cum = 0.0;
               done = false;
               while (!done) {
                  while ((TAction)at != TAction.DItem) {
                     p += atLen [at];
                     at = CEngine.Decoded (sList [p]);
                  }
                  a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);

                  // ---------------------------------------------------------------------------
                  // Stuff reinserted from old version, 1/29*15. 
                  // There was a lot of other stuff having to do with TLR, but that's not needed
                  // now since we will have <CalcTlr> action to get TLR.
                  // ---------------------------------------------------------------------------
                  // This section converts the arg, a0, to 'prob'...
                  //
                  if (a0 <= 1000)
                     prob = 0.001 * a0;
                  else {
                     // This is a numbered parameter.
                     // For these, probs are not hard coded in model, they must be 
                     // supplied by the client application...
                     // i= slot[ab]; j = curp[1-ab];
                     switch (a0) {
                     case 1001: prob = cpara.h; break;
                     case 1002: prob = cpara.b2; break; // double
                     case 1003: prob = cpara.b3; break; // triple
                     case 1004: prob = cpara.hr; break; // home run
                     case 1005: prob = cpara.bb; break; // bb
                     case 1006: prob = cpara.so; break; // strike out
                     case 1007: prob = cpara.sb; break; // steal (Used in list, "Steal")
                     case 1101: prob = cpara.sb - .05; break; // .05 is from model list, "Steal"
                     case 1008: prob = cpara.sb / 5; break; // steal home (Used in list, "StealHome")
                     case 1102: prob = cpara.sb / 5.0 - .08; break; // .08 is from model list, "StealHome"
                     case 1099: prob = 1.0 - cum; break; // was cpara.oth; break; // other N1706.19
                     case 1098: prob = 1.0 - (cpara.b2 + cpara.b3 + cpara.hr); break; // single
                     }
                  }
                  //  prob = 0.001 * a0;
                  // ----------------------------------------------------- End reinserted stuff

                  cum += prob;
                  if (r <= cum) {
                     done = true;
                     Debug.WriteLine (lvl + "  Done");
                  } else {
                     p += atLen [(int)TAction.DItem];
                     at = CEngine.Decoded (sList [p]);
                     Debug.WriteLine (lvl + "  Not Done: p={0}, at={1}({2})", p, at, atName [at]);
                  }
               }
               break;

            case TAction.Do:
               a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);
               Debug.WriteLine (lvl + "  at=Do: Will call DoList({0})", a0);
               mEng.DoList (a0, lvl + "  ");
               break;

            case TAction.DItem:
               //          Scan for the ending EndDoOne or EndDoOneIx...
               Debug.WriteLine (lvl + "  at=DItem");
               while (!((TAction)at == TAction.EndDoOne || (TAction)at == TAction.EndDoOneIx)) {
                  p = p + atLen [at];
                  at = CEngine.Decoded (sList [p]);
               }
               break;

            case TAction.Say:
               a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);
               Debug.WriteLine (lvl + "  at=Say: a0={0}, {1}", a0, aSay [a0]);
               Say (aSay [a0]);
               break;

            case TAction.Say1:
               if (ok < 2) {
                  a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);
                  Debug.WriteLine (lvl + "  at=Say1: a0={0}, {1}", a0, aSay [a0]);
                  Say (aSay [a0]);
               }
               break;

            case TAction.Adv:
               a = CEngine.Decoded (sList [p + 1], sList [p + 2]);
               a0 = (int)Math.Floor ((double)(a & MASK_4) / 512);
               a1 = (int)Math.Floor ((double)(a & MASK_3) / 64);
               a2 = (int)Math.Floor ((double)(a & MASK_2) / 8);
               a3 = a & MASK_1;
               Debug.WriteLine (lvl + "  at=Adv: {0}--> Will call Advance({1}, {2}, {3}, {4})", a, a0, a1, a2, a3);
               Advance (a0, a1, aArg2 [a2], aArg3 [a3]);
               break;

            case TAction.BatDis:
               a = CEngine.Decoded (sList [p + 1]);
               BatDis (a);
               Debug.WriteLine (lvl + "  at=BatDis({0})", a);
               break;

            case TAction.Err:
               a = CEngine.Decoded (sList [p + 1]);
               err1 (a);
               Debug.WriteLine (lvl + "  at=Err: {0}", a);
               break;

            case TAction.Pos:
               //          The 'Pos...' list adddresses are held in GTAB/gres row 99...
               posn = SelectList (gres [99, gplay]);
               Debug.WriteLine (lvl + "  at=Pos--> posn={0}", posn);
               break;

            case TAction.GPlay:
               a = CEngine.Decoded (sList [p + 1]);
               gplay = a;
               Debug.WriteLine (lvl + "  at=GPlay: {0}--> gplay={1}", a, gplay);
               break;

            case TAction.GPlayS:
               a = CEngine.Decoded (sList [p + 1], sList [p + 2]);
               gplay = SelectList (a);
               Debug.WriteLine (lvl + "  at=GPlayS: {0}--> gplay={1}", a, gplay);
               break;

            case TAction.Choose:
               a0 = CEngine.Decoded (sList [p + 1]);
               a1 = CEngine.Decoded (sList [p + 2]);
               a2 = CEngine.Decoded (sList [p + 3]);
               choice = choose (a0, a1, a2);
               a = gres [choice, onsit ()];
               Debug.WriteLine (lvl + "  at=Choose ({0}, {1}, {2})--> Will call DoList({3})", a0, a1, a2, a);
               mEng.DoList (a, lvl + "  ");
               break;

            case TAction.Same: sameguy = true; break;

            case TAction.SacBunt: Debug.WriteLine (lvl + "  at=SacBunt"); break;
            case TAction.SSqueeze: Debug.WriteLine (lvl + "  at=SSqueeze"); break;
            case TAction.Homer: homer = true; Debug.WriteLine (lvl + "  at=Homer"); break;

            case TAction.GRes:
               a0 = CEngine.Decoded (sList [p + 1], sList [p + 2]);
               genericResult = a0;
               a = gres [a0, onsit ()];
               Debug.WriteLine (lvl + "  at=GRes: {0}, {1}--> Will call DoList({2})", a0, onsit (), a);
               mEng.DoList (a, lvl + "  ");
               break;

            case TAction.SItem:
            // Should not happen here
               throw new Exception ("Did not expect TAction.SItem in DoList");
            }

         } catch (Exception ex) { //Error handling added. #1706.20
            string msg =
              "Exception in DoAction: " + ex.Message + "\r\n" +
              "n = " + n + "\r\n" +
              "list = " + sList + "\r\n" +
              "at = " + at;
            Debug.WriteLine (msg);
            ENotifyUser?.Invoke(msg);
         }

      } 
      
      
      public string gname(int p) {
      // ---------------------------------}
         switch (p) {
            default: return "Ball hit";
            case 1: return "Pop up";
            case 2: return "Foul pop";
            case 3: return "Grounder";
            case 4: return "Fly ball";
            case 5: return "Line drive";
            case 6: return "Line drive";
            case 7: return "Long fly ball";
         }
      }
      
      
      private int choose(int nGood, int nBad, int p) {
      // ---------------------------------------------------------------
      // n1 is the favorable result (for fielder) to  n2 unfavorable.
      // p is position (1..9)
      // howgood:=0: n2 always chosen, :=3: 50/50, :=6: n1 always chosen.
      // ---------------------------------------------------------------
      // In BCX 1.0, we do not use howgood -- instead we call skill() 
      
         int s; //double r;
         string txt;
         
         if (p==0) p= posn;
         s = skill(p, t[fl].bat[t[fl].who[p]].skillStr);
         if (s < 0) s = 0;
         if (s > 6) s = 6;
         //r = rn.NextDouble();

         if (nGood == 7 && nBad == 6) txt = "out/safe stretching";
         else if (nGood==1 && nBad==2) txt = "adv 1 base/adv 2 bases";
         else if (nGood==12 && nBad==15) txt = "holds at 3rd/scores";
         else if (nGood==31 && nBad==37) txt = "no adv/advance";
         else if (nGood==33 && nBad==13) txt = "out/double";
         else if (nGood==31 && nBad==35) txt = "out/error";
         else if (nGood==40 && nBad==4) txt = "out/single";
         else if (nGood==47 && nBad==48) txt = "dp/lead forced";
         else if (nGood==54 && nBad==55) txt = "out/single";
         else if (nGood==54 && nBad==43) txt = "out/error";
         else txt = nGood.ToString() + "/" + nBad.ToString();
         
         int res;

         fpara = new CFieldingParamSet(s);
         diceRollFielding = fpara.RollTheDice(rn);
         if (diceRollFielding.topLevelResult == TLR.GoodPlay) res = nGood;
         else res = nBad;
         
         EFmtFieldingBar?.Invoke(fpara, txt, t[fl].bat[t[fl].who[p]].bname);
         EPlaceFldgPointer?.Invoke(diceRollFielding, true);
         fpara.Description = txt; //This sets 'description' and parses it into 2 'SegmentLabel's.
         fpara.fielderName = t[fl].bat[t[fl].who[p]].bname;
         fpara.fielderSkill = t[fl].bat[t[fl].who[p]].DisplaySkill;
         return res;
      }      


      private int onsit() {
      // ---------------------------------------------
         int o= 1;
         if (r[1].ix > 0) o+= 1;
         if (r[2].ix > 0) o+= 2;
         if (r[3].ix > 0) o+= 4;
         if (o==5) o= 4; else if (o==4) o= 5;
         if (ok==2 && o>1) o+= 7;
         return o;
      }
      
      
      private int onsit2() {
      // ---------------------------------------------
      // This version of onsit ignores outs and just returns
      // combination of 1, 2, and 4.

         int o= 0;
         if (r[1].ix > 0) o+= 1;
         if (r[2].ix > 0) o+= 2;
         if (r[3].ix > 0) o+= 4;
         return o;
      }


      private void punch(string s, int newLine, bool pause) {
      // -------------------------------------------------------------
      // V1: Replaced txtResults.txt with new global var: results
      // DELPHI: I tried char(10) and char(13) for delim, but only both worked.
      //
      // Delim:
      //   0: Put 1 space after any existing text
      //   1: Put ' -- ' after. However it knows not to put '--' after '...'
      //   2: Put CRLF after
      // -------------------------------------------------------------}
         string delim;

         switch (newLine) {
            case 1: delim = " -- "; break;
            case 2: delim = "\r\n"; break;
            default: delim = ""; break; // 4/25'17 was " "
         }
         //if (results == "") 
         //   results = s;
         //else {
         //   if ((results[results.Length-1] == '.') && (newLine != 2)) delim = " ";
         //   results = results + delim + s;
         //}
         ShowResults(s, delim, pause);
      }


      /// <summary>
      /// Here we can do any game-side actions, before raising the event to the 
      /// client. This might consist of accumulating a whole-game scroll or 
      /// something.
      /// </summary>
      /// 
      private void ShowResults(string msg, string delim, bool delay) {

         //if (delay) DelayResults();
         //EShowResults(msg, delim);

      // New approach, #1510.01.
      // We just add to the List<> i/o raising event.
      // First char of the msg indicates if we want delay or not.
      // #1512.02: Put ?: in delay arg...
         lstResults.Add(new TextToSay('S', msg, delim, delay));

      }


      public string SerializeResults() {
      // ---------------------------------------------------------
      // Will return string like:
      // S|Fly to left|\r\n|T|S|Base hit|||F|X||F| ...etc...
         string s = "";
         foreach (var t in lstResults) {
            s +=
               t.action.ToString() + "|" + t.msg.ToString() + "|" + t.delim.ToString() + "|" +
               t.delay.ToString()[0] + "|";
         }
         return s;
      
      }


      private void ClearResults() {
         //DelayResults(); //#150715: Out: Don't pause when clearing results.
         //EClearResults();

         // New approach, #1510.01.
         // We just clear the List<> i/o raising event.
         lstResults.Add(new TextToSay('X', "", "", false));

      }

      private int pick(int n1, int n2, int r0) {
      // ---------------------------------------------
         int r;
         r = (int)Math.Round(1000.0 * rn.NextDouble());  
         if (r < r0) return n1; else return n2;
      }


      private int piv(int posn) {
      // ----------------------------------
         switch (posn) {
            case 6: return 4;
            case 5: return pick(6, 4, 500);
            case 1: return pick(6, 4, 500);
            case 3: return 6;
            case 2: return 6;
            default: return 4;
         }
      }   
         
         
      private void Say(string line1) {
      // ------------------------------

         string sym, exp1 = "", line;
         int n, bNewLine;
         ///double t;

         line = line1;
         bNewLine = 2;
         if (line == "NULL") line = "..."; //#1507.B01
         if (line[0] == '@') {bNewLine= 0; line = line.Remove(0,1);}

         CTeam t1 = t[fl];
         n = line.IndexOf("*"); 
         while (n >= 0) {
            sym = line.Substring(n+1, 2);
            if ((posn < 1) || (posn > 9)) posn = 6;
            if (sym == "r0") exp1 = r[0].name;
            else if (sym == "r1") exp1 = r[1].name;
            else if (sym == "r2") exp1 = r[2].name;
            else if (sym == "r3") exp1 = r[3].name;
            else if (sym == "fp") exp1 = t1.bat[t1.who[posn]].bname;
            else if (sym == "pl") exp1 = gname(gplay);
            else if (sym == "fl") exp1 = t1.bat[t1.who[1]].bname;
            else if (sym == "f2") exp1 = t1.bat[t1.who[2]].bname;
            else if (sym == "f3") exp1 = t1.bat[t1.who[3]].bname;
            else if (sym == "f4") exp1 = t1.bat[t1.who[4]].bname;
            else if (sym == "f5") exp1 = t1.bat[t1.who[5]].bname;
            else if (sym == "f6") exp1 = t1.bat[t1.who[6]].bname;
            else if (sym == "f7") exp1 = t1.bat[t1.who[7]].bname;
            else if (sym == "f8") exp1 = t1.bat[t1.who[8]].bname;
            else if (sym == "f9") exp1 = t1.bat[t1.who[9]].bname;
            else if (sym == "pv") exp1 = t1.bat[t1.who[piv(posn)]].bname;
            else if (sym == "po") exp1= PosName[posn];

            if (n < 0) line = exp1 + line.Substring(2);
            else line = line.Substring(0,n) + exp1 + line.Substring(n+3);
            n = line.IndexOf("*");
          }

      //  Delay until SAY_INTERVAL has elapsed since last "say"...
      //  Typically SAY_INTERVAL is 1.0.
      //  This delay logic has been moved to ShowResults.
          //t= 0;
          //if (runMode != RunMode.Fast) {
          //    while (t < SAY_INTERVAL) t = CBCXCommon.TimeInSeconds() - sayTimer;
          //}
          //sayTimer= CBCXCommon.TimeInSeconds();
          punch (line, bNewLine, pause:true);
      }


      /// <summary>
      /// This imposes a delay between postings to the results box.
      /// Note that this is not a delay between postings. If time interval since the
      /// prior posting is already elaped, there is no delay.
      /// </summary>
      /// 
      private void DelayResults() {
         double t = 0.0;
         double t0 = CBCXCommon.TimeInSeconds(); //<-- Try this, delay is absulute.
         if (!IsFastRunMode) { 
            while (t < SAY_INTERVAL) t = CBCXCommon.TimeInSeconds() - t0; //timeOfPriorPost;
         }
         //timeOfPriorPost = CBCXCommon.TimeInSeconds(); 
      }
      
      
      private void BumpB(ref int n, StatCat sc) {
      // -----------------------------------------------------
      // By firing EUpdateBBox event, this updates
      // the box score on the screen.

         int bx;
         bx = t[ab].slot;
         n++;
         //EUpdateBBox(n, sc, ab, linup[ab, bx]);
         lstBoxUpdates.Add(new StatToUpdate('B', n, sc, ab, t[ab].linup[bx])); //#1604.02
      }


      private void BumpP(ref int n, StatCat sc) {
      // -----------------------------------------------------
      // By firing EUpdatePBox event, this updates
      // the box score on the screen.

         int px;
         px = t[fl].curp;
         n++;
         //EUpdatePBox(n, sc, fl, px);
         lstBoxUpdates.Add(new StatToUpdate('P', n, sc, fl, px));
      }

      
      private void BumpR(ref int n, StatCat sc, int rix) {
      // -----------------------------------------------------
      // Update a stat for a runner (normally r)
         n++;
         //EUpdateBBox(n, sc, ab, rix);
         lstBoxUpdates.Add(new StatToUpdate('B', n, sc, ab, rix)); //1604.02
      }


      public static string StatDisplayStr(int n, StatCat sc) {
      // -----------------------------------------------------
         switch (sc) {
            case StatCat.ip:
               int m = n % 3;
               int ip = n / 3;
               if (m==0) return ip.ToString();
               else return ip.ToString() + "." + m.ToString();
            default: 
               return n.ToString();
         }
      }

      /// <summary>
      /// Calls BumpB and BumpP to increment stats as needed.<para>
      /// Arg is like h, 2, 3, 4, k, etc.</para>
      /// </summary>
      /// 
      private void incr(char code) { 
      // ---------------------------
         int y, bx, px;
         y = t[ab].slot;
         bx = t[ab].linup[y];
         px = t[fl].curp;

         CBatter b = t[ab].bat[bx];
         CPitcher p = t[fl].pit[px];      
         switch (code) {
            case 'h': 
               BumpB (ref b.bs.h, StatCat.h);
               BumpP (ref p.ps.h, StatCat.h);
               rk[ab,1]= rk[ab,1] + 1;
               EShowRHE?.Invoke();
               break;

            case '2': BumpB(ref b.bs.b2, StatCat.b2); break;
            case '3': BumpB(ref b.bs.b3, StatCat.b3); break;
            
            case '4': 
               BumpB(ref b.bs.hr, StatCat.hr);
               BumpP(ref p.ps.hr, StatCat.hr);
               break;

            case 'a': BumpB(ref b.bs.ab, StatCat.ab); break;

            case 'k': 
               BumpB(ref b.bs.so, StatCat.k);
               BumpP(ref p.ps.so, StatCat.k);
               break;

            case 'w': 
               BumpB(ref b.bs.bb, StatCat.bb);
               BumpP(ref p.ps.bb, StatCat.bb);
               break;

            case 'o': 
            // We just carry ip3, and devide by 3 at display time...
               BumpP(ref p.ps.ip3, StatCat.ip);
               ok++;
               break;

            case 'r':
            // note: rnr and pitcher stats 'bumped' by adv, not here.
            // BumpB(ref rk[ab,0], StatCat.scR);
            // We do not write the updated score to Results because
            // there could be multiple runs scored on the play, so
            // We write the new score in AtBat.
               rk[ab, 0]++;
               ///if runMode <> 3 then play ('440, 100');
               lines[ab, inn]++;
               EShowLinescore?.Invoke();

               break;

            case 's': break; //earned run: bumped by adv

            case 'b': BumpB(ref b.bs.bi, StatCat.rbi); break;

            case 'e': 
               rk[fl,2]= rk[fl,2] + 1;
               EShowRHE?.Invoke();
               break;
         }
      
      }


      private void err1(int p) {
   // -------------------------
      //MessageBox.Show("err1 " + p.ToString());
      po = posn;
      incr ('e');
      eok++;

      }


      private void BatDis(int n) {
      //--------------------------------------------------------
      // adv is resp for runner outs do begin  batdis for batter
      // outs, except if batter is safe then out extending,
      // which is adv 0 0. Err is resp for incrementing errors.
      //--------------------------------------------------------
         switch (n) {
            case 1: //Single
               incr ('a');
               incr ('h');
               break;

            case 2: //Double
               incr ('a');
               incr ('h');
               incr ('2');
               break;

            case 3: //Triple
               incr ('a');
               incr ('h');
               incr ('3');
               break;

            case 4: //Home run
               incr ('a');
               incr ('h');
               incr ('4');
               break;

            case 5: //Walk
               incr ('w');
               break;
               
            case 6: //Strike out
               incr ('a');
               incr ('o');
               incr ('k');
               break;

            case 7: //regular out
               incr ('a');
               incr ('o');
               break;
               
            case 8: //ip
               incr ('w');
               break;
               
            case 9: //hp
               incr ('t');
               break;

            case 10:  //k - no out (catcher drops ball
               incr ('a');
               incr ('k');
               break;
              
            case 11: //sac fly
               incr ('f');
               incr ('o');
               break;

            case 12: //sac bunt
               incr ('c');
               incr ('o');
               break;

            case 13: //on on err
               incr ('a');
               break;

            case 14: //fc
               incr ('a');
               break;
               
            default:
               throw new Exception("BatDis not implimented: " + n.ToString());   
               
         }         
    
      }
        
      /// <summary>  
      /// The arg, n, should point to a List that has a single action of
      /// type 'Select'. It scans the 'SItem' items in it, and returns the
      /// 'res' paramter of the chosen item. This is called separately
      /// from DoList by the client app.
      /// </summary>
      ///  
      private int SelectList(int n) { 
      // -------------------------------------------------------------- 
         //MessageBox.Show("SelectList " + n.ToString()); 
         //return 0;

         double cum = 0.0, a1;
         int a2, p=0; //Was 1 in Delphi;
         string sList = mEng.aActions[n];
         TAction at = (TAction)CEngine.Decoded(sList[0]);

      // The arg, n, to SelectList should have a singlr 'Select' Action...
         if (at != TAction.Select) {
            throw new Exception ("Expected atSelect in SelectList");
         }

         double r = rn.NextDouble();
         int ctr = 0;
         while (true) {
            ctr++;
            p += atLen[(int)at];
            at = (TAction)CEngine.Decoded(sList[p]);
            if (at != TAction.SItem) {
               throw new Exception ("Expected atSItem in SelectList");
               }
            else {
               a1 = 0.001*CEngine.Decoded(sList[p+1], sList[p+2]);
               cum += a1;
               if (r <= cum) {
                  a2 = CEngine.Decoded(sList[p+3]); 
                  return a2; //Normal return point
               }
            }
            if (ctr > 1000) {
               throw new Exception ("Error: Infinate loop in SelectList.");
            }   
         }
      
      }
      
         
      private void Advance(int rnr, int base0, char ue, char bi) {
      // ---------------------------------------------------------
         int n = r[rnr].ix;
         if (ok > 2) return;

         if (rnr == 0 && base0 != 4) {
         // batter gets on
            s[base0].Copy(r[0]);
            s[base0].stat = ue;
            s[base0].resp = t[1-ab].curp;
         }

         if (base0 == 4) {
            // run scores
            scoringPlay = true;
            if (n != 0) {
               if (homer || !(ab == 2 && inn > 8 && rk[1, 0] > rk[0, 0])) {
                  incr('r');
                  BumpR(ref t[ab].bat[n].bs.r, StatCat.r, n); //r for CRunner who scored
                  //pit[fl, r[rnr].resp].ps.r++; Bump does it.
                  BumpP(ref t[fl].pit[r[rnr].resp].ps.r, StatCat.r); //r for pitcher
                  if (r[rnr].stat == 'e' && ok + eok < 3) {
                  // An earned run...
                     //pit[fl, r[rnr].resp].ps.er++; Bump does it.
                     BumpP(ref t[fl].pit[r[rnr].resp].ps.er, StatCat.er); //er for pitcher
                  }
                  if (bi == 'r') incr('b');
               }
            }
            r[rnr].Clear();
         }
         else if (base0 == 0) {
         // runner is out
            r[rnr].Clear();
            incr('o');
         }
         else if (rnr != 0) {
         // normal advance
            s[base0].Copy(r[rnr]);
            r[rnr].Clear();
         }
      }  
      
     
      void InitSide() {
      // --------------------------------------------------------------
         flushrnrs(r);
         flushrnrs(s);
         ok = 0;
         eok = 0;
         sameguy = false;
         EShowLinescore?.Invoke();
         //SetMainCaption();
         eos = false;
         EShowFielders?.Invoke(fl);
         ESelectBoxTabs?.Invoke(ab);

      }
      
      
      void laycdf(int i, int j) {}     
      
      
      string nutshell() {
      // --------------------------------------------------------------
         string s = "";
         int o = onsit();
         o -= o <= 8 ? 1 : 8;
         if (o==0 && ok==0) 
            s = halfInn[ab] + inn.ToString() + ordSuffix(inn);
         else {
            switch (o) {
               case 0: s = "Bases empty, "; break;
               case 1: s = r[1].name + " on 1st, "; break;
               case 2: s = r[2].name + " on 2nd, "; break;
               case 3: s = r[3].name + " on 3rd, "; break;
               case 4: 
                  s = r[1].name + " & " + r[2].name + " on 1st & 2nd, ";
                  break;
               case 5: 
                  s = r[1].name + " & " + r[3].name + " on 1st & 3rd, ";
                  break;
               case 6: 
                  s = r[2].name + " & " + r[3].name + " on 2nd & 3rd, ";
                  break;
               case 7: s = "Bases loaded, "; break;
            }
            s += ok.ToString() + " out.";
         }
         return s;

      }
      
      
      void ResetRunners() {
      // --------------------------------------------------------------
         for (int i= 1; i<=3; i++) {
            if (s[i].ix > 0) r[i].Copy(s[i]);
            s[i].Clear();
         }
         r[0].name = t[ab].bat[t[ab].linup[t[ab].slot]].bname;
         r[0].ix = t[ab].linup[t[ab].slot];
         r[0].stat = 'e';
         r[0].resp = t[fl].curp;

      }


      /// <summary>
      /// This starts an a bat, or sets up for the next at bat, depending
      /// on the value of PlayState.
      /// </summary>
      /// 
      //public async Task<int> Go1() { 
      public int Go1() {

         string msg; int nReg;

         #region RegValidation
         // REG VALIDATION HERE
         // Check registration...
         //    nReg:= RegState;
         // if nReg = 0 then begin
         //    msg:=
         //      'BCX Baseball is not registered. ' +
         //      'Tap Help/Register in menus to register.';
         // end;
         // if msg <> '' then begin
         //   MsgBox (msg, '');
         // else//3
         #endregion

         #region MustTapTeams
         //if bname[0,1] = '' or bname[1,1] = '' then begin
         //   MsgBox('You must tap ''Teams'' to select home and visiting teams.','');
         //else //1
         #endregion

         do {

            if (PlayState == PLAY_STATE.START) {
            
            // Vaidate visitors' defense...
               msg = ValidateDefense(side.vis);
               if (msg != "") {
                  msg = "Visitors must make defensive changes: " + msg;
                  ENotifyUser?.Invoke(msg);
                  return 5;
               }
               
            // Vaidate home team's defense...
                msg = ValidateDefense(side.home);
               if (msg != "") {
                  msg = "Home team must make defensive changes: " + msg;
                  ENotifyUser?.Invoke(msg);
                  return 5;
               }
            
               InitGame();
            }

            if (PlayState == PLAY_STATE.NEXT || PlayState == PLAY_STATE.START) {
               //cmdGo.display := ". . .";
               //cmdGo.hidden:= true;
               //PlayState = PLAY_STATE.NONE;
               mEng.ClearLists();

               AdvSlot();
               InitBatter();
               PlayState = PLAY_STATE.PLAY;
               //If NoPauseBeforePlay;
               //   cmdGo;
            }
            else if (PlayState == PLAY_STATE.PLAY) {
         
            // Validate the defense -- If after pinch hitting in previous
            // half inning, there could be players with no position...
               msg = ValidateDefense((side)fl);
               if (msg != "") {
                  msg = "You must make defensive changes: " + msg;
                  ENotifyUser?.Invoke(msg);
                  return 5;
               }
            
               AtBat();

               // ----------------------------------------------
               // For testing extra innings
               // ----------------------------------------------
               //if (inn <= 1) {
               //   inn = 9;
               //   rk[1, 0] = 2;
               //   rk[0, 0] = 2;
               //   for (int i = 1; i <= 9; i++) { lines[0, i] = 0; lines[1, i] = 0; };
               //   lines[0, 3] = 2;
               //   lines[1, 7] = 2;
               //}

               if (eog) {
                  runMode = RunMode.Normal; //Game over so revert to normal mode.
                  //msg = "Final score: " +
                  //   t[0].lineName + " " + rk[0,0].ToString() + ", " +
                  //   t[1].lineName + " " + rk[1,0].ToString();
                  //ShowResults(msg, "\r\n", delay:true);
                  PlayState = PLAY_STATE.OVER;
               }
               else
                  PlayState = PLAY_STATE.NEXT;   
            }
            else {
               //cmdGo.display := 'Next'; cmdGo.hidden:= false;
               PlayState = PLAY_STATE.NEXT;
            }   

            #region CheckReg
	            // Check registration...
               // if (eos) and (inn >= 3) and (ab=1) then begin
               // CheckRegistration;
            #endregion         }

            #region AutoPlay
	         //if runMode = 2 or runMode = 3 then begin
            //   AutoPlay;
            //end;
            #endregion

         //} while (runMode != RunMode.Normal && !eos || IsFastRunMode);
         // #1512.02 -- Fixed this...
         } while (!(
              runMode == RunMode.Normal ||
              runMode == RunMode.FastEOP ||
             (runMode == RunMode.Auto && eos) ||
             (runMode == RunMode.Fast && eos) ||
              eog));

         return 0;

      }
      

      public void AdvSlot() {
      // --------------------------------------------------------------
      // Advance slot...
         if (PlayState != PLAY_STATE.START) {  
            if (!sameguy) { 
               t[ab].slot++; 
               if (t[ab].slot > 9) t[ab].slot = 1;
            }
            sameguy = false;
         }
      }

      
      public void InitBatter() {
      // --------------------------------------------------------------

         if (eos) {
         // Move to the next half inning by adjusting ab a/o inn...
            switch (ab) {
               case 0: ab = 1; fl = 0;  break;
               case 1: ab = 0; fl = 1; inn++; break;
            }
            InitSide(); 
         }

         //ClearResults(); // Out #150715: Redundant
         ResetRunners();
         homer = false;

      // Compute the combined parameters...
      // bp's and pp's have already been weighted for credibility...
      // #1605.01: All the logic consolidated into CombineParameters.
         int i = t[ab].linup[t[ab].slot], j = t[fl].curp;

   // Note: Change for custom teams...
   // Here, cmean must be recalc'd based on batter and pitcher's own lgMeans
      // cmean.CombineLeagueMeans(CBatter's lgmean, pitcher'switch lgMean))
         cpara.CombineParameters(
            t[ab].bat[i].par, t[ab].bat[i].lgPar, 
            t[fl].pit[j].par, t[fl].pit[j].lgPar, 
            cmean);

         ClearResults();
         punch(nutshell(), 2, pause:false);  //#150715: Made nutshell a fn and reorganized this
         punch(r[0].name, 2, pause:true);    //Yes, let's pause after nutshell
         punch("...", 0, pause:false);

         EShowResults?.Invoke(1);
         EShowRunners?.Invoke();
         EPostOuts?.Invoke();      

      // Update the param bar...
         EFmtParamBar?.Invoke();
         //sayp := 15: sayx := 1      

         EPostOuts?.Invoke();      
         EHideDicePointer?.Invoke();
         EHighlightBBox?.Invoke(ab, i); //#1506.01
         EHighlightPBox?.Invoke(fl, j); //#1506.01

      // Null out diceRoll objects, so batting spinner and fielding will not show...
         diceRollBatting.topLevelResult = TLR.none;
         diceRollFielding.topLevelResult = TLR.none;
         
      }


      /// ----------------------------------------------------------------
      /// <summary>
      /// Does an at bat, which can be regular AtBat, or some special play
      /// such as bunt, steal, IP. See remarks.
      /// </summary>
      /// <remarks>
      /// It translates specialPlay into a list index. Then it calls DoList
      /// on the list index. Regular at bat is list index 1.
      /// Special play list indexes are stored in GTAB in row 99: 
      /// 8-Steal, 9-Steal Home, 10-Sac Bunt, 11-Squeeze, 12-Walk(IP)
      /// 
      /// Normally invoked through cmdGo_Click -> Go1(), although I make it public.
      /// It needs to have CGame's specialPlay set.
      /// </remarks>
      /// 
      public void AtBat() {

         int listIx, col = 1;
         if (specialPlay==SPECIAL_PLAY.Steal && r[1].ix==0 && r[2].ix==0 && r[3].ix==0) {
            //MessageBox.Show ("You can't steal with no base runners.");
            ENotifyUser?.Invoke("You can't steal with no base runners.");
            return;
         }
         if (specialPlay==SPECIAL_PLAY.Bunt && r[1].ix==0 && r[2].ix==0 && r[3].ix==0) {
            //MessageBox.Show ("You can't sacrifice with no base runners.");
            ENotifyUser?.Invoke("You can't sacrifice with no base runners.");
            return;
         }

      // Translate specialPlay into listIx. 
      // listIx is the list index that will be DoList'ed.
         switch (specialPlay) {
            case SPECIAL_PLAY.AtBat: 
               listIx = 1; break;
            case SPECIAL_PLAY.Steal: 
               if (r[3].ix == 0) listIx = gres[99,8]; else listIx = gres[99,9]; break;
            case SPECIAL_PLAY.Bunt: 
               if (r[3].ix == 0) listIx = gres[99,10]; else listIx = gres[99,11]; break;
            case SPECIAL_PLAY.IP: 
               listIx = gres[99,12]; break;
            default: 
               listIx = 1; break;
         }

         //new BcxbLib.CAtBat().AtBat(specialPlay)
         Debug.WriteLine("--------------------------------------");
         Debug.WriteLine("DoList(ix=" + listIx.ToString() + ")");
         scoringPlay = false;
         mEng.DoList(listIx, "");
         if (listIx == 1) {
            EPlaceDicePointer?.Invoke(diceRollBatting, true);
         }

   //    Write new score if scoring play. Must do it here i/o Advance 
   //    or Incr because if multiple runs on same play...
         if (scoringPlay) { 
            var msg = 
               t[0].nick + " " + rk[0, 0].ToString() + ", " +
               t[1].nick + " " + rk[1, 0].ToString();
            ShowResults(msg, "\r\n", delay:true);
         }

      // reset SetSpecialPlay...
         specialPlay = SPECIAL_PLAY.AtBat;

      // Check for eos...
         if (ok >= 3) {
            //results += "\r\n" + "Side retired.";
            ShowResults("Side retired.", "\r\n", delay:true);
            eos = true;
         }
         if (ab == 1 && inn > 8  && rk[1, 0] > rk[0, 0]) 
            eos = true;

      // Check for eog...
         if (eos && inn > 8) 
            switch (ab) {
               case 1: if (rk[1,0] != rk[0,0]) eog = true; break;
               case 0: if (rk[1,0] > rk[0,0]) eog = true; break;
            }     
            
         if (eog) {
            var msg = "Final score: " +
               t[0].nick + " " + rk[0,0].ToString() + ", " +
               t[1].nick + " " + rk[1,0].ToString();
            ShowResults(msg, "\r\n", delay:true);
         }

         if (eos && !eog) {
            var msg = ab==0 ? "Middle of " : "End of ";
            msg += inn.ToString() + ordSuffix(inn) + ": " +
               t[0].nick + " " + rk[0, 0].ToString() + ", " +
               t[1].nick + " " + rk[1, 0].ToString();
            ShowResults(msg, "\r\n", delay:true);
         }

         ResetRunners();
         EShowResults?.Invoke(2);
         EUpdateBoxes?.Invoke();
         EShowRunnersOnly?.Invoke();
         EPostOuts?.Invoke();
         
     }  
      
      /// <summary>
      /// This matches pos to skillStr and returns a skill number,
      /// 0 to 6 (6 best, 0 worst). 
      /// </summary>
      /// <param name="pos">
      /// A position index, 1 to 9.
      /// </param>
      /// <param name="skillStr">
      /// Example: "---435---"
      /// 9 characters, each corresponds to std pos numbering (3:=1b, etc.)
      /// 
      /// Example: "5s,4ir,1" ??? Don't think this syntax is actually supported!
      /// Meaning: Skill=5 for ss to  4 for 2b, 3b, or rf, 1 for all other"
      /// Symbols: pk123slcr
      /// k:=catcher, i:=infield(2b, 3b, ss), o := of(lf, cr, rf)
      /// </param>
      /// 
      private int skill(int pos, string skillStr) {
      // -----------------------------------------------------------------
      // pos s/b 1 to 9.

         if (pos == 0) return 0;
         char c = skillStr[pos-1];
         if (c == '-') return 0;
         else return int.Parse(c.ToString());
      } 


      public void InitGame() {
      // --------------
         int ab0, bx, px;
         int c, inn0;

         inn = 1;
         ab = 0;
         fl = 1;
         t[0].slot = 1;
         t[1].slot = 1;
         eog = false;

      //// Debug...
      //   nTrials = 0;
      //   nHits = 0;
      //   totProb = 0;

         CBatter b;
         CPitcher p; 
         for (ab0=0; ab0<=1; ab0++) {
            for (bx=1; bx<=25 && t[ab0].bat[bx]!=null; bx++) {
               b = t[ab0].bat[bx];
               b.bs.ab = 0;
               b.bs.r = 0;
               b.bs.h = 0;
               b.bs.bi = 0;
               b.bs.b2 = 0;
               b.bs.b3 = 0;
               b.bs.hr = 0;
               b.bs.so = 0;
               b.bs.bb = 0;
               b.bs.sb = 0;
               b.bs.cs = 0;
               //bs_hp[ab0,bx] = 0;
               //bs_sf[ab0,bx] = 0;
               //bs_sh[ab0,bx] = 0;
               b.bbox = 0;
               t[ab0].xbox[bx] = 0;
            }
            for (px=1; px<=10 && t[ab0].pit[px]!=null; px++) { 
               p = t[ab0].pit[px];
               //p.ps.ip = 0;
               p.ps.ip3 = 0;
               p.ps.h = 0;
               p.ps.r = 0;
               p.ps.er = 0;
               p.ps.bb = 0;
               p.ps.so = 0;
               //p.ps.hb = 0;
               //p.ps.wp = 0;
               p.pbox = 0;
               t[ab0].ybox[px] = 0;
            }
            //for (inn0=1; inn0<=29; inn0++) lines[ab0,inn0] = 0; Not needed with CLineScore!
            lines = new CLineScore();
            EInitLinescore?.Invoke(); 
            for (c=0; c<=2; c++) {
               rk[ab0,c] = 0;
               EShowRHE?.Invoke();
            }
         }

      // Here, initialize the box score structures...
         InitializeBox(0);
         InitializeBox(1);
         InitSide();
         PlayState = PLAY_STATE.START;

      }
      
      
      string CvtSkill(int ab1, int bx, int opt) {return "";}


      public void InitializeBox(int ab1) {
      // ---------------------------------
      // Rebuild box score structures...

         int slt, pos;
         CBatter b;

         for (int bx=1; bx<=SZ_BAT-1 && t[ab1].bat[bx]!=null; bx++) {
            t[ab1].bat[bx].bbox = 0;
            t[ab1].xbox[bx] = 0;
            t[ab1].bat[bx].bs.boxName = "";
         }
         for (int bx=1; bx<=SZ_BAT-1 && t[ab].bat[bx]!=null; bx++) {
            b = t[ab1].bat[bx];
            slt = b.when;
            pos = b.where;
            if (slt > 0) {
               b.bbox = slt;
               t[ab1].xbox[slt] = bx;
               b.bs.boxName = b.bname2;
               b.bs.bx = bx; //1906.02
               if (b.where != 0) b.bs.boxName += "," + aPosName[b.where];
            }
            //2014/4: Out -- why show non-better in batter box?
            //else if (slt == 0 && pos > 0) { 
            //// This s/b non-batting pitcher, show 10th...
            //   b.bs.boxName = b.bname + "," + aPosName[b.where];
            //   b.bbox = 10;
            //   xbox[ab1,10] = bx;
            //}   
         }
         ERefreshBBox?.Invoke(ab1);

      // Pitcher box...  //1906.02: This section modified for ps.px.
         CPitcher p;
         for (int px=1; px<=SZ_PIT-1 && t[ab1].pit[px]!=null; px++) {
            p = t[ab1].pit[px];
            p.pbox = 0;
            t[ab1].ybox[px] = 0;
            p.ps.px = px; //1906.02 
            p.ps.bx = 0; //1906.02: How to get bx for a pitcher???
         }
         t[ab1].pit[t[ab1].curp].pbox = 1;
         t[ab1].ybox[1] = t[ab1].curp;
         ERefreshPBox?.Invoke(ab1);
         
      }


      /// <summary>
      /// Insert batter bx into BBox following bbox index bix.
      /// </summary>
      /// <param name="bix"Index in bbox></param>
      /// <param name="bx">Batter index</param>
      /// <param name="ab1">Side, 0 or 1</param>
      /// 
      public void InsertIntoBBox(int bix, int bx, int ab1) {

         for (int i = 25; i > bix + 1; i--) t [ab1].xbox [i] = t [ab1].xbox [i - 1];
         t[ab1].xbox [bix + 1] = bx;

      // Rebuild the 'bbox' column of CBatter, using xbox...
         for (int i = 1; i <= 25; i++) if (t[ab1].bat[i] != null) t[ab1].bat[i].bbox = 0;
         for (int i = 1; i <= 25; i++) {
            int j = t[ab1].xbox[i];
            if (j > 0) t[ab1].bat[j].bbox = i;
         }
      }

     /// <summary>
     /// Insert pitcher px into PBox at the end.
     /// </summary>
     /// <param name="px">Pitcher index</param>
     /// <param name="ab1">Side, 0 or 1</param>
     /// 
     public void InsertIntoPBox(int px, int ab1) {

         int nextix = t[ab1].pit[t[ab1].curp].pbox + 1;
         t[ab1].pit[px].pbox = nextix;
         t[ab1].ybox[nextix] = px;
         t[ab1].curp = px;
      }

      /// <summary>
      /// This has not been implemented.
      /// It woukd appear that its/ call BBoxTotext an PBoxToText.
      /// </summary>
      /// <param name="ab1"></param>
      /// <returns>String version of Roster</returns>
      public string RosterToText(int ab1) {
 
         int bx;
         var s =
            new System.Text.StringBuilder("Name         ab  r  h bi 2b 3b hr bb so");
         CBatter b;
         for (int i = 1; i < CGame.SZ_BAT; i++) {
            if ((bx = t[ab1].xbox[i]) == 0) continue;
            b = t[ab1].bat[bx];
            s.Append("/r/n" + string.Format("{0:-12}", b.bs.boxName.Substring(0, 12)));
            s.Append(b.bs.ab.ToString("##0"));
         }
         return s.ToString();
      }

      /// <summary>
      /// This has not been implimented.
      /// </summary>
      /// <param name="ab"></param>
      /// <returns>String version of batter box score</returns>
      /// 
      public string BBoxToText(int ab) {
      // -------------------------------------------------------------------
         int bx;
         var s =
            new System.Text.StringBuilder("Name         ab  r  h bi 2b 3b hr bb so");
         CBatter b;
         for (int i = 1; i < CGame.SZ_BAT; i++)
         {
            if ((bx = t[ab].xbox[i]) == 0) continue;
            b = t[ab].bat[bx];
            s.Append("/r/n" + String.Format("{0:-12}", b.bs.boxName.Substring(0, 12)));
            s.Append(b.bs.ab.ToString("##0"));
            s.Append(b.bs.r.ToString("##0"));
            s.Append(b.bs.h.ToString("##0"));
            s.Append(b.bs.bi.ToString("##0"));
            s.Append(b.bs.b2.ToString("##0"));
            s.Append(b.bs.b3.ToString("##0"));
            s.Append(b.bs.hr.ToString("##0"));
            s.Append(b.bs.bb.ToString("##0"));
            s.Append(b.bs.so.ToString("##0"));
         }
         return s.ToString();
      }

      /// <summary>
      /// This has not been implimented.
      /// </summary>
      /// <param name="ab"></param>
      /// <returns>String version of pitcher box score</returns>
      /// 
      public string PBoxToText(int fl) {
      // -------------------------------------------------------------------
         int px;
         var s = new System.Text.StringBuilder("Name          ip  h hr  r er so bb");
         CPitcher p;
         for (int i = 1; i < CGame.SZ_PIT; i++)
            if ((px = t[fl].ybox[i]) != 0) {
               p = t[fl].pit[px];
               s.Append("/r/n" + String.Format("{0:-12}", p.pname.Substring(0, 12)));
               s.Append(StatDisplayStr(p.ps.ip3, StatCat.ip));
               s.Append(p.ps.h.ToString("##0"));
               s.Append(p.ps.hr.ToString("##0"));
               s.Append(p.ps.r.ToString("##0"));
               s.Append(p.ps.er.ToString("##0"));
               s.Append(p.ps.so.ToString("##0"));
               s.Append(p.ps.bb.ToString("##0"));
            }
         return s.ToString();

      }

      /// <summary>
      /// This reads CFEng2 and CFEng3 filling aSay[] and gres[] respectively.
      /// </summary>
      /// -------------------------------------------------------------------
      /// 
      public void ReadModel()
      {

         string rec;
         int n;

         // Scan CFEng2, converting it to array, aSay.
         //
         // Open CFEng2 for input -- read util eof, get n and the string, and
         // fill aSay. Upper bound to be found at: #RECCNT: 139.
         // rec looks like this: 0x00000000,0x0000,1,"Base hit."   }
         //
         using (StreamReader f = this.ERequestModelFile(2))
         {
            while ((rec = f.ReadLine()) != null)
            {
               if (rec.Length >= 7 && rec.Substring(0, 7) == "#RECCNT")
               {
                  n = int.Parse(rec.Substring(9));
                  aSay = new string[n + 1]; //So that last elt is #n
               }
               if (rec[0] == '#') continue;
               n = rec.IndexOf(",");
               if (n == 0)
               {
                  throw new Exception("Invalid format in cfeng2.bcx");
               }
               int ix = int.Parse(rec.Substring(0, n));
               string s = rec.Substring(n + 1);
               CBCXCommon.DeQuote(ref s);
               aSay[ix] = s;
            }
         }


         // Scan CFEng3, converting it to array, gres.
         //
         // Open CFEng3 for input -- read util eof, get n and the string, and
         // fill gres. gres is 100x15. The string has 15 numbers, each in
         // 4 character hexadecimal format.
         // rec looks like this:
         // 0x00000000,0x0000,2,002A002B002C0025002D002E002F0030002B002C0025002D002E002F0030   }
         //
         using (StreamReader f = ERequestModelFile(3))
         {
            while ((rec = f.ReadLine()) != null)
            {
               if (rec[0] == '#') continue;
               n = rec.IndexOf(",");
               if (n == 0)
               {
                  throw new Exception("Invalid format in cfeng3.bcx");
               }
               int ix = int.Parse(rec.Substring(0, n));
               string s = rec.Substring(n + 1);

               // s should have 15 numbers, each encoded a 4 hexadecimal characters.
               if (s.Length != 60)
               {
                  throw new Exception("Invalid format in cfeng3.bcx");
               }

               // Now convert the 15 hex numbers that are encoded in s...
               for (int i = 0; i <= 14; i++)
               {
                  n = CBCXCommon.GetWord(s, i);
                  gres[ix, i + 1] = n;
               }
            }
         }

      }

      string rec, sVer;
      string db_NameUse, db_SkillStr, db_stats;


      /// <summary>
      /// TASK: Open sFile, find start of sTeam (eg, NYA2005), and then read
      /// records filling team roster variables for side, ab.
      /// </summary>
      /// <remarks>
      /// DELETE WHEN CTEAM INPLIMENTED
      /// </remarks>
      /// 
      //public void ReadTeam(int ab) { 
      //// ----------------------------------------------------
      //// bx is index into the batter matrix, bp.
      //// px is index into the pitcher matrix, pp.

      //   int ptr;
      //   int j, bx, px;
      //   int slot, posn, slotDh, posnDh;
      //   //int pitSlot=0, pitPosn=0; string pitStats = "";
         
      //   PlayState = PLAY_STATE.START;

      //   // Open the team table.
      //   //string fileName = @"C:\Prj\BCXBWinPrj\CSApp\TeamPackages\" + sFile + ".bcxb";
      //   //string fileName = @"C:\Prj\General\CF2001\PlayerData2010\Teams\" + sFile + ".bcxt";
      //   //string fileName = @"C:\Prj\PlayerData2010\Teams\" + sFile + ".bcxt";
      //   //StreamReader f = new StreamReader(fileName);
      //   using (StreamReader f = ERequestTeamFileReader(fileName[ab])) {

      //      // Line 1: Read the version...
      //      rec = f.ReadLine();
      //      sVer = rec.Trim();

      //      // Line 2:Read the team record...
      //      string[] aRec = (f.ReadLine()).Split(',');
      //      nick[ab] = aRec[3]; city[ab] = aRec[2]; lineName[ab] = aRec[1];

      //      // Line 3: League-level stats...
      //      aRec = (f.ReadLine()).Split(',');
      //      db_stats = aRec[1];
      //      if (db_stats[0] != '0')
      //         throw new Exception("Error in ReadTeam: Expected '0'");
      //      ptr = 1;
      //      lgStats[ab].ab = CBCXCommon.GetHex(db_stats, ref ptr, 5);
      //      lgStats[ab].h = CBCXCommon.GetHex(db_stats, ref ptr, 4);
      //      lgStats[ab].b2 = CBCXCommon.GetHex(db_stats, ref ptr, 4);
      //      lgStats[ab].b3 = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].hr = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].so = CBCXCommon.GetHex(db_stats, ref ptr, 4);
      //      lgStats[ab].sh = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].sf = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].bb = CBCXCommon.GetHex(db_stats, ref ptr, 4);
      //      lgStats[ab].ibb = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].hbp = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].sb = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      lgStats[ab].cs = CBCXCommon.GetHex(db_stats, ref ptr, 3);
      //      //lgStats[ab].ip = (double)(CBCXCommon.GetHex(db_stats, ref ptr, 5)) / 3.0;
      //      lgStats[ab].ip3 = CBCXCommon.GetHex(db_stats, ref ptr, 5);
      //      lgStats[ab].ave = lgStats[ab].ab == 0 ?
      //         0.0 :
      //         Math.Round((double)lgStats[ab].h / (double)lgStats[ab].ab, 3);

      //      lgMean[ab].FillLgParas(lgStats[ab]);
      //      lgMean[ab].h /= fMean0; // This applies the fudge factor...   

      //      //This section moved to CHittingParamSet.CombineLeagueMeans...
      //      //----------------------------------------------------------
      //      //if (ab == 1)
      //      //{
      //      //   // Combine the means for the 2 teams into a single set...
      //      //   cmean.h = (mean[0].h + mean[1].h) / 2.0;
      //      //   cmean.b2 = (mean[0].b2 + mean[1].b2) / 2.0;
      //      //   cmean.b3 = (mean[0].b3 + mean[1].b3) / 2.0;
      //      //   cmean.hr = (mean[0].hr + mean[1].hr) / 2.0;
      //      //   cmean.so = (mean[0].so + mean[1].so) / 2.0;
      //      //   cmean.bb = (mean[0].bb + mean[1].bb) / 2.0;
      //      //   cmean.oth = 1.0 - (cmean.h + cmean.so + cmean.bb);
      //      //   cmean.sb = (mean[0].sb + mean[1].sb) / 2.0;
      //      //   cmean.fPitBase = (mean[0].fPitBase + mean[1].fPitBase) / 2.0;
      //      //   cmean.fSacBunt = (mean[0].fSacBunt + mean[1].fSacBunt) / 2.0;
      //      //}

      //      // Lines 4+: Player records (batter & pitcher)
      //      bx = 0;
      //      px = 0;
      //      CBatter b;
      //      CPitcher p;
      //      while ((rec = f.ReadLine()) != null)
      //      {
      //         aRec = rec.Split(',');
      //         db_NameUse = aRec[0]; db_SkillStr = aRec[1]; db_stats = aRec[2];

      //         // First read the batter stats, for both batters & pitchers...
      //         ptr = 1;
      //         if (usingDh) ptr += 2;
      //         slot = CBCXCommon.GetHex(db_stats, ref ptr, 1);
      //         posn = CBCXCommon.GetHex(db_stats, ref ptr, 1);
      //         if (!usingDh) ptr += 2;

      //         bx++;
      //         if (bx >= SZ_BAT) throw new Exception("Too many batters in " + teamTag[ab]);
      //         b = t[ab].bat[bx] = new CBatter(this);
      //         //if (bx > 25) MessageBox.Show("Too many batters in " + sTeam);
      //         b.bname = db_NameUse;
      //         b.skillStr = db_SkillStr;
      //         FillBatStats(db_stats, ref b.br, ref ptr);
      //         b.par.FillBatParas(b.br, lgMean[ab]);
      //         b.when = (slot == 10 ? 0 : slot);
      //         if (slot > 0 && slot <= 9) t[ab].linup[slot] = bx; //So 10 (non-hitting pitcher) is slot 0
      //         b.where = (posn == 10 ? 10 : posn); //dh is 10 in the file, keep that.
      //         if (posn > 0 && posn <= 9) t[ab].who[posn] = bx;
      //         b.used = (slot > 0 || posn > 0);
      //         b.bx = bx;
      //         b.px = 0; //See below, this is assigned for pitchers.
      //         b.sidex = (side)ab; //Tells which team he's on, 0 or 1.

      //         if (db_stats[0] == '2')
      //         {
      //            // It's a Pitcher record...
      //            px++; //Initialized to 0, so starts with 1.
      //            if (px == 1) t[ab].curp = 1;  //First pitcher listed starts today.
      //            if (px >= SZ_PIT) throw new Exception("Too many pitchers in " + teamTag[ab]);
      //            p = t[ab].pit[px] = new CPitcher();
      //            b.px = px;
      //            p.pname = db_NameUse;
      //            FillPitStats(db_stats, ref p.pr, ref ptr); //Continue with same value of ptr...
      //            p.par.FillPitParas(p.pr, lgMean[ab]);
      //            b.px = p.px = px;
      //            p.sidex = (side)ab;
      //         }
      //      }
      //   }

      //}

 

   public TextReader GetTeamFileReader (string fName) {
   // ------------------------------------------------------------------
   // This allows other classes, like forms, to have access to the event...   
      if (fName == null || fName == "") 
         throw new Exception("File name is missing in GetTeamFileReader");
      TextReader f = ERequestTeamFileReader(fName);
      return f;

   }


   /// <summary> 
   /// Update the skill string for 1 position for 1 player... 
   /// </summary>
   /// ------------------------------------------------------
   /// 
   public void PersistSkill(int ab1, int bx1, string skillStr) { 
 
      int bx, n;
      List<StringBuilder> roster = LoadRoster(t[ab1].fileName);
 
   // Write the roster back out to file, replacing skill where needed... 
      using (StreamWriter g = ERequestTeamFileWriter(t[ab1].fileName)) { 
         bx = n = 0; 
         foreach(StringBuilder rec1 in roster) { 
         // First 3 lines are version ID, team record, league record...
            n++;
            if (n <= 3) {g.WriteLine(rec1.ToString()); continue;}
            else bx++; 
            if (bx == bx1) {
               int i = rec1.ToString().IndexOf(',');
               rec1.Remove(i+1, 9);
               rec1.Insert(i+1, skillStr);
            } 
            g.WriteLine(rec1.ToString()); 
         }  
      } 

   }
 
 
   /// <summary> 
   /// Update lineup and positions for a team, in the team file on disk... 
   /// </summary>
   /// ----------------------------------------------------------------
   /// 
   public void PersistLineup(int ab1) {

      int bx, n;
      int ix;
      List<StringBuilder> roster = LoadRoster(t[ab1].fileName);
 
      // Write the roster back out to file, replacing skill where needed... 
      using (StreamWriter g = ERequestTeamFileWriter(t[ab1].fileName)) {
 
         int slotOffset;
         int posnOffset;
         bx = n = 0; 
         foreach (StringBuilder rec1 in roster) {
 
         // First 3 lines are version ID, team record, league record...
            n++;
            if (n <= 3) {g.WriteLine(rec1.ToString()); continue; }

         // Layout: 
         // 1: 1=batter, 2=pitcher
         // 2:slot-3:posn (non-dh), 4:slot-5:posn (dh)
            bx++;
            ix = rec1.ToString().LastIndexOf(',');
            slotOffset = UsingDh ? ix + 4 : ix + 2;
            posnOffset = UsingDh ? ix + 5 : ix + 3;
 
            int sl = t[ab1].bat[bx].when;   // 0 if not in lineup, or if non-batting pitcher
            int po = t[ab1].bat[bx].where; // 0 if not fielding, or if dh
 
            ix = rec1.ToString().LastIndexOf(',');
 
            if (UsingDh) {
               if (po == 1 && sl == 0) rec1[slotOffset] = 'A'; //Non-batting pitcher.
               else rec1[slotOffset] = sl.ToString()[0];
 
               if (sl > 0 && po == 10) rec1[posnOffset] = 'A'; //Non-fielding hitter (DH).
               else rec1[posnOffset] = po.ToString()[0];
            }
            else {
               rec1[slotOffset] = sl.ToString()[0];
               rec1[posnOffset] = po.ToString()[0];
            }
            g.WriteLine(rec1.ToString());
         }
 
      }
   } 
 
 
   /// <summary>
   /// Reds a team file from disk, and loads it into a List of
   /// StringBuilder objects.
   /// </summary>
   /// 
   private List<StringBuilder> LoadRoster(string sFile) {
 
      var roster = new List<StringBuilder>();
      using (TextReader f = ERequestTeamFileReader(sFile)) {
 
         while ((rec = f.ReadLine()) != null) {
            roster.Add(new StringBuilder(rec));
         }
      }
      return roster;
 
   }
 

   /// <remarks>
   /// DELETE WHEN CTEAM IMPLIMENTED
   /// It's been moved to CTeam.
   /// </remarks>
   /// 
   private void FillBatStats(string stats, ref CBatRealSet br, ref int ptr) {
   // -------------------------------------------------------------------
   // GetHex returns -1 if db_stats is all F's -- this indicates missing.
  
      br.ab = CBCXCommon.GetHex(stats, ref ptr, 3);
      if (br.ab > 15) {
         br.hr = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.bi = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.sb = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.cs = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.h = CBCXCommon.GetHex(stats, ref ptr, 3);
         br.ave = Math.Round((double)br.h / (double)br.ab,3);
         br.b2 = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.b3 = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.bb = CBCXCommon.GetHex(stats, ref ptr, 2);
         br.so = CBCXCommon.GetHex(stats, ref ptr, 3);
      }
      else {
      // 15 or fewer ab's: all stats 1 digit...
         br.hr = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.bi = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.sb = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.cs = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.h = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.ave = 0.0;
         br.b2 = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.b3 = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.bb = CBCXCommon.GetHex(stats, ref ptr, 1);
         br.so = CBCXCommon.GetHex(stats, ref ptr, 1);
      }

   }


   /// <remarks>
   /// DELETE WHEN CTEAM IMPLIMENTED
   /// It's been moved to CTeam.
   /// </remarks>
   /// 
   private void FillPitStats(string stats, ref CPitRealSet pr, ref int ptr) {
   // -------------------------------------------------------------------
   // GetHex returns -1 if db_stats is all F's -- this indicates missing.

      pr.g = CBCXCommon.GetHex(stats, ref ptr, 2);
      pr.gs = CBCXCommon.GetHex(stats, ref ptr, 2);
      pr.w = CBCXCommon.GetHex(stats, ref ptr, 2);
      pr.l = CBCXCommon.GetHex(stats, ref ptr, 2);
      pr.bfp = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.ip3 = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.h = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.er = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.hr = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.so = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.bb = CBCXCommon.GetHex(stats, ref ptr, 3);
      pr.sv = CBCXCommon.GetHex(stats, ref ptr, 2);
      pr.era = pr.ip3 == 0.0 ? 0.0 : pr.er / ((double)pr.ip3 / 3.0) * 9.0;
   }


   public string ShowLists() {
   // --------------------------------------------------------------
      return(mEng.ShowLists());
   }


   /// <summary>
   /// Not wired in.
   /// Purpose: get batter totals for box score.
   /// </summary>
   /// 
   public CBatBoxSet BBoxTotals(int ab) {
   // --------------------------------------------------------------
      var tot = new CBatBoxSet();
      CBatter b;
      int bx;

      tot.ab = 0;
      tot.h = 0;
      tot.r = 0;
      tot.bi = 0;
      tot.b2 = 0;
      tot.b3 = 0;
      tot.hr = 0;
      tot.bb = 0;
      tot.so = 0;

      for (int i = 1; (bx = t[ab].xbox[i]) != 0; i++) {
         b = t[ab].bat[bx];
         tot.ab += b.bs.ab;
         tot.h += b.bs.h;
         tot.r += b.bs.r;
         tot.bi += b.bs.bi;
         tot.b2 += b.bs.b2;
         tot.b3 += b.bs.b3;
         tot.hr += b.bs.hr;
         tot.bb += b.bs.bb;
         tot.so += b.bs.so;
      }

      return tot;
   }


   /// <summary>
   ///  Purpose: get batter totals for box score.
   ///  Not wired in
   /// </summary>
   /// 
   public CPitBoxSet PBoxTotals(int fl) {

      var tot = new CPitBoxSet();
      CPitcher p;
      int px;

      //tot.ip = 0;
      tot.ip3 = 0;
      tot.h = 0;
      tot.r = 0;
      tot.er = 0;
      tot.so = 0;
      tot.bb = 0;

      for (int i = 1; (px = t[fl].ybox[i]) != 0; i++) {
         p = t[fl].pit[px];
         //tot.ip += p.ps.ip;
         tot.ip3 += p.ps.ip3;
         tot.h += p.ps.h;
         tot.r += p.ps.r;
         tot.er += p.ps.er;
         tot.so += p.ps.so;
         tot.bb += p.ps.bb;
      }

      return tot;

   }


   /// <summary>
   /// You need a player at each pos, 1..9.
   /// This handles when pinch hitters, they need position assigned at
   /// end of inning.
   /// </summary>
   /// <returns>Error message or empty string</returns>
   /// 
   public string ValidateDefense(side ab) {
      string msg = "";
      CTeam t1 = t[(int)ab];

   // Check for a player at each position 1..9 or 10...
      for (int pos = 1; pos <= PosLim; pos++) {
         if (t1.who[pos] == 0)
            msg += "No player at position: " + CGame.posAbbr[pos] + "\r\n";
      }
  
      return msg;
   }


   /// <summary>
   /// This is the master box score formatter. It calls separate functions
   /// for the formatting the batter and pitcher sections, and writing
   /// them to file.
   /// </summary>
   /// 
   public void PrintBox(string fName) {

         //var f = new StreamWriter(fName);
         using (StreamWriter f = ERequestBoxFileWriter(fName)) {
            string s =
               t[0].city + " at " + t[1].city + ", " + DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            f.WriteLine(new string('=', s.Length));
            f.WriteLine(s);
            f.WriteLine(new string('=', s.Length));
            f.WriteLine(eog ? "Final" : "In-game");

            PrintLineScore(f);
            PrintBBox(f, (int)side.vis);
            PrintPBox(f, (int)side.vis);
            PrintBBox(f, (int)side.home);
            PrintPBox(f, (int)side.home);
         }

   }


      /// <summary>
      /// This formats the batting portion of the box score for 
      /// one side, and writes it to file.
      /// </summary>
      /// 
      private void PrintBBox(StreamWriter f, int ab) {
         int bx, j;
         CBatter b;
         string rec1 = "";
         f.WriteLine();

      // First, a header line...
         rec1 = t[ab].city.PadRight(21);
         rec1 += "ab".PadLeft(3);
         rec1 += "r".PadLeft(3);
         rec1 += "h".PadLeft(3);
         rec1 += "bi".PadLeft(3);
         rec1 += "2b".PadLeft(3);
         rec1 += "3b".PadLeft(3);
         rec1 += "hr".PadLeft(3);
         rec1 += "bb".PadLeft(3);
         rec1 += "so".PadLeft(3);
         f.WriteLine(rec1);
         f.WriteLine(new string('-', rec1.Length));

      // Then each batter... 
         for (int i = 1; i < CGame.SZ_BAT; i++) {
            if ((bx = t[ab].xbox[i]) == 0) continue;
            b = t[ab].bat[bx];
            rec1 = b.bs.boxName.PadRight(21);
            rec1 += b.bs.ab.ToString().PadLeft(3);
            rec1 += b.bs.r.ToString().PadLeft(3);
            rec1 += b.bs.h.ToString().PadLeft(3);
            rec1 += b.bs.bi.ToString().PadLeft(3);
            rec1 += b.bs.b2.ToString().PadLeft(3);
            rec1 += b.bs.b3.ToString().PadLeft(3);
            rec1 += b.bs.hr.ToString().PadLeft(3);
            rec1 += b.bs.bb.ToString().PadLeft(3);
            rec1 += b.bs.so.ToString().PadLeft(3);
            f.WriteLine(rec1);
         }

      // Finally, the totals line...
         CBatBoxSet tot = BBoxTotals(ab);
         rec1 = "Totals".PadRight(21);
         rec1 += tot.ab.ToString().PadLeft(3);
         rec1 += tot.r.ToString().PadLeft(3);
         rec1 += tot.h.ToString().PadLeft(3);
         rec1 += tot.bi.ToString().PadLeft(3);
         rec1 += tot.b2.ToString().PadLeft(3);
         rec1 += tot.b3.ToString().PadLeft(3);
         rec1 += tot.hr.ToString().PadLeft(3);
         rec1 += tot.bb.ToString().PadLeft(3);
         rec1 += tot.so.ToString().PadLeft(3);
         f.WriteLine(rec1);

      }


      /// <summary>
      /// This formats the pitching portion of the box score for 
      /// one side, and writes it to file.
      /// </summary>
      /// 
      private void PrintPBox(StreamWriter f, int fl) {
         int i = 1, px;
         CPitcher p;
         string rec1 = "";
         f.WriteLine();

      // First, a header line...
         rec1 = "Pitching".PadRight(23);
         rec1 += "ip".PadLeft(5);
         rec1 += "h".PadLeft(4);
         rec1 += "r".PadLeft(4);
         rec1 += "er".PadLeft(4);
         rec1 += "bb".PadLeft(4);
         rec1 += "so".PadLeft(4);
         f.WriteLine(rec1);
         f.WriteLine(new string('-', rec1.Length));

      // Then each pitcher... 
         while ((px = t[fl].ybox[i]) != 0) {
            p = t[fl].pit[px];
            rec1 = p.pname.PadRight(23);
            rec1 += StatDisplayStr(p.ps.ip3, StatCat.ip).PadLeft(5); // Converts to "n.m", m=0,1,2
            rec1 += p.ps.h.ToString().PadLeft(4);
            rec1 += p.ps.r.ToString().PadLeft(4);
            rec1 += p.ps.er.ToString().PadLeft(4);
            rec1 += p.ps.bb.ToString().PadLeft(4);
            rec1 += p.ps.so.ToString().PadLeft(4);
            f.WriteLine(rec1);
            i++;
         }   

      // Finally, the totals line...
         CPitBoxSet tot = PBoxTotals(fl);
         rec1 = "Totals".PadRight(23);
         rec1 += StatDisplayStr(tot.ip3, StatCat.ip).PadLeft(5); // Converts to "n.m", m=0,1,2
         rec1 += tot.h.ToString().PadLeft(4);
         rec1 += tot.r.ToString().PadLeft(4);
         rec1 += tot.er.ToString().PadLeft(4);
         rec1 += tot.bb.ToString().PadLeft(4);
         rec1 += tot.so.ToString().PadLeft(4);
         f.WriteLine(rec1);

      }


      /// <summary>
      /// This prints the line score to file
      /// </summary>
      /// 
      public void PrintLineScore(StreamWriter f) {

         int i2, i, wid = 3;
         string rec1 = "";
         f.WriteLine();

      // Header line...
         rec1 = "Line Score" + "  r  h  e".PadLeft(10 + 3*inn + 8);
         f.WriteLine(rec1);
         f.WriteLine(new string('-', rec1.Length));

      // Visitors...
         rec1 = t[0].city.PadRight(16);
         for (i = 1; i <= this.inn; i++) {
            //wid = ((i-1) % 3) == 0 ? 5 : 3;
            rec1 += this.lines[0, i].ToString().PadLeft(wid);
         }
      // RHE...
         rec1 += " - ";
         rec1 += this.rk[0, 0].ToString().PadLeft(3);
         rec1 += this.rk[0, 1].ToString().PadLeft(3);
         rec1 += this.rk[0, 2].ToString().PadLeft(3);

         f.WriteLine(rec1);

      // Home...
         rec1 = t[1].city.PadRight(16);
         i2 = this.inn;
         if (this.ab == 0) i2 = this.inn - 1; else i2 = this.inn;
         for (i = 1; i <= i2; i++) {
            //wid = ((i - 1) % 3) == 0 ? 5 : 3;
            rec1 += this.lines[1, i].ToString().PadLeft(3);
         }
         if (i2 < inn) rec1 += "   "; 
      // RHE...
         rec1 += " - ";
         rec1 += this.rk[1, 0].ToString().PadLeft(3);
         rec1 += this.rk[1, 1].ToString().PadLeft(3);
         rec1 += this.rk[1, 2].ToString().PadLeft(3);

         f.WriteLine(rec1); //Maybe this should be moved to caller, and this just return a string???

      }

   }  //class 


}  //namespace


