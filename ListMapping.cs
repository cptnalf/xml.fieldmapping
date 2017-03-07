using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldMapping
{
  using System.Linq.Expressions;

  internal class ListMapping<O,Q> : FieldMapBase where O : class
  {
    internal void extractAccess(Expression<Func<O,List<Q>>> fx)
    {
      var b = fx as LambdaExpression;
      var g = (b.Body as MemberExpression).Member;

      switch(g.MemberType)
      {
        case (System.Reflection.MemberTypes.Field): 
          { 
            var gam = g.DeclaringType.GetField(g.Name);
            this.exp = gam;
            break;
          }
        case (System.Reflection.MemberTypes.Property):
          {
            var ggm = g.DeclaringType.GetProperty(g.Name).GetGetMethod(false);
            this.pi = ggm;
            break;
          }
      }
    }

    internal override void set(object o, string[] x, bool exists)
    {
      var res = _GetValue(typeof(Q), x);
      
      Q v = (Q)res;
      List<Q> lst = null;

      if (pi != null) { lst = (List<Q>)pi.Invoke(o,new object[] { }); }
      if (exp != null) { lst = (List<Q>)exp.GetValue(o); }

      lst.Add(v);
    }
  }
}
