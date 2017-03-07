using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldMapping
{
  internal class FieldTransform<T> : FieldMapBase where T : class, new()
  {
    internal Func<string,string> func {get;set; }

    internal override void set(object o, string[] x, bool exists)
    {
      
    }
  }
}
