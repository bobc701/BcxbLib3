using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using BcxbDataAccess;
using TestBcxbLib;

using BCX.BCXB;
using System.Reflection;

namespace TestBcxbLib {

   class Program {

      static async Task Main(string[] args) {

         //CustTeamsTest ct = new();
         //await ct.GetCustTeamList();
         //await ct.GetCuatTeamRoster();

         string[] files = Assembly.GetExecutingAssembly().GetManifestResourceNames();


         GameTester gt = new();
         await gt.SetupNewGame();
         gt.PlayGame();

      }


      //private static async Task SetupNewGame() {

      //   Console.WriteLine($"Press enter to start loading team data...");
      //   Console.ReadLine();


      //   mGame.EShowResults += delegate (int scenario) {
            
      //      TextToSay[] list1 = mGame.lstResults.ToArray();
      //      string txtResults = "";

      //      foreach (TextToSay txt in list1) {
      //         if (txt.action == 'X') {
      //            txtResults = "";
      //         }
      //         else {
      //            if (txtResults == "") txtResults = txt.msg;
      //            else txtResults += txt.delim + txt.msg;
      //            //if (txt.delay) Thread.Sleep(1200);
      //         }
      //      }
      //      Console.WriteLine(txtResults);
      //      mGame.lstResults.Clear();

      //   };


      //   mGame.SetupEngineAndModel();

      //   mGame.t = new CTeam[2];
      //   mGame.t[0] = new CTeam(mGame);
      //   mGame.t[1] = new CTeam(mGame);

      //   mGame.cmean = new CHittingParamSet();
      //   mGame.PlayState = PLAY_STATE.START;

      //   try {
      //      string tm = "DET";
      //      int yr = 2019;
      //      DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine(tm, yr); //#b2102c
      //      if (ros == null) throw new Exception($"Error: Could not load data for team, Home Team");
      //      mGame.t[1].ReadTeam(ros, 1);
      //   }
      //   catch (Exception ex) {
      //      throw new Exception($"Error loading data for team, Home Team\r\n{ex.Message}");
      //   }

      //   try {
      //      string tm = "BOS";
      //      int yr = 2019;
      //      DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine(tm, yr);
      //      if (ros == null) throw new Exception($"Error: Could not load data for team, Vis Team");
      //      mGame.t[0].ReadTeam(ros, 0);
      //   }
      //   catch (Exception ex) {
      //      throw new Exception($"Error loading data for team, Vis Team\r\n{ex.Message}");
      //   }

      //   mGame.InitGame();
      //   //ShowRHE();               
      //   //InitLinescore();
      //   //ShowFielders(1);
      //   //ShowRunners();

      //   //EnableControls();

      //}


      //private async static void PlayGame() { 

      //   Console.WriteLine($"Press enter to start playing...");
      //   Console.ReadLine();

      //   while (true) {

      //      int result = mGame.Go1();
      //      Console.ReadLine();

      //   }

      //   GameTester gf = new();
      //   await gf.SetupNewGame();
      //   gf.PlayGame();

      //}


   }


}
