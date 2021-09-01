using System;

namespace BCX.BCXCommon
{
	/// <summary>
	/// Summary description for CBCXCommon.
	/// </summary>
	public class CBCXCommon
	{
		public CBCXCommon()
		{
			//
			// TODO: Add constructor logic here
			//
		}


//From Delphi


//interface
//
//   function right(s: string; n: integer): string;
//   function TimeInSeconds(): real;
//   procedure DelayInSeconds(sec: real);
//   procedure DeQuote(var s: string);
//   function GetWord (a: string; i: integer): integer;
//   function GetParm(s, name: string): string;
//
//
//   public string right(string s, int n) {
//   //  ---------------------------------------------
//   //  Returns right-most n bytes from a string
//   //  --------------------------------------------
//
//   if (n > s.Length) {
//      result= s; }
//   else
//      result= copy(s, length(s)-n+1, n);
//   end;
//
//
     public static double TimeInSeconds() {
     // ----------------------------
     // Return: Number of seconds since midnight.
        //result:= 86400 * Time() <-- Delphi
        DateTime dtm = DateTime.Now;  
        double x = 3600 * dtm.Hour + 60 * dtm.Minute + dtm.Second;
        return x;
     }


//procedure DelayInSeconds(sec: real);
//{  --------------------------------}
//   var t1, t2: real;
//   begin
//      t1:= 86400 * Time();
//      repeat
//         t2:= 86400 * Time();
//      until t2 - t1 >= sec
//
//   end;
//
//procedure DeQuote(var s: string);
//{---------------------------}
//   begin
//   if s[1] = '"' then delete(s,1,1);
//   if s[length(s)] = '"' then delete(s, length(s), 1);
//
//   end;
//
//
      public static int HexVal(char c) {
      // -------------------------------
         switch (c) {
            case '0': return 0;
            case '1': return 1; 
            case '2': return 2; 
            case '3': return 3; 
            case '4': return 4; 
            case '5': return 5; 
            case '6': return 6; 
            case '7': return 7; 
            case '8': return 8; 
            case '9': return 9; 
            case 'A': return 10; 
            case 'B': return 11; 
            case 'C': return 12; 
            case 'D': return 13; 
            case 'E': return 14; 
            case 'F': return 15; 
         }
         return 0;

      }


      public static string GetParm(string s, string name) {
      // ----------------------------------------------------------
      // TASK: Return value for name in s: name1=val1; name2=name2;...
      // ----------------------------------------------------------
         int n, m;
         n = s.IndexOf(name);
         if (n<0) return("");
         n = s.IndexOf("=", n);
         if (n<0) return("");
         m = s.IndexOf(";", n);
         if (m>=0) return (s.Substring(n+1, m-n-1).Trim());
         else return (s.Substring(n+1).Trim());
      
      }


      public static int GetWord (string a, int i) {
      // -------------------------------------------------------
      // TASK: Treat 'a' as set of 4-char hex numbers. Return the
      // i-th such number. i is zero-based, so if i = 0, we want
      // characters 1 to 4. If i = 1, we want characters 5 to 8.
      // And so on.
      // -------------------------------------------------------
         string s = a.Substring(4*i,4);
         return
            4096*HexVal(s[0]) + 256*HexVal(s[1]) +
            16*HexVal(s[2]) + HexVal(s[3]);

      }


      public static int GetHex(string s, ref int p, int len) {
      // --------------------------------------------------------------------
      // It operates on s, starting in position p (0-based), for len characters.
      // It works backward, with values of the characters = 1, 16, 256, ...
      // Each character is a hex digit, 0...F.
      // --------------------------------------------------------------------
         int v = 16, result = HexVal(s[p + len - 1]);
         for (int i = p + len - 2; i >= p; i--) {
            result += v * HexVal(s[i]);
            v *= 16;
         }
         p += len;
         if (result == (int)Math.Pow(16,len)-1 && len > 1) return -1; //All F’s means ‘NULL’, use -1.
         else return result;

      }


//
//
//function PathName(fileName: string): string;
//// -----------------------------------------
//   var n: integer;
//   begin
//   n:= PosEx('\', ReverseString(fileName));
//   if n > 0 Then
//      n:= Length(fileName) - n + 1;
//   result:= LeftStr(fileName, n-1);
//
//   end;
//

      public static void DeQuote(ref string s) {
      // ----------------------------------
         int n = s.Length;
         if (s[n - 1] == '"') s = s.Remove(n - 1, 1);
         if (s[0] == '"') s = s.Remove(0, 1);
      }
      



	}
}
