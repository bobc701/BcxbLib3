using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Bcx.Util {


/// <summary>
/// Xarray is a class that is very similar to a Dictionary and also an array,<br />
/// it combines properties of each. It is fixed len like an array, so you <br />
/// don't need to do Add's, but it is index-able by an Enum, which sadly in C#,<br />
/// you can't do with an array.
/// </summary>
/// <typeparam name="TIx">the type of the index, normally an enum</typeparam>
/// <typeparam name="TData">the type of the data stored in the array</typeparam>

public class XArray<TIx, TData> where TIx : IConvertible where TData : new() {

   TData[] data;

   public XArray(int size) {
      data = new TData[size];
      for (int i=0; i<size; i++) data[i] = new TData();
   }

   public TData this[TIx ix] {
      get { return data[ix.ToInt32(new CultureInfo("en-US"))]; }
      set { data[ix.ToInt32(new CultureInfo("en-US"))] = value; }
   }

   public int Length {
      get { return data.Length; }
   }
   

}


}
