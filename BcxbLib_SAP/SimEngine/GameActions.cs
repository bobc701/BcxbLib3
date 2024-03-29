﻿using BCX.BCXB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace SimEngine
{

   //public enum TAction {

   //   listdef = 1,
   //   DoOne = 2,
   //   Select = 3,
   //   Do = 4,
   //   DItem = 5,
   //   Say = 6,
   //   Say1 = 7,
   //   Adv = 8,
   //   BatDis = 9,
   //   Err = 10,
   //   Pos = 11,
   //   GPlay = 12,
   //   GPlayS = 13,
   //   Choose = 14,
   //   Same = 15,
   //   SacBunt = 16,
   //   SSqueeze = 17,
   //   Homer = 18,
   //   GRes = 19,
   //   SItem = 20,
   //   DoOneIx = 21,
   //   CalcTlr = 22,
   //   CalcTlrSteal = 23,
   //   CalcTlrStealHome = 24,
   //   GetTlr = 25,
   //   Endlistdef = 31,
   //   EndDItem = 35,
   //   EndDoOne = 32,
   //   EndSelect = 33,
   //   EndDoOneIx = 36,
   //};


   public class AdvAction : BaseSimAction
   {
      public string Bases { get; set; }


      public AdvAction(string bases, CSimEngine sim)
      {
         this.AType = TAction.Adv;
         this.Bases = bases;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this actionin CGme...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         //Debug.WriteLine($"{ATag}, text: {SimulationModel.SayList[SayIx]}, SayIx: {SayIx}");
         Debug.WriteLine($"{AType.ToString()}:, {this.Bases}");
      }

   }


   public class BatDisAction : BaseSimAction
   {
      public int Disp { get; set; }

      public BatDisAction(int disp, CSimEngine sim) 
      {
         AType = TAction.BatDis;
         Disp = disp;
         mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this action in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}:, {this.Disp}");
      }

   }

   public class ChooseAction : BaseSimAction
   {
      public string Choices { get; set; }


      public ChooseAction(string choices, CSimEngine sim)
      {
         this.AType = TAction.Choose;
         this.Choices = choices;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this action in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         //Debug.WriteLine($"{ATag}, text: {SimulationModel.SayList[SayIx]}, SayIx: {SayIx}");
         Debug.WriteLine($"{AType.ToString()}:, {this.Choices}");
      }

   }

      public class CommentAction : BaseSimAction
      {
         public string Text;

         public CommentAction(string text) 
         {
            this.AType = TAction.Comment;
            this.Text = text;
         }

         public override int DoIt()
         {
            Debug.WriteLine("Doing Comment");
            return 0;
         }

         public override void PrintIt()
         {
            Debug.WriteLine($"{AType.ToString()}: {this.Text}");
         }

      }


   public class DItemAction : BaseSimAction
   {
      public double Prob { get; set; }
      public List<BaseSimAction> AList { get; set; }

      public DItemAction(double prob, List<BaseSimAction> alist, CSimEngine sim)
      {
         this.AType = TAction.DItem;
         this.Prob = prob;
         this.mSim = sim;
         this.AList = alist;

      }

      public override int DoIt()
      {
      // Handle this here, not in CGame...
         Debug.WriteLine($"Doing DoItem:{AType.ToString()} Prob:{Prob}");
         foreach (BaseSimAction act in this.AList) 
            act.DoIt();
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: Prob: {Prob} AList:");
         CSimEngine.PrintActionList(AList);

      }


   }


   public class DoAction : BaseSimAction
   {
      public string ListIDs { get; set; }
      private CSimEngine Model { get; set; } 

      public DoAction(string listIDs, CSimEngine sim)
      {
         this.AType = TAction.Do;
         this.ListIDs = listIDs;
         this.mSim = sim;
      }

      public override int DoIt()
      {  
      // Handle this here, since no ref to CGame...
         Debug.WriteLine($"Doing Do: {ListIDs}");
         string[] a = ListIDs.Split(',').Select(e => e.Trim()).ToArray();
         foreach (string a1 in a) {
            Debug.Indent();
            this.mSim.DoNamedList(a1);
            Debug.Unindent();
         }
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: ListIDs: {this.ListIDs}");
      }

   }


   public class DoOneAction : BaseSimAction
   {
      public List<BaseSimAction> AList { get; set; }

      public DoOneAction(List<BaseSimAction> alist, CSimEngine sim) 
      {
         this.AType = TAction.DoOne;
         this.mSim = sim;
         this.AList = alist;
      }


      public override int DoIt()
      {
      // This handles the action in CGame...
         mSim.DoAction(this);
         return 0;
      }


      public override void PrintIt()
      {
         Debug.WriteLine($"{this.AType}: AList:");
         CSimEngine.PrintActionList(AList);
      }

   }


   public class ErrAction : BaseSimAction
   {
      public int Pos { get; set; }

      public ErrAction(int pos, CSimEngine sim) {
         this.AType = TAction.Err;
         this.Pos = pos;
         this.mSim = sim;
      }

      public override int DoIt() {
      // Handle this action in client (CGame)...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt() {
         Debug.WriteLine($"{AType.ToString()}: Pos:{this.Pos}");
      }

   }


   public class GetTlrAction : BaseSimAction 
   {
      public int Tlr { get; set; }

      public GetTlrAction(int tlr, CSimEngine sim) 
      {
         this.AType = TAction.GetTlr;
         this.Tlr = tlr;
         this.mSim = sim;
      }

      public override int DoIt()
      {
         // Handle this action in client
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: tlr:{Tlr}");
      }

   }


   public class GPlayAction : BaseSimAction
   {  
      public int PlayNum { get; set; }

      public GPlayAction(int n, CSimEngine sim)
      {
         this.AType = TAction.GPlay;
         this.PlayNum = n;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: n:{this.PlayNum}");
      }


   }


   public class GPlaysAction : BaseSimAction
   {
   // This is string version of GPlay
      public string PlayName { get; set; }

      public GPlaysAction(string s, CSimEngine sim)
      {
         this.AType = TAction.GPlay;
         this.PlayName = s;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: n:{this.PlayName}");
      }


   }


   public class GresAction : BaseSimAction
   {
      public int Res { get; set; }

      public GresAction(int res, CSimEngine sim)
      {
         this.AType = TAction.GRes;
         this.Res = res;
         this.mSim = sim;

      }

      public override int DoIt()
      {
         // Handle this action in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: res:{Res}");
      }

   }


   public class HomerAction : BaseSimAction
   {

      public HomerAction(CSimEngine sim)
      {
         this.AType = TAction.Homer;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this action in client (CGame)...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }


   public class PosAction : BaseSimAction
   {
      public PosAction(CSimEngine sim) 
      {
         this.AType = TAction.Pos;
         this.mSim = sim;
      }

      public override int DoIt()
      {
         // Task: Fill g.Posn based on g.Gplay...
         //Debug.WriteLine($"Doing DoOne: Gplay={g.Gplay}");
         //if (g.Gplay < 1 || g.Gplay > 7)
         //   throw new Exception($"Invalid Gplay, {g.Gplay}, in PosAction.DoIt");
         //string listName = g.Gplay
         //   switch {
         //      1 => "PosPopup",
         //      2 => "PosFoulPop",
         //      3 => "PosGrounder",
         //      4 => "PosFlyBall",
         //      5 => "PosLDtoIF",
         //      6 => "PosLDtoOF",
         //      7 => "PosLongFly"
         //   };
         //BaseSimAction act = s.Model[listName][0]; //We wanr the 1st action in the list
         //if (act is not SelectAction)
         //   throw new Exception("Expected SelectAction in PosAction.DoIt");
         //g.Posn = ((SelectAction)act).DoIt(); 

      // Handle this in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }


   public class SameAction : BaseSimAction
   {
      public SameAction(CSimEngine sim)
      {
         this.AType = TAction.Same;
      }

      public override int DoIt()
      {
      // Handle this in client (eg, CGame)...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }



   public class SayAction : BaseSimAction
   {
      //public int SayIx { get; } //May want this in future
      public string Text { get; }

      public SayAction(string text, CSimEngine sim)
      {
         this.AType = TAction.Say;
         //this.SayIx = 0;
         //this.SayIx = SimulationModel.GetSayIndex(jAction.GetProperty("text").ToString());
         this.Text = text;
         this.mSim = sim;
      }

      public override int DoIt()
      {
         //Console.WriteLine($"Doing Say: {SimulationModel.SayList[SayIx]}");

         // Handle this action in CGame...
         mSim.DoAction(this);
         return 0;
      }

      public override void PrintIt()
      {
         //Debug.WriteLine($"{ATag}, text: {SimulationModel.SayList[SayIx]}, SayIx: {SayIx}");
         Debug.WriteLine($"{AType.ToString()}:, Text:{Text}");
      }

   }


   public class Say1Action : BaseSimAction
   {
   // Say1 is same as Say, but is only 'said' if < 2 outs.

      //public int SayIx { get; } //May want this in future
      public string Text { get; }
      public CSimEngine mSim { get; }

      public Say1Action(string text, CSimEngine sim)
      {
         this.AType = TAction.Say1;
         //this.SayIx = 0;
         //this.SayIx = SimulationModel.GetSayIndex(jAction.GetProperty("text").ToString());
         this.Text = text;
         this.mSim = sim;
      }

      public override int DoIt()
      {
      // Handle this in CGame since it ref's 'ok' of CGame.
         mSim.DoAction(this);
         return 0;
      }


      public override void PrintIt()
      {
         //Debug.WriteLine($"{ATag}, text: {SimulationModel.SayList[SayIx]}, SayIx: {SayIx}");
         Debug.WriteLine($"{AType.ToString()}: Text:{Text}");
      }

   }


   public class SelectAction : BaseSimAction
   {
      public List<BaseSimAction> AList { get; set; }

      public SelectAction(List<BaseSimAction> alist, CSimEngine sim)
      {
         this.AType = TAction.Select;
         this.AList = alist;
         this.mSim = sim;

      }

      public override int DoIt()
      {
         // Select actions are not called via the model. 
         // Rather they are accessed in code through the ListName,
         // And the code returns an int.
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: AList:");
         CSimEngine.PrintActionList(AList);
      }

   }


   public class SItemAction : BaseSimAction
   {
      public double Prob { get; set; }
      public int Res { get; set; }
      private CGame g;

      public SItemAction(double prob, int res, CSimEngine sim)
      {
         this.AType = TAction.SItem;
         this.Prob = prob;
         this.Res = res;
      }

      public override int DoIt()
      {
      // DoIt should not be called. These objects are handled by parent,
      // SelectAction.
         Console.WriteLine($"Doing GetTlr: {Res}");
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: prob: {this.Prob} res: {this.Res}");
      }

   }


}

