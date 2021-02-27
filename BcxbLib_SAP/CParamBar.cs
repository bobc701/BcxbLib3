using System;
using System.Windows.Forms;
using System.Collections.Generic;



namespace BCX.BCXB {

   /// --------------------------------------------------------------------
   /// <remarks>
   /// This is an abstraction of a parameter bar.
   /// Main member is an XArray of BarSegment objects, BarSegments.
   /// (BarSegment is also abstract.)
   ///  
   /// The basic idiom is this...
   /// 1. You call the constructor, passing array of labels.
   /// 2. You call FmtBarSegments passing a CParamSet.
   /// 3. You call FmtPhysicalSegments(passing height, like screen width, 
   ///    top (like top of the labels).
   ///    
   /// As for the dice pointer, after doing the above, if you want to show
   /// a dice pointer...
   /// 1. You call SetPhysicalPointer, passing a PictureBox.
   /// 2. You call PlaceDicePointer, passing ptInBracket, TLR, and 
   ///    optionally, visibility.
   ///     
   /// Dice.pointerOffset is location of tip of the pointer measured from the 
   /// left edge of the image control. If pointer is at far left, then
   /// offset will be 0, if at far right, will equal width.</para>
   /// </remarks>
   /// --------------------------------------------------------------------
   /// 
   public class CParamBar {

      public Bcx.Util.XArray<int, BarSegment> BarSegments = new Bcx.Util.XArray<int, BarSegment>(8);
      private struct Dice { public int t, w, h, pointerOffset, l;}

      /// <remarks>
      /// Here are the main state properties.
      /// </remarks>
      private CParamSet param1;
      private Dice dice1;
      private int barHeight, barTop, barWidth;
      
      /// <remarks>
      /// The physical stuff... an array of Labels, and a dice pointer.
      /// </remarks>
      private Label[] physicalSegments;
      public PictureBox physicalPointer; 
      private Control physicalDescriptor;

      /// <summary>
      /// Constructor that takes height, width & top.
      /// This constructor is not currently used.
      /// </summary>
      /// 
      public CParamBar(int height, int width, int top) {

         barHeight = height;
         barTop = top;
         barWidth = width; 
      }


      /// <summary>
      /// Constructor that takes an array of labels.
      /// Second arg is label used for descriptor.
      /// This is the constructor that is used currently.</summary>
      /// 
      public CParamBar(Label[] bar, Control lblDescriptor) {

         this.physicalSegments = bar;
         this.physicalDescriptor = lblDescriptor;
         //physicalDescriptor.Text = "Hello";
      }


      /// <summary>
      /// This sets up the abstraction of the bar & its segments.
      /// Uses the arg, par, to format the elements<br />
      /// of BarSegments, wrt width, height, and left.
      /// </summary>
      /// <param name="par">CParamSet object used to format the param bar.</param>
      /// <remarks>This is where the order is set of hr, 3b, ..., other, so.</remarks>
      /// 
      public void FmtBarSegments(CParamSet par, int height, int width, int top) {

         const double virtualWidth = 1.0;
         double w1 = 0.0;
         param1 = par;

         double[] w = param1.GetWidthArray();
         int n = w.Length;
         
         BarSegments[0].width = 0.0;
         BarSegments[0].left = 0.0;
         for (int i=1; i<=n-1; i++) {
            BarSegments[i].width = w[i];
            BarSegments[i].left = BarSegments[i-1].left + w[i-1];
         }

         barHeight = height;
         barTop = top;
         barWidth = width;

         for (int i = 1; i <= physicalSegments.Length; i++) {
            physicalSegments[i-1].Left = (int)(BarSegments[i].left * (double)barWidth);
            physicalSegments[i-1].Width = (int)(BarSegments[i].width * (double)barWidth);
            physicalSegments[i-1].Top = top;
            physicalSegments[i-1].Height = height;
            if (physicalSegments[i-1].Width == 0) physicalSegments[i-1].Width = 1;
            physicalSegments[i-1].Visible = true;
            physicalSegments[i-1].Refresh();
         }

                               
         //// First, compute the widths...
         //w1 += BarSegments[(int)TLR.hr].width = w[0]; //par.h * par.hr * virtualWidth;
         //BarSegments[(int)TLR.b3].width = par.h * par.b3 * virtualWidth;
         ////if (BarSegments[(int)TLR.b3].width < 1) BarSegments[(int)TLR.b3].width = 1;
         //w1 += BarSegments[(int)TLR.b3].width;
         //w1 += BarSegments[(int)TLR.b2].width = par.h * par.b2 * virtualWidth;
         //w1 += BarSegments[(int)TLR.b1].width =
         //   par.h * virtualWidth
         //   - BarSegments[(int)TLR.b2].width - BarSegments[(int)TLR.b3].width - BarSegments[(int)TLR.hr].width;
         //w1 += BarSegments[(int)TLR.bb].width = par.bb * virtualWidth;
         //w1 += BarSegments[(int)TLR.so].width = par.so * virtualWidth;
         //BarSegments[(int)TLR.oth].width = virtualWidth - w1;

         // Next, comput the left edge's (this establishes the display order)...
         //double left = 0.0;
         //BarSegments[(int)TLR.hr].left = left;
         //BarSegments[(int)TLR.b3].left = (left += BarSegments[(int)TLR.hr].width);
         //BarSegments[(int)TLR.b2].left = (left += BarSegments[(int)TLR.b3].width);
         //BarSegments[(int)TLR.b1].left = (left += BarSegments[(int)TLR.b2].width);
         //BarSegments[(int)TLR.bb].left = (left += BarSegments[(int)TLR.b1].width);
         //BarSegments[(int)TLR.oth].left = (left += BarSegments[(int)TLR.bb].width);
         //BarSegments[(int)TLR.so].left = virtualWidth - BarSegments[(int)TLR.so].width;
      }


      public void SetBarLabels(string[] labels) {

         for (int i = 0; i <= physicalSegments.Length-1; i++) {
            physicalSegments[i].Text = labels[i];
            physicalSegments[i].Refresh();
         }

      }


      public void SetDice(int height, int width, int offset) {

         dice1.h = height;
         dice1.w = width;
         dice1.pointerOffset = offset;
         dice1.t = barTop - height;
      }


      public void SetPhysicalSegments(Label[] bar) {
      
         this.physicalSegments = bar;
      }


      /// <summary>
      /// Once you've called FmtBarSegments, you call this to physically put
      /// the bar on the screen.</summary>
      /// <remarks>
      /// This is obsolete as it has been subsumed into FmtbarSegments</remarks>
      /// 
      public void FmtPhysicalSegments(int height, int width, int top) {

         barHeight = height;
         barTop = top;
         barWidth = width;

         for (int i = 1; i <= physicalSegments.Length; i++) {
            physicalSegments[i-1].Left = (int)(BarSegments[i].left * (double)barWidth);
            physicalSegments[i-1].Width = (int)(BarSegments[i].width * (double)barWidth);
            physicalSegments[i-1].Top = top;
            physicalSegments[i-1].Height = height;
            physicalSegments[i-1].Visible = true;
            if (physicalSegments[i-1].Width == 0) physicalSegments[i-1].Width = 1;
         }
      }


      public void Hide() {

         for (int i = 1; i <= physicalSegments.Length; i++) {
            physicalSegments[(int)i - 1].Visible = false;
            physicalSegments[(int)i - 1].Refresh();
         }
      }      

    
      /// <summary>
      /// Call this to tell it what PictureBox to use as the dice pointer.
      /// </summary>
      /// --------------------------------------------------------------
      public void SetPhysicalPointer(PictureBox pic) {

         this.physicalPointer = pic;
         this.dice1.pointerOffset = pic.Width; //This assumes arrow is on the right
      }


      /// <summary>
      /// Call this to tell it what TextBox to use for the bar descriptor.
      /// </summary>
      /// --------------------------------------------------------------
      public void SetPhysicalDescriptor(Label txtBox) {

         this.physicalDescriptor = txtBox;
      }


      /// <summary>
      /// Call this, on the other hand, to actually update the text in
      /// the descriptor.
      /// Examples: "Smith pitching to Jones..." --or-- "Jones fielding...".
      /// </summary>
      /// 
      public void UpdPhysicalDescriptor(string s) {

         this.physicalDescriptor.Text = s;
         this.physicalDescriptor.Visible = true;

      }


      public void HidePhysicalDescriptor() {

         this.physicalDescriptor.Text = "";
         this.physicalDescriptor.Visible = false;
   
      }

      /// <summary>
      /// This is how you position the dice pointer...
      /// </summary>
      /// ------------------------------------------------------------------
      public void PlaceDicePointer(CDiceRoll diceRoll, bool vis) {

         int l = GetDicePointerLocation(diceRoll);
         physicalPointer.Left = l;
         physicalPointer.Visible = vis;
      }


      private int GetDicePointerLocation(CDiceRoll diceRoll) {

         int tlr = (int)diceRoll.topLevelResult;
         double pib = diceRoll.pointInBracket;

         double l = BarSegments[tlr].left * (double)barWidth;
         double w = BarSegments[tlr].width * (double)barWidth;
         dice1.l = (int)(l + pib * w) - dice1.pointerOffset;
         return dice1.l;
      }



   }




}
