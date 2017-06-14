using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldMapping
{
  using System.Linq.Expressions;

  /// <summary>
  /// processes the saving of a target list field on a target object type.
  /// </summary>
  /// <typeparam name="O"></typeparam>
  /// <typeparam name="Q"></typeparam>
  internal class ListMapping<O,Q> : FieldMapBase where O : class
  {
    private System.Reflection.MethodInfo _mi = null;

    internal bool ignoreNulls {get;set;}
    internal Func<O,Q,Q> filter {get;set;}
    
    internal override void set(object o, string[] x, bool exists)
    {
      var res = _GetValue(typeof(Q), x);
      
      Q v = (Q)res;
      List<Q> lst = null;
      
      if (pi != null) 
        { 
          if (_mi == null)
            { _mi = pi.GetGetMethod(false); }
          if (_mi != null)
            { lst = (List<Q>)_mi.Invoke(o,new object[] { }); }
        }
      if (exp != null) { lst = (List<Q>)exp.GetValue(o); }

      if (filter != null)
        {
          O ob1 = o as O;
          v = filter(ob1,v);
        }

      if (!ignoreNulls || v != null) { lst.Add(v); }
    }
  }
}
