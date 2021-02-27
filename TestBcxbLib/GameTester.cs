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


namespace TestBcxbLib {

   class GameTester {

   // Dependencies...
   // Note: Actually, this would seem to violate dep inj principles,
   // as 'new' s/not be used here. These obj's s/b instantiated o/s
   // the class and passed in through constr or props.
      public CGame mGame = new();
      //public DataAccess dataAccess = new();


      public async Task SetupNewGame() {

         Console.WriteLine($"Press enter to start loading team data...");
         Console.ReadLine();


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


         mGame.SetupEngineAndModel();

         mGame.t = new CTeam[2];
         mGame.t[0] = new CTeam(mGame);
         mGame.t[1] = new CTeam(mGame);

         mGame.cmean = new CHittingParamSet();
         mGame.PlayState = PLAY_STATE.START;

         try {
            DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine("DET", 2019); //#b2102c
            if (ros == null) throw new Exception($"Error: Could not load data for Home Team");
            mGame.t[1].ReadTeam(ros, 1);
         }
         catch (Exception ex) {
            throw new Exception($"Error loading data for Home Team\r\n{ex.Message}");
         }

         try {
            DTO_TeamRoster ros = await DataAccess.GetTeamRosterOnLine("BOS", 2019);
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



   }

}
