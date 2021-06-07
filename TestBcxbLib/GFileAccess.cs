/* --------------------------------------------------------
 * This approach uses Embedded Resource files and GetManifestResourceStream()
 * as in BcxbXF.
 * As opp to BcxbWin which currently just does file I/O vs known folder
 * locations on disk, but could be changed. 
 * --------------------------------------------------------
 */


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net;
using System.Linq;


namespace BCX.BCXB {

   public class GFileAccess {

      public void SetFolders() {

         // For this app, no action needed, folders are not used.

      }


      public StreamReader GetTextFileOnDisk(string fName) {
         // --------------------------------------------------------------------------------
         // This returns StreamReader for file stored under Resources.
         // FileName should include folders separated by '.'.
         // EG: Model.cfeng1 <-- Note: Case sensitive!!!
         // --------------------------------------------------------------------------------
         Assembly assembly = typeof(TestBcxbLib.Program).GetTypeInfo().Assembly;

         string path = @"TestBcxbLib.Resources." + fName;

         // For testing, look at these...
         /* Strings returned have these 5 parts, all separated by dots...
          * - Default namespace ('BcxbXf')
          * - 'Resources'
          * - Folder structure ('Model' or 'Teams' etc)
          * - File name
          * - Extension ('bcx')
          */

         //var files = Assembly.GetExecutingAssembly().GetManifestResourceNames();
         //files = assembly.GetManifestResourceNames();

         //Note: 
         //Both of the following seem to work here. (I thought in BcxbXf only 'assembly' worked??)
         //---------------------------------------------------------------------
         //Stream strm = assembly.GetManifestResourceStream(path);
         Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

         return new StreamReader(strm);

      }


      public StreamReader GetModelFile(short engNum) {
         // ------------------------------------------------------------------
         // This returns a file object for CFEng1,2 oe 3.
         // ------------------------------------------------------------------
         string path1 = "Model.cfeng" + engNum.ToString() + ".bcx";
         try {
            StreamReader f = GetTextFileOnDisk(path1);
            return f;
         }
         catch (Exception ex) {
            string msg = "Could not open " + path1 + "\r\nError: " + ex.Message;
            throw new Exception(msg);
         }

      }


      public StreamReader GetModelFile(string fName) {
         // ------------------------------------------------------------------
         // This returns a file object for CFEng1,2 oe 3.
         // ------------------------------------------------------------------
         string path1 = "Model." + fName + ".bcx";
         try {
            StreamReader f = GetTextFileOnDisk(path1);
            return f;
         }
         catch (Exception ex) {
            string msg = "Could not open " + path1 + "\r\nError: " + ex.Message;
            throw new Exception(msg);
         }

      }
   }

}
