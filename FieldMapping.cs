using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldMapping
{
  internal class FieldMapping<O,Q> 
    : FieldMapBase 
    where O : class
  {
    internal Func<O,Q,Q> filter {get;set; }
    internal bool defset {get;set; }
    internal Q defval {get;set; }
    internal Func<string[],Q> converter {get;set; }

    internal FieldMapping() : base() { defset = false; }

    internal override void set(object obj, string[] x, bool exists)
    {
      O t = (O)obj;
      var newx = default(Q);
      object dest = null;
        
      if (!exists && defset) { dest = newx = defval; }
      else 
        {
          if (converter != null) { dest = converter(x); }
          else { dest = _GetValue(typeof(Q), x); }
        }

      if (filter != null) 
        {
          /* only do cast if we can actually get a result. */
          if (dest == null && !typeof(Q).IsValueType || dest != null) 
            { newx = (Q)dest; }
          dest = filter(t,newx); 
        }

      _objchange(obj, dest);
    }
  }
}
