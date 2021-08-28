using BCX.BCXB;
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
      private readonly CSimEngine Sim;


      public AdvAction(string bases, CSimEngine sim)
      {
         this.AType = TAction.Adv;
         this.Bases = bases;
         this.Sim = sim;
      }

      public override int DoIt()
      {
         Sim.DoAction(this);

         var a = this.Bases.Split();
         int a0 = int.Parse(a[0]);
         int a1 = int.Parse(a[1]);
         char a2 = a[2][0];
         char a3 = a[3][0];

         Debug.WriteLine($"Adv: {a0},{a1},{a2},{a3}");
         g.Advance(a0, a1, a2, a3);
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
      private readonly CGame g;

      public BatDisAction(int disp, CGame game)
      {
         AType = TAction.BatDis;
         Disp = disp;
         g = game;
      }

      public override int DoIt()
      {
         g.BatDis(this.Disp);
         Debug.WriteLine($"Doing BatDis: {this.Disp}");
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
      private readonly CSimEngine Sim;


      public ChooseAction(string choices, CSimEngine sim)
      {
         this.AType = TAction.Choose;
         this.Choices = choices;
         this.Sim = sim;
      }

      public override int DoIt()
      {
         Sim.DoAction(this);

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
      private readonly CGame g;

      public DItemAction(double prob, List<BaseSimAction> alist, CGame game)
      {
         this.AType = TAction.DItem;
         this.Prob = prob;
         this.g = game;
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

      public DoAction(string listIDs, CSimEngine model)
      {
         this.AType = TAction.Do;
         this.ListIDs = listIDs;
         this.Model = model;
      }

      public override int DoIt()
      {  
      // Handle this here, since no ref to CGame...
         Console.WriteLine($"Doing Do: {ListIDs}");
         string[] a = ListIDs.Split(',').Select(e => e.Trim()).ToArray();
         foreach (string a1 in a) {
            Debug.Indent();
            this.Model.DoNamedList(a1);
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
      private readonly CSimEngine mSim;

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
      private readonly CGame g;

      public ErrAction(int pos, CGame game) {
         this.AType = TAction.Err;
         this.Pos = pos;
         this.g = game;
      }

      public override int DoIt() {
         g.err1(Pos);
         Debug.WriteLine($"aDoing ErrActionr: {Pos}");
         return 0;
      }

      public override void PrintIt() {
         Debug.WriteLine($"{AType.ToString()}: Pos:{this.Pos}");
      }

   }


   public class GetTlrAction : BaseSimAction 
   {
      public int Tlr { get; set; }
      private readonly CGame g;

      public GetTlrAction(int tlr, CGame game) 
      {
         this.AType = TAction.GetTlr;
         this.Tlr = tlr;
         this.g = game;
      }

      public override int DoIt()
      {
         g.diceRollBatting = g.cpara.GetTlr((TLR)Tlr, g.rn);
         Debug.WriteLine(
            $@"GetTlr: 
               pointInBracket={g.diceRollBatting.pointInBracket:#0.0000}, 
               topLevelResult={g.diceRollBatting.topLevelResult}");
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: tlr:{Tlr}");
      }

   }


   public class GPlayAction : BaseSimAction
   {  
      public int Play { get; set; }
      private readonly CGame g;

      public GPlayAction(int n, CGame game)
      {
         this.AType = TAction.GPlay;
         this.Play = n;
         this.g = game;
      }

      public override int DoIt()
      {
         Debug.WriteLine($"Doing Gplay: {this.Play}");
         g.Gplay = Play;
         //g.BatDis(Disp); //TODO: In BcxbLib, make public
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: n:{this.Play}");
      }


   }


   public class GPlaysAction : BaseSimAction
   {
   // This is string version of GPlay
      public string Play { get; set; }
      private readonly CGame g;

      public GPlaysAction(string s, CGame game)
      {
         this.AType = TAction.GPlay;
         this.Play = s;
         this.g = game;
      }

      public override int DoIt()
      {
         Debug.WriteLine($"Doing Gplay: {this.Play}");
         g.Gplay = Play;
         //g.BatDis(Disp); //TODO: In BcxbLib, make public
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: n:{this.Play}");
      }


   }



   public class GresAction : BaseSimAction
   {
      public int Res { get; set; }
      private readonly CGame g;
      private readonly CSimEngine Sim;

      public GresAction(int res, CGame game, CSimEngine sim)
      {
         this.AType = TAction.GRes;
         this.Res = res;
         this.g = game;
         this.Sim = sim;

      }

      public override int DoIt()
      {
         try {
            Debug.WriteLine($"Doing GetTlr: {Res}");
            g.genericResult = Res;
            int a = g.Gres[Res, g.onsit];
            Sim.DoNamedList("n" + a.ToString());
            return 0;
         }
         catch (Exception ex) {
            throw new Exception("Error in GresAction.DoIt", ex);
         }
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}: res:{Res}");
      }

   }


   public class HomerAction : BaseSimAction
   {
      private readonly CGame g;

      public HomerAction(CGame game)
      {
         this.AType = TAction.Homer;
         this.g = game;
      }

      public override int DoIt()
      {
         Console.WriteLine("Doing Homer");
         g.Homer = true; 
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }


   public class PosAction : BaseSimAction
   {
      private readonly CGame g;
      private readonly CSimEngine s;

      public PosAction(CGame game, CSimEngine sim) 
      {
         this.AType = TAction.Pos;
         this.g = game;
         this.s = sim;
      }

      public override int DoIt()
      {
      // Task: Fill g.Posn based on g.Gplay...
         Debug.WriteLine($"Doing DoOne: Gplay={g.Gplay}");
         if (g.Gplay < 1 || g.Gplay > 7)
            throw new Exception($"Invalid Gplay, {g.Gplay}, in PosAction.DoIt");
         string listName = g.Gplay
            switch {
               1 => "PosPopup",
               2 => "PosFoulPop",
               3 => "PosGrounder",
               4 => "PosFlyBall",
               5 => "PosLDtoIF",
               6 => "PosLDtoOF",
               7 => "PosLongFly"
            };
         BaseSimAction act = s.Model[listName][0]; //We wanr the 1st action in the list
         if (act is not SelectAction)
            throw new Exception("Expected SelectAction in PosAction.DoIt");
         g.Posn = ((SelectAction)act).DoIt(); 
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }


   public class SacBuntAction : BaseSimAction
   {
      private readonly CGame g;

      public SacBuntAction(CGame game)
      {
         this.AType = TAction.SacBunt;
         this.g = game;
      }

      public override int DoIt()
      {
         Console.WriteLine("Doing DoOne");
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }


   public class SameAction : BaseSimAction
   {
      private CGame g;

      public SameAction(CGame game)
      {
         this.AType = TAction.Same;
         this.g = game;
      }

      public override int DoIt()
      {
         Console.WriteLine("Doing DoOne");
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
      public readonly string Text;
      public CSimEngine mSim;

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
      private readonly string Text;
      private CGame g;

      public Say1Action(string text, CGame game)
      {
         this.AType = TAction.Say1;
         //this.SayIx = 0;
         //this.SayIx = SimulationModel.GetSayIndex(jAction.GetProperty("text").ToString());
         this.Text = text;
         this.g = game;
      }

      public override int DoIt()
      {
         //Console.WriteLine($"Doing Say: {SimulationModel.SayList[SayIx]}");
         Console.WriteLine($"Doing Say1: {Text}");
         g.Say(Text);
         return 0;
      }


      public override void PrintIt()
      {
         //Debug.WriteLine($"{ATag}, text: {SimulationModel.SayList[SayIx]}, SayIx: {SayIx}");
         Debug.WriteLine($"{AType.ToString()}:, Text:{Text}");
      }

   }


   public class SelectAction : BaseSimAction
   {
      public List<BaseSimAction> AList { get; set; }
      private CGame g;
      private CSimEngine s;

      public SelectAction(List<BaseSimAction> alist, CGame game, CSimEngine sim)
      {
         this.AType = TAction.Select;
         this.AList = alist;
         this.g = game;
         this.s = sim;

      }

      public override int DoIt()
      {
         Console.WriteLine("Doing DoOne");
         double r = g.rn.NextDouble();
         double cum = 0.0;
         foreach (BaseSimAction item in AList) {
            if (item is not SItemAction)
               throw new Exception("Expected SItemAction in SelectAction.DoIt");
            if (r <= (cum += ((SItemAction)item).Prob)) 
               return ((SItemAction)item).Res;
         }
         throw new Exception($"Result not found in SelectList.DoIt");
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

      public SItemAction(double prob, int res, CGame game)
      {
         this.AType = TAction.SItem;
         this.Prob = prob;
         this.Res = res;
         this.g = game;
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


   public class SSqueezeAction : BaseSimAction
   {
      private CGame g;

      public SSqueezeAction(CGame game)
      {
         this.AType = TAction.SSqueeze;
         this.g = game;
      }

      public override int DoIt()
      {
         Console.WriteLine("Doing DoOne");
         return 0;
      }

      public override void PrintIt()
      {
         Debug.WriteLine($"{AType.ToString()}");
      }

   }



}

