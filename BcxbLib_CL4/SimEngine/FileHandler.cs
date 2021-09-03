using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimEngine
{
   public static class FileHandler
   {

      public static string GetTextFileOnDisk(string jsonFilename)
      {
         // --------------------------------------------------------------------------------
         // This returns StreamReader for file stored under Resources.
         // FileName should include folders separated by '.'.
         // EG: Model.cfeng1 <-- Note: Case sensitive!
         // --------------------------------------------------------------------------------

         //string path = $"ListDef.Resources.{jsonFilename}";
         string path = jsonFilename;

         // For testing, look at these...
         /* Strings returned have these 5 parts, all separated by dots...
          * - Default namespace ('BcxbXf')
          * - 'Resources'
          * - Folder structure ('Model' or 'Teams' etc)
          * - File name
          * - Extension ('bcxt')
          */

         string[] files = Assembly.GetExecutingAssembly().GetManifestResourceNames();
         string s = Assembly.GetExecutingAssembly().GetName().Name;

         //Assembly assembly = typeof(TestBcxbLib.Program).GetTypeInfo().Assembly;
         //Stream s1 = assembly.GetManifestResourceStream("path");

         //Note: 
         //Both of the following seem to work here. (I thought in BcxbXf only 'assembly' worked??)
         //---------------------------------------------------------------------
         //Stream strm = assembly.GetManifestResourceStream(path);
         Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);


         StreamReader rdr = new(strm);
         string json = rdr.ReadToEnd();
         return json;
      }
   }
}
