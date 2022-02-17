using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BCX.BCXCommon;
using System.Linq;

using BcxbDataAccess;

namespace BCX.BCXB {

   public class CTeam {

      public const int SZ_BAT = 26; //We use 1..25, 0 is unused. 
      public const int SZ_PIT = 12; //We use 1.11, 0 is unused.
      const int SZ_AB = 2;    
      const int SZ_POS = 11;   //1..9; //1903.1 Was 10
      const int SZ_SLOT = 10;  //1..9;
      const int SZ_LINESCORE = 31;    //1..30; //index in line score

      public CGame g = null;
      public CBatter[] bat;  
      public CPitcher[] pit;

      public CBatBoxSet btot = new() { boxName = "Total" }; //#2202.1
      public CPitBoxSet ptot = new() { boxName = "Total" };

      public CBatRealSet lgStats;
      public string fileName;
      public string teamTag;
      public bool usesDhDefault = false;

      public string city, nick, lineName;
      
      public int[] linup = new int[SZ_SLOT];
      public int slot;
      public int[] xbox = new int[SZ_BAT];//<-- boxx For bbox entry i, tells what batter (bs_) ix he is.

      public CHittingParamSet lgMean =
         new CHittingParamSet(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);


      public CTeam (CGame g1) {
      // -------------------------------------
      // Constructor
      // Leave g1 as null if there is no game contect.

         g = g1;
         //Don't use g's usingDh for this...
         //usingDh = g == null ? false : g.usingDh;
      }


      public string CurrentBatterName {
   // ----------------------------------
         get { return bat[linup[slot]].bname; }
      }


      public string CurrentPitcherName {
   // ----------------------------------
         get { return pit[curp].pname; }
      }


      //public CBatBoxSet BTot {
      //   get {
      //      return new CBatBoxSet() {
      //         ab = bat.Sum(e => e.bs.ab),
      //         h = bat.Sum(e => e.bs.h),
      //         bi = bat.Sum(e => e.bs.bi),
      //         r = bat.Sum(e => e.bs.r),
      //         so = bat.Sum(e => e.bs.so),
      //         bb = bat.Sum(e => e.bs.bb)
      //      };
      //   }
      //}

   // pitcher arrays
      public int curp;  //<--px
      public int[] ybox = new int[SZ_PIT];//<--pbx  For pbox entry i, tells what pitcher (ps_) ix he is.

   // fielding
      public int[] who = new int[SZ_POS];    //<-- bx

      //public int whoDh {
      //// -----------------------------------------------------------
      //   get {
      //      CBatter b = bat.FirstOrDefault(b1 => (b1?.where ?? 0) == 10);
      //      return b?.bx ?? 0;
      //   }
      //}
      

      /// <summary>
      /// TASK: Open sFile, find start of sTeam (eg, NYA2005), and then read
      /// records filling team roster variables for side, ab.
      /// </summary>
      /// -----------------------------------------------------------------
      public void ReadTeam(DTO_TeamRoster ros, int ab) {

         // fMean0 is the fudge factor for hits. 

         // bx is index into the batter matrix, bp.
         // px is index into the pitcher matrix, pp.

         int bx, px;
         int slot, posn;
         string dataVersion;

         string db_stats = "";

         bat = new CBatter[SZ_BAT];
         pit = new CPitcher[SZ_PIT];

         // Line 1: Read the version...
         dataVersion = "V3.0"; //<-- TODO: Add this DTO_TeamRoster

         // Read the team-level data...
         nick = ros.NickName; 
         city = ros.City; 
         lineName = ros.LineName;
         teamTag = ros.Team + ros.YearID.ToString();
         usesDhDefault = ros.UsesDhDefault;

      // This here is obs replaced by Meld calc at ab-time.
      // There is now separate lgMeans for each batter & pitcher
         //FillLgStats(ros.leagueStats, ros.ComplPct, ref lgStats);
         //lgMean.FillLgParas(lgStats);
         //lgMean.h /= CGame.fMean0; // This applies the fudge factor...   

       // Player records (batter & pitcher)...
         bx = 0;
         px = 0;
         CBatter b;
         CPitcher p;
         foreach (var ply in ros.PlayerInfo) { 

            // Logic for if the game itself will use DH. Depends on home (ab=1)
            // or vis (ab=0). It is driven by home. We set g.UsingDh if ab=1.
            // Note: important to call ReadTeam(1) first, then (0).
            {
               bool usingDh = g != null ? g.UsingDh : usesDhDefault;
               if (g == null) {
                  usingDh = usesDhDefault;
               }
               else {
                  switch (ab) {
                     case 0: usingDh = g.UsingDh; break;
                     case 1: usingDh = usesDhDefault; g.UsingDh = usingDh; break;
                  }
               }
               slot = usingDh ? ply.slotdh : ply.slot;
               posn = usingDh ? ply.posnDh : ply.posn;
            }
            bx++;
            if (bx >= SZ_BAT) throw new Exception("Too many batters in " + teamTag);
            b = bat[bx] = new CBatter(g);
            //if (bx > 25) MessageBox.Show("Too many batters in " + sTeam);
            b.bname = ply.UseName;
            b.bname2 = ply.UseName2;
            b.skillStr = ply.SkillStr;
            
            // League-level stuff for this batter... (#2101.01)
            FillLgStats(ply.leagueStats, ros.ComplPct, ref b.lgBr);
            //b.lgPar.FillBatParas(b.lgBr, b.lgPar, ply.Playercategory switch { 'B'=>'1', 'P'=>'2' });
            b.lgPar.FillLgParas(b.lgBr);

            // Batter-level stuff for this batter... (#2101.01)
            FillBatStats(ply.battingStats , ref b.br);
            b.par.FillBatParas(b.br, b.lgPar, ply.Playercategory switch { 'B'=>'1', 'P'=>'2' }); 

            b.when = (slot == 10 ? 0 : slot);
            if (slot > 0 && slot <= 9) linup[slot] = bx; //So 10 (non-hitting pitcher) is slot 0
            b.where = posn; //dh is 10 in the file, keep that.
            if (posn > 0) who[posn] = bx;
            b.used = (slot > 0 || posn > 0);
            b.bx = bx;
            b.px = 0; //See below, this is assigned for pitchers.
            b.sidex = (side)ab; //Tells which team he's on, 0 or 1.

            if (ply.Playercategory == 'P' && ply.pitchingStats != null) {
               // It's a Pitcher record.
               // Note: 'B' type players CAN have pitchingStats, but we ignore that.
               px++; //Initialized to 0, so starts with 1.
               if (px == 1) curp = 1;  //First pitcher listed starts today.
               if (px >= SZ_PIT) throw new Exception("Too many pitchers in " + teamTag);
               p = pit[px] = new CPitcher();
               b.px = px;
               p.pname = ply.UseName;
               p.pname2 = ply.UseName2;

            // League-level stuff for this pitcher... (#2101.01)
               p.lgPar.FillLgParas(b.lgBr);

               FillPitStats(ply.pitchingStats, ref p.pr); //Continue with same value of ptr...
               p.par.FillPitParas(p.pr, b.lgPar); //#2101.01 was lgmean //Yes 'b.' correct here

               b.px = p.px = px;
               p.sidex = (side)ab;
            }

         }
         //f.Close();

      }


      /// <summary>
      /// Breaks down string, stats, using GetHex(), filling br
      /// </summary>
      /// ----------------------------------------------------------------
      private void FillBatStats(DTO_BattingStats stats, ref CBatRealSet br) {

         br.pa = stats.pa;
         br.ab = stats.ab;
         br.hr = stats.hr;
         br.bi = stats.rbi;
         br.sb = stats.sb;
         br.cs = stats.cs;
         br.h = stats.h;
         br.ave = br.ab > 0 ? Math.Round((double)br.h / (double)br.ab, 3) : 0.0;
         br.b2 = stats.b2; ;
         br.b3 = stats.b3;
         br.bb = stats.bb;
         br.ibb = stats.ibb;
         br.so = stats.so;

      }


      private void FillPitStats(DTO_PitchingStats stats, ref CPitRealSet pr) {
      // -------------------------------------------------------------------
      // GetHex returns -1 if db_stats is all F's -- this indicates missing.

         pr.g = stats.g;
         pr.gs = stats.gs;
         pr.w = stats.w;
         pr.l = stats.l;
         pr.bfp = stats.bfp;
         pr.ip3 = stats.ipOuts;
         pr.h = stats.h;
         pr.er = stats.er;
         pr.hr = stats.hr;
         pr.so = stats.so;
         pr.bb = stats.bb;
         pr.ibb = stats.ibb;
         pr.sv = stats.sv;
         pr.era = pr.ip3 == 0.0 ? 0.0 : pr.er / ((double)pr.ip3 / 3.0) * 9.0;
      }


      private void FillLgStats(DTO_BattingStats stats, int complPct, ref CBatRealSet lgStats) {
      // ---------------------------------------------------------------------------
      // Nore: In the database, in Batting table, PA is null for all years until 2020.
      // But the SP that builds LeagueStats table has logic to use ab+bb+hbp+sh+sf is
      // PA is null.
      // ----------------------------------------------------------------------------
      
         lgStats.pa = stats.pa; //See Note, above
         lgStats.ab = stats.ab; 
         lgStats.h = stats.h;
         lgStats.b2 = stats.b2;
         lgStats.b3 = stats.b3;
         lgStats.hr = stats.hr;
         lgStats.bi = stats.rbi;
         lgStats.so = stats.so;
         lgStats.sh = stats.sh;
         lgStats.sf = stats.sf;
         lgStats.bb = stats.bb;
         lgStats.ibb = stats.ibb;
         lgStats.hbp = stats.hbp;
         lgStats.sb = stats.sb;
         lgStats.cs = stats.cs;
         lgStats.ip3 = stats.ipOuts;
         lgStats.complPct = complPct; 

         // Batting ave just a calc...
         lgStats.ave = lgStats.ab == 0 ?
            0.0 :
            Math.Round((double)lgStats.h / (double)lgStats.ab, 3);

      }

   }
}
