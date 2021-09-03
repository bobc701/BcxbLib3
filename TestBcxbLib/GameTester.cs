/* --------------------------------------------------------
 * This approach is what BcxbWin currently does, just
 * does file I/O from known folder locations on disk.
 * (As opp to what BcxbXf which uses Embedded Resource files
 * and GetManifestResourceStream(). )
 * --------------------------------------------------------
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using BcxbDataAccess;
using BCX.BCXB;
using Newtonsoft.Json;
using SimEngine;
using System.Reflection;

namespace TestBcxbLib {

   class CustTeamsTest {

      public async Task GetCuatTeamRoster() {

         int teamID = 5007;
         Console.WriteLine($"Press enter to get custom team {teamID}...");
         Console.ReadLine();


         try {
            DTO_TeamRoster ros = await DataAccess.GetCustTeamRoster(teamID); 
            if (ros is null) Console.WriteLine($"Error: Could not load data for Home Team");
            string sros = JsonConvert.SerializeObject(ros,Formatting.Indented);
            Console.WriteLine(sros);
         }
         catch (Exception ex) {
            Console.WriteLine($"Error loading data for custom team {teamID}\r\n{ex.Message}");
         }

      }


      public async Task GetCustTeamList() {

         string userName = "bobc";
         Console.WriteLine($"Press enter to get list of custom teams for {userName}...");
         Console.ReadLine();

         try {
            List<CTeamRecord> list = await DataAccess.GetCustTeamListForUser(userName);
            if ((list?.Count ?? 0) == 0) throw new Exception($"Error: No teams found for {userName}");
            else {
               string slist = JsonConvert.SerializeObject(list, Formatting.Indented);
               Console.WriteLine(slist);
            }
         }
         catch (Exception ex) {
            Console.WriteLine($"Error loading data for user {userName}\r\n{ex.Message}");
         }

      }
   }

   class GameTester {

   // Dependencies...
   // Note: Actually, this would seem to violate dep inj principles,
   // as 'new' s/not be used here. These obj's s/b instantiated o/s
   // the class and passed in through constr or props.

      public CGame mGame { get; set; }
      //public DataAccess dataAccess = new();


      public async Task SetupNewGame() {


         Console.WriteLine($"Press enter to start loading team data...");
         Console.ReadLine();

         mGame = new CGame();


      // Step 1. Load the engine
      // -----------------------
         CSimEngine sim = new();
         sim.RaiseHandler += mGame.DoSimAction;

         string jsonString;
         jsonString = ResourceReader.ReadEmbeddedRecouce("TestBcxbLib.Resources.Model.model1.json");
         CModelBldr.LoadModel(jsonString, sim);

         //jsonString = ResourceReader.ReadEmbeddedRecouce("TestBcxbLib.Resources.Model.model1.json");
         //CModelBldr.LoadModel(jsonString1, sim);

         //jsonString = ResourceReader.ReadEmbeddedRecouce("TestBcxbLib.Resources.Model.model1.json");
         //CModelBldr.LoadModel(jsonString1, sim);

         //jsonString = ResourceReader.ReadEmbeddedRecouce("TestBcxbLib.Resources.Model.model1.json");
         //CModelBldr.LoadModel(jsonString1, sim);


         mGame.mSim = sim; // Here we 'inject' the dependancy into the CGame obj.


      // Hmm.. This looks to be obsolete!
      // Needed by mGame, but not used?
         GFileAccess fileAccess = new();
         mGame.fileAccess = fileAccess; // Here we "inject" the dependancy, GFileAccess.


      // Step 2. Assign eventhandlers (we just do one)
      // -------------------------------------
         mGame.EShowResults += delegate (int scenario) {

            TextToSay[] list1 = mGame.lstResults.ToArray();
            string txtResults = "";

            foreach (TextToSay txt in list1) {
               if (txt.action == 'X') {
                  txtResults = "";
               }
               else {
                  if (txtResults == "") txtResults = txt.msg;
                  else txtResults += txt.delim + txt.msg;
                  //if (txt.delay) Thread.Sleep(1200);
               }
            }
            Console.WriteLine(txtResults);
            mGame.lstResults.Clear();

         };


      // Step 3. Load the team data
      // ---------------------------

         mGame.t = new CTeam[2];
         mGame.t[0] = new CTeam(mGame);
         mGame.t[1] = new CTeam(mGame);

         mGame.cmean = new CHittingParamSet();
         mGame.PlayState = PLAY_STATE.START;

      // Get the 2 new teams from the user... 
         (string, int)[] newTeams = PickTeams();

         try {
            (string tm, int yr) = newTeams[1]; // Do home team first!
            DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine(tm, yr); //#b2102c
            if (ros == null) throw new Exception($"Error: Could not load data for Home Team");
            mGame.t[1].ReadTeam(ros, 1);
         }
         catch (Exception ex) {
            throw new Exception($"Error loading data for Home Team\r\n{ex.Message}");
         }

         try {
            (string tm, int yr) = newTeams[0]; // Visiting team second.
            DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine(tm, yr);
            if (ros == null) throw new Exception($"Error: Could not load data for Vis Team");
            mGame.t[0].ReadTeam(ros, 0);
         }
         catch (Exception ex) {
            throw new Exception($"Error loading data for Vis Team\r\n{ex.Message}");
         }

         mGame.InitGame();
         //ShowRHE();               
         //InitLinescore();
         //ShowFielders(1);
         //ShowRunners();

         //EnableControls();

      }


      public void PlayGame() {

         Console.WriteLine($"Press enter to start playing...");
         Console.ReadLine();

         while (true) {

            int result = mGame.Go1();
            Console.ReadLine();

         }

      }


      public (string, int)[] PickTeams() {

         var teams = new (string, int)[2] { ("DET", 2019), ("BOS", 2019) };
         return teams;
      }

   }

}
