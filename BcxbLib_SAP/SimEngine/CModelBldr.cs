using BCX.BCXB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SimEngine
{
   public class CModelBldr 
   {

      public static void LoadModel(string jsonString, CGame gm, CSimEngine sim)
      {
         // This loads a model from a json string into an existing model, 'model'.
         // Caller must specify the model as an arg.

         try
         {
            using (JsonDocument jDocument = JsonDocument.Parse(jsonString))
            {
               JsonElement jRoot = jDocument.RootElement;

               sim.Version += jRoot.GetProperty("Version").GetString() + ",";
               sim.ModelName += jRoot.GetProperty("ModelName").GetString() + ",";
               JsonElement jModel = jRoot.GetProperty("Model");

               foreach (JsonElement jNamedList in jModel.EnumerateArray())
               {
                  string tag = jNamedList[0].GetString();
                  JsonElement jActionList = jNamedList[1];
                  List<BaseSimAction> list1 = BldActionList(jActionList, gm, sim);
                  sim.Model.Add(tag, list1);
               }
            }
         }
         catch (Exception ex) {
            Debug.WriteLine("Error in LoadModel...");
            Debug.WriteLine($"{ex.Message}");
            throw;

         }

      }


      public static List<BaseSimAction> BldActionList(JsonElement jsonList, CGame gm, CSimEngine sim)
      {
         List<BaseSimAction> list1 = new();
         BaseSimAction act = null;
         List<BaseSimAction> alist = null;

         foreach (JsonElement jAction in jsonList.EnumerateArray()) {

            string actionType = jAction[0].GetString();
            switch (actionType) {

               case "DoOne":
                  //act = new DoOneAction(jAction, Gm1);
                  alist = BldActionList(jAction[1], gm, sim);
                  act = new DoOneAction(alist, gm);
                  break;
               case "DItem":
                  double prob = jAction[1].GetDouble();
                  alist = BldActionList(jAction[2], gm, sim);
                  act = new DItemAction(prob, alist, gm);
                  break;
               case "Do":
                  string listIDs = jAction[1].GetString();
                  act = new DoAction(listIDs, sim);
                  break;
               case "GetTlr":
                  int tlr = jAction[1].GetInt32();
                  act = new GetTlrAction(tlr, gm);
                  break;
               case "Gres":
                  int gres = jAction[1].GetInt32();
                  act = new GresAction(gres, gm, sim);
                  break;
               case "Select":
                  alist = BldActionList(jAction[1], gm, sim);
                  act = new SelectAction(alist, gm, sim);
                  break;
               case "SItem":
                  prob = jAction[1].GetDouble();
                  int res = jAction[2].GetInt32();
                  act = new SItemAction(prob, res, gm);
                  break;
               case "Comment":
                  string text = jAction[1].GetString();
                  act = new CommentAction(text);
                  break;
               case "BatDis":
                  int disp = jAction[1].GetInt32();
                  act = new BatDisAction(disp, gm);
                  break;
               case "Say":
                  text = jAction[1].GetString();
                  act = new SayAction(text, gm);
                  break;
               case "Say1":
                  text = jAction[1].GetString();
                  act = new SayAction(text, gm);
                  break;
               case "Adv":
                  string bases = jAction[1].GetString();
                  act = new AdvAction(bases, sim);
                  break;
               case "Err":
                  int pos = jAction[1].GetInt32();
                  act = new ErrAction(pos, gm);
                  break;
               case "Homer":
                  act = new HomerAction(gm);
                  break;
               case "Same":
                  act = new SameAction(gm);
                  break;
               case "GPlay":
                  int n = jAction[1].GetInt32();
                  act = new GPlayAction(n, gm);
                  break;
               case "Pos":
                  act = new PosAction(gm, sim);
                  break;
               case "SacBunt":
                  act = new SacBuntAction(gm);
                  break;
               case "Squeeze":
                  act = new SqueezeAction(gm);
                  break;
               default:
                  break;
            }
            if (act != null) list1.Add(act);
         }

         return list1;

      }



   }
}
