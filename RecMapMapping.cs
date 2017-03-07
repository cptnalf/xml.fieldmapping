using System;
using System.Collections.Generic;

namespace FieldMapping
{
  internal class RecMapMapping<R,Q> : FieldMapBase where R : class where Q : class, new()
  {
    internal FieldMapper<Q> mapper {get;set; }
    internal Func<R,Q,Q> filter {get;set; }

    internal override void set(object o, string[] x, bool exists)
    {
      Dictionary<string,string> rec = new Dictionary<string, string>();
      for(int i=0; i < fieldName.Count; ++i) { rec.Add(fieldName[i], x[i]); }

      Q q = mapper.convert(rec);
      if (filter != null) { q = filter((R)o, q); }
      _objchange(o, q);
    }
  }
}
