using System;
using System.IO;

namespace BCX.BCXB
{
   public interface IFileAccess
   {
      void SetFolders();
      StreamReader GetTextFileOnDisk(string fName);
      StreamReader GetModelFile(short engNum);
      StreamReader GetModelFile(string fName);
      
   }

}

