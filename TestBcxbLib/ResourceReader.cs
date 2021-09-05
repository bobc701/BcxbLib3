using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestBcxbLib
{
   class ResourceReader
   {

   /* ------------------------------------------------------
    * The purpose oft his class is to deal with the vagaries of
    * embedded resource files.
    * -----------------------------------------------------
    */


      public static string ReadEmbeddedRecouce(string path) {

      // path is like: MyProject.Resources.Model.MyFile.txt
      // This assumes you have a Resources folder under the project, and a Model
      // folder under that.
      // Notes:
      // -- Not clear if MyProject is the proj name, the project's folder, or the namespace of Program.
      // -- Not clear if this only works for Recources folder.
      // -- The file must be marked 'Embedded Recource' in properties.

         Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
         return new StreamReader(strm).ReadToEnd();
         
      }


      public static StreamReader GetEmbeddedRdr(string path) {

         Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
         return new StreamReader(strm);

      }


      public static void MiscStuff() {

      // You can call these to see a list embedded resoutces, with their paths...
         string[] files = Assembly.GetExecutingAssembly().GetManifestResourceNames();

      // This seems to return the name of the solution...
         string s = Assembly.GetExecutingAssembly().GetName().Name;

      }


   }

}

