using System;
using System.Collections.Generic;

namespace FieldMapping
{
  internal abstract class FieldMapBase
  {
    internal List<string> fieldName {get;set; }
    internal System.Reflection.FieldInfo exp {get;set; }
    internal System.Reflection.MethodInfo pi {get;set; }

    internal abstract void set(object o, string[] x, bool exists);

    internal FieldMapBase() { this.fieldName = new List<string>(); }

    protected void _objchange(object cls, object val)
    {
      if (exp != null)
        { exp.SetValue(cls, val); }
      if (pi != null)
        { pi.Invoke(cls, new object[] { val}); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <param name="origval">ALWAYS a non-null array. elements can be null.</param>
    /// <returns></returns>
    internal static object _GetValue(Type t, string[] origval)
    {
      object result = null;
      if (t == typeof(string)) { result = origval[0]; }
        
      bool nullable = false;
      bool resok = false;

      Type coreType = null;
      coreType = t;

      if (coreType.IsGenericType)
        {
          if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
            { coreType = Nullable.GetUnderlyingType(t); nullable = true; }
        }

      if (nullable && origval[0] == null) { result = null; }
      else
        {
          if (coreType == typeof(decimal))
            {
              decimal val = decimal.Zero;
              resok = decimal.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(int) == coreType)
            {
              int val = 0;
              resok = int.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(long) == coreType)
            {
              long val = 0;
              resok = long.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(uint) == coreType)
            {
              uint val = 0;
              resok = uint.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(bool) == coreType)
            {
              bool val = false;
              resok = true;
              if (!bool.TryParse(origval[0], out val))
                {
                  resok = false;
                  if (string.Compare(origval[0], "yes", true) == 0) { val = true; }
                  if (string.Compare(origval[0], "no", true) == 0) { val = false; }
                  if (!resok)
                    {
                      int x = -1;
                      if (int.TryParse(origval[0], out x)) { val = x != 0; }
                    }
                }
            }

          if (typeof(DateTime) == coreType)
            {
              string dp1 = origval[0];
              string tp1 = origval.Length > 1 ? origval[1] : null;

              DateTime dtres = DateTime.MinValue;
              resok = false;

              if (string.IsNullOrWhiteSpace(dp1))
                {
                  if (string.IsNullOrWhiteSpace(tp1)) { }
                  else { dp1 = tp1; }
                }
              else
                {
                  if (string.IsNullOrWhiteSpace(tp1)) { }
                  else
                    { 
                      if (tp1.IndexOf(':') < 0 && tp1.Trim().Length == 6)
                        {
                          string tmp = tp1.Trim();
                          tp1 = tmp.Substring(0,2) + ":" + tmp.Substring(2,2) + ":" + tmp.Substring(4,2);
                        }
                      if (tp1.IndexOf(':') >= 0 && tp1.Trim().Length > 8)
                        {
                          tp1 = tp1.Trim().Substring(0,8);
                        }
                      dp1 += " " + tp1;
                    }
                }
              if (!string.IsNullOrWhiteSpace(dp1)) 
                {
                  DateTime.TryParse(dp1, out dtres);
                  resok = (dtres != DateTime.MinValue);
                }

              if (resok) { result = dtres; }
            }

          if (!resok && nullable) { result = null; }
        }

      return result;
    }
  }
}
