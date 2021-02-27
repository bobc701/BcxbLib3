using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace BCX.BCXB {

public class CLineupCard : INotifyPropertyChanged { 
// ===================================================================================== 
  
   /* This is a non-platform specific shadow to the Lineup card form.
    * It has methods for doing all manipulations of the lineup -- pinch hit,
    * pinch run, defensive change, etc.   
    * 
    * This is instantiated when substitutions are being made, not kept as a 
    * property in CGame. The lineup property in this is built on the fly when
    * instantiated. It can be used for data binding in UI's that support it
    * (eg, VS).
    *     
    * User sees a lineupof 9 or 10 players. Listed in batting order, with a non-batting player
    * (pitcher) listed 10th. Each player has a slot snd /or a pos or both. User can select a 
    * player in the lineup and do one of 3 things: 
    *  
    * (1) User says replace player x with y. 
    * User then sees a list of available players. This list will
    * depend on situation: If on defense, list has all unused playrs. Except if replaced
    * player's pos = 1, list only has unused pitchers. If on offense, list has all unused 
    * players. Except if replaced player is not the batter or one of the base runners, 
    * there is no list and the replacement is not allowed. The replaced player goes in
    * with NO pos. User can cancel from the available list, but once accepted it is final.
    * Thus no need for temp structures.
    *  
    * (2) User says change this player's pos. Or assign a pos if none currently.
    * User sees a list of the 9 positions.
    *  
    * (3) Moves player up or down
    * 
    * 
    * Valiadate lineup: IF team is in the field, or about to take the field, the
    * lineup is validated to assure that there is exactly 1 player at each of the 9
    * positions.
    *  
    *  
    * 3 routines accomplish this...
    *
    * (1) ReplacePlayer(x,y) -- Replace player x with y.
    * -------------------------------------------------
    * If x has a slot, y gets that slot. x loses his slot a/o pos.
    * y gets NO pos. y goes into BBox right
    * after x, indented, with NO pos.
    * If x has pos=1, y is required to be a pitcher. <-- NOT. See (2)
    * Even if x's pos = 1, there is no adj to PBox
    *  
    * (2) AssignPos(x,p) -- Assign posn p, to player x.
    * -------------------------------------------------
    * x's pos is changed to p. If p=1, x is required to be a pitcher.
    * If x has a slot, his BBox name is  appended with ",p". If p=1, x is added to 
    * PBox, at the end.
    *  
    * (3) validate lineup. As above.
    */

   public CGame g; 
   public int abMng;
   public ObservableCollection<CBatter> CurrentLineup { get; set; } //Use 0-8 or 0-9.
   public List<CBatter> Available { get; set; }
   public string TeamNickVis { get => g.t[0].nick; }  // New //1906.03
   public string TeamNickHome { get => g.t[1].nick; }  // New //1906.03


   public enum GameState {
      PreGame,
      Offense,
      Defense
   }
   public GameState gameState;
      
   /// <summary>
   /// You pass it a game obj and a side.<para></para> 
   /// The 'side' is the calling team, assigned to abMng.<para></para>
   /// This then determines Offense, Defense or Pregame.<para></para>
   /// Then it call SetLineup so you don't need to call that, and <para></para>
   /// that fills CurrentLineup, a List of CBatter.
   /// </summary>
   ///
   public CLineupCard(CGame g1, side side1) {
   
      g = g1;
      abMng = (int)side1;
      if (g.PlayState == PLAY_STATE.START || g.PlayState == PLAY_STATE.NONE)
         gameState = GameState.PreGame;
      else if (g.ab == abMng) gameState = GameState.Offense;
      else gameState = GameState.Defense;
      SetLineupCard();

      }


      /// <summary>
      /// This fills the structure, CurrentLineup, which is rebuilt on the fly 
      /// here, rather than being incrementally maintained. 
      /// </summary>
      /// 
      public void SetLineupCard() {
   
         CurrentLineup = new ObservableCollection<CBatter>();
         
         for (int i=1; i<=9; i++) {
            int bx = g.t[abMng].linup[i];              
            CurrentLineup.Add(g.t[abMng].bat[bx]);
         }
         
      // Add non-batting pitcher:
         int bxp = g.t[abMng].who[1];
         if (bxp != 0)
            if (g.t[abMng].bat[bxp].when == 0) CurrentLineup.Add(g.t[abMng].bat[bxp]);


         // For debugging...   
         //Debug.WriteLine (g.t[abMng].nick + " ---------------------------------");
         //for (int i=1; i<=CGame.SZ_BAT-1; i++) {
         //if (g.t[abMng].bat[i] != null) Debug.WriteLine(i.ToString() + ": " +  g.t[abMng].bat[i].bname);
         //}

         OnPropertyChanged("CurrentLineup");

      }

   
   public void GetAvailable(char crit) {
   // --------------------------------------------------------------------
   // As overhauled 3'19
   // crit: a=All, p=just pitchers, n=just Non-pitchers
   // Purpose is to fill Available, a List<CBatter>
   // --------------------------------------------------------------------
      //Available = new List<CBatter>();
      IEnumerable<CBatter> avail = null;

      switch (gameState) {
         case GameState.PreGame:
         // List will include all players not in lineup
            avail = g.t[abMng].bat
               .Where
                  (b => b != null && b.when == 0 && b.where == 0 &&
                  (crit == 'a' || crit == 'p' && b.px > 0 || crit == 'n' && b.px == 0))
               .OrderBy(b => b.bx);
            break;

         case GameState.Offense:
         case GameState.Defense:
         // List will include all unused players
            avail = g.t[abMng].bat
               .Where
                  (b => b != null && !b.used &&
                  (crit == 'a' || crit == 'p' && b.px > 0 || crit == 'n' && b.px == 0))
               .OrderBy(b => b.bx);
            break;

         default:
            break;

      }
         //Available = (ObservableCollection<CBatter>)avail;
         Available = avail.ToList();

         //foreach (CBatter b in avail) Available.Add(b); 
   }



   /// <summary>
   /// This is a single method for all substitutions, whether pinch hit
   /// pinch run, fielding change.
   /// It replaces player x with player y.
   /// The position of y is left vacant, so this should be followed by
   /// call to AssignPos if it's a fielding change.
   /// Note that if replacing a non-batting pitcher, this does nothing.
   /// That is all handled by AssignPos.
   /// </summary>
   /// <param name="x">Index of player to be replaced</param>
   /// <param name="y">Index of replacing player</param>
   /// 
   public void ReplacePlayer(int x, int y) {

      int slot0 = g.t[abMng].bat[x].when;
      int pos = g.t[abMng].bat[x].where;

      if (slot0 != 0) {
         g.t [abMng].bat [x].when = 0;
         g.t [abMng].bat [x].where = 0;
      // Replaced player still avail if game not started...
         if (gameState == GameState.PreGame) g.t[abMng].bat[x].used = false; 
         g.t [abMng].bat [y].when = slot0;
         g.t [abMng].bat [y].used = true;
         g.t [abMng].bat [y].where = 0;  //New players go in with NO position: will be set in caller
         g.t [abMng].bat [y].bs.boxName = "  " + g.t [abMng].bat [y].bname2;
         g.t [abMng].linup [slot0] = y;
         g.t [abMng].who [pos] = 0;
         if (gameState == GameState.PreGame)
            g.InitializeBox (abMng);
         else {
            int bboxx = g.t [abMng].bat [x].bbox;
            g.InsertIntoBBox (bboxx, y, abMng);
         }  

      // Replace runners, if any, and batter...
         if (gameState == GameState.Offense) {
            for (int b=0; b<=3; b++) {
               if (g.r[b].ix == x) { g.r[b].ix = y; g.r[b].name = g.t[abMng].bat[ y].bname;}
            }
         }
      }

   }
      
   /// <summary>
   /// Assigns position p to player x
   /// </summary>
   /// 
   public void AssignPos(int x, int p1) {
   // -----------------------------------
   // p0 is player x's current posn
   // p1 is player x's new posn
   // -----------------------------------
      int slot = g.t[abMng].bat[x].when;
      int p0 = g.t[abMng].bat[x].where; //x's current posn
      int y = g.t[abMng].who[p1]; //y is current player at the new posn 

      if (p1 == 1) {
         int px1 = g.t[abMng].bat[x].px;
         if (gameState == GameState.PreGame) g.InitializeBox (abMng);
         else g.InsertIntoPBox(px1, abMng);
         g.t[abMng].curp = px1;
      }

      if (y != 0) g.t[abMng].bat[y].where = 0; //Player y now has no posn 
      if (p0 > 0 && p0 < 10) g.t[abMng].who[p0] = 0; //Put no one at current posn

      g.t[abMng].bat[x].where = p1; //Player x moved to new posn
      if (p1 != 10) g.t[abMng].who[p1] = x; //New posn now has player x.
      if (slot != 0) {
         if (gameState != GameState.PreGame)
            g.t[abMng].bat[x].bs.boxName += "," + CGame.posAbbr[p1];
         else 
            g.t[abMng].bat[x].bs.boxName = g.t[abMng].bat[x].bname2 + "," + CGame.posAbbr[p1];
      }     
   }


   /// <summary>
   /// Move x up or down in lineup.
   /// </summary>
   /// <remarks>
   /// Should only be called when gameState == PreGame.
   /// dir is +1 for moving down, -1 for moving up.
   /// Attempt to move above 1 or below 9 is not an error, but no action occurrs.
   /// 
   /// </remarks>
   /// 
   public void MovePlayerUpDown(int x, int dir) {

      if (gameState != GameState.PreGame) return;
      int slot0 = g.t[abMng].bat[x].when;
      if (dir == -1 && slot0 <= 1) return;
      if (dir == 1 && slot0 >= 9)  return;
      int y = g.t[abMng].linup[slot0 + dir];
      g.t[abMng].bat[x].when += dir;
      g.t[abMng].bat[y].when -= dir;
      g.t[abMng].linup[slot0] = y;
      g.t[abMng].linup[slot0 + dir] = x;
      g.InitializeBox(abMng); //Resets box based on current lineup.
   }

      public event PropertyChangedEventHandler PropertyChanged;
      void OnPropertyChanged([CallerMemberName] string propertyName = "") {

         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }


   }

}