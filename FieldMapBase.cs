using System;
using System.Collections.Generic;

namespace FieldMapping
{
  internal abstract class FieldMapBase
  {
    private System.Reflection.MethodInfo _gi = null;
    private System.Reflection.MethodInfo _mi = null;
    
    internal List<string> fieldName {get;set; }
    internal System.Reflection.FieldInfo exp {get;set; }
    internal System.Reflection.PropertyInfo pi {get;set; }
    internal bool overwrite {get;set; }

    internal abstract void set(object o, string[] x, bool exists);

    internal FieldMapBase() { this.fieldName = new List<string>(); }

    protected void _objchange(object cls, object val)
    {
      object o = val;

      if (!overwrite)
        {
          Type t = null;
          if (exp != null) { t = exp.FieldType; o = exp.GetValue(cls); }
          if (pi != null)
            {
              t = pi.PropertyType;
              if (_gi == null) { _gi = pi.GetGetMethod(false); }
              if (_gi != null) { o = _gi.Invoke(cls, new object[] {}); }
            }

          o = _CombineValue(t, o, val);
        }

      if (exp != null) { exp.SetValue(cls, o); }
      if (pi != null)
        {
          if (_mi == null) { _mi = pi.GetSetMethod(false); }
          if (_mi != null) { _mi.Invoke(cls, new object[] { o }); }
        }
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
          if (typeof(uint) == coreType)
            {
              uint val = 0;
              resok = uint.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(long) == coreType)
            {
              long val = 0;
              resok = long.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(ulong) == coreType)
            {
              ulong val = 0;
              resok = ulong.TryParse(origval[0], out val);
              result = val;
            }
          if (typeof(bool) == coreType)
            {
              bool val = false;
              resok = true;
              if (!bool.TryParse(origval[0], out val))
                {
                  resok = true;
                  if (string.Compare(bool.FalseString, origval[0], true) == 0) { val = false; }
                  else
                    {
                      if (string.Compare(bool.TrueString, origval[0], true) == 0) { val = true; }
                      else
                        {
                          if (string.Compare(origval[0], "yes", true) == 0) { val = true; }
                          else
                            {
                              if (string.Compare(origval[0], "no", true) == 0) { val = false; }
                              else
                                { resok = false; }
                            }
                        }
                    }
                  
                  if (!resok)
                    {
                      int x = -1;
                      if (int.TryParse(origval[0], out x)) { val = x != 0; }
                    }
                }
              result = val;
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

              result = dtres;
            }

          if (!resok && nullable) { result = null; }
        }

      return result;
    }

    internal static object _CombineValue(Type t, object o1, object newo)
    {
      object result = null;
      bool nullable = false;
      bool resok = false;

      Type coreType = null;
      coreType = t;

      if (coreType.IsGenericType)
        {
          if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
            { coreType = Nullable.GetUnderlyingType(t); nullable = true; }
        }
      if (coreType.IsClass) { nullable = true; }

      if (nullable && o1 == null && newo == null) { result = null; }
      else 
        {
          if (nullable && (o1 == null || newo == null))
            {
              if (newo == null) { result = o1; }
              if (o1 == null) { result = newo; }
            }
          else
            {
              result = newo;

              if (coreType == typeof(string))
                {
                  string val = (string)o1;
                  if (newo != null) { result = o1 == null ? newo : val + (string)newo; }
                  else { result = val; }
                }
              if (coreType == typeof(decimal))
                {
                  decimal d1 = (decimal)o1;
                  decimal d2 = (decimal)newo;
                  result = d1 + d2;
                }
              if (coreType == typeof(int))
                {
                  int i1 = (int)o1;
                  int i2 = (int)newo;
                  result = i1 + i2;
                }
              if (coreType == typeof(long))
                {
                  long l1 = (long)o1;
                  long l2 = (long)newo;
                  result = l1 + l2;
                }
              if (coreType == typeof(uint))
                {
                  uint u1 = (uint)o1;
                  uint u2 = (uint)newo;
                  result = u1 + u2;
                }
              if (coreType == typeof(bool))
                {
                  bool b1 = (bool)o1;
                  bool b2 = (bool)newo;
                  result = b1 & b2;
                }

              if (coreType == typeof(DateTime))
                {
                  DateTime do1 = (DateTime)o1;
                  DateTime no = (DateTime)newo;
                  if (do1 == DateTime.MinValue && no != DateTime.MinValue) { result = newo; }
                  if (no == DateTime.MinValue) { result = o1; }
                }
            }
        }

      return result;
    }
  }
}
