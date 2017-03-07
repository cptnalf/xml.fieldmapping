using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldMapping
{
  using System.Linq.Expressions;

  /// <summary>
  /// maps query fields to object fields.
  /// </summary>
  /// <typeparam name="T">Record Object</typeparam>
  public class FieldMapper<T> where T : class, new()
  {
    /// <summary>
    /// Create a Field Mapper for a particular object.
    /// </summary>
    /// <returns></returns>
    public static FieldMapper<T> Create() { return new FieldMapper<T>(); }

    private List<FieldMapBase> _mappings = new List<FieldMapBase>();

    public FieldMapper<T> field<K>(Expression<Func<T,List<K>>> x, string fieldname)
    {
      var m = new ListMapping<T,K>();
      m.extractAccess(x);
      m.fieldName.Add(fieldname);
      _mappings.Add(m);
      return this;
    }

    /// <summary>
    /// add a new mapping.
    /// </summary>
    /// <typeparam name="K">Destination Type</typeparam>
    /// <param name="x">the destination</param>
    /// <param name="filter">after the value is converted, call this to munge it more</param>
    /// <param name="defaultval">default value if none of the fields are available</param>
    /// <param name="fieldnames">name of fields to map to this parameter</param>
    /// <returns></returns>
    public FieldMapper<T> field<K>(Expression<Func<T,K>> x, K defaultval, Func<T,K,K> filter, params string[] fieldnames)
    {
      if (fieldnames == null) { throw new ArgumentNullException("Must have atleast 1 field mapped."); }
      var m = new FieldMapping<T,K>();
      m.defset = true;
      m.defval = defaultval;
      m.filter = filter;
      m.fieldName.AddRange(fieldnames);

      _extractAccess(m, x);

      _mappings.Add(m);
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K">Destination Type</typeparam>
    /// <param name="x">the destination</param>
    /// <param name="filter">after the value is converted, call this to munge it more</param>
    /// <param name="fieldnames">name of fields to map to this parameter</param>
    /// <returns></returns>
    public FieldMapper<T> field<K>(Expression<Func<T,K>> x, Func<T,K,K> filter, params string[] fieldnames)
    {
      if (fieldnames == null) { throw new ArgumentNullException("Must have atleast 1 field mapped."); }

      var m = new FieldMapping<T,K>();
      m.fieldName.AddRange(fieldnames);
      m.filter = filter;
      _extractAccess(m, x);
            
      _mappings.Add(m);
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K">Destination Type</typeparam>
    /// <param name="x">the destination</param>
    /// <param name="fieldnames">name of fields to map to this parameter</param>
    /// <param name="filter">after the value is converted, call this to munge it more</param>
    /// <returns></returns>
    public FieldMapper<T> field<K>(Expression<Func<T,K>> x, string fieldName, Func<T,K,K> filter) 
    { return field(x, filter, fieldName); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K">Destination Type</typeparam>
    /// <param name="x">the destination</param>
    /// <param name="fieldnames">name of fields to map to this parameter</param>
    /// <returns></returns>
    public FieldMapper<T> field<K>(Expression<Func<T,K>> x, string fieldName) { return field(x, null, fieldName);  }

    public FieldMapper<T> convert<K>(Expression<Func<T,K>> x, Func<string[],K> convtr, params string[] fields)
    {
      var m = new FieldMapping<T,K>();
      m.converter = convtr;
      m.fieldName.AddRange(fields);
      _extractAccess(m,x);
      _mappings.Add(m);
      return this;
    }

    public FieldMapper<T> map<K>(Expression<Func<T,K>> x, FieldMapper<K> mapper, params string[] fieldnames) 
      where K : class,new()
    { return map(x, mapper, null, fieldnames); }

    public FieldMapper<T> map<K>(Expression<Func<T,K>> x, FieldMapper<K> mapper, Func<T,K,K> filter, params string[] fieldnames) 
      where K : class,new()
    {
      var m = new RecMapMapping<T,K>();
      m.fieldName.AddRange(fieldnames);
      m.filter = filter;
      m.mapper = mapper;
      _extractAccess(m, x);
      return this;
    }

    public T convert(IReadOnlyDictionary<string,string> record)
    {
      T obj = new T();

      foreach(var m in _mappings)
        {
          string[] vals = new string[m.fieldName.Count];
          bool exists = false;
          for(int x=0; x < m.fieldName.Count; ++x) 
            { if (record.TryGetValue(m.fieldName[x], out vals[x])) { exists = true; } }
           m.set(obj, vals, exists);
        }

      return obj;
    }

    public T create() { return new T(); }
    public void set(T obj, string field, string value)
    {
      var m = _mappings.Where(x => x.fieldName.Contains(field)).FirstOrDefault();
      m.set(obj, new string[] { value }, true);
    }
    
    private void _extractAccess<K>(FieldMapBase fmb, Expression<Func<T,K>> fx)
    {
      var b = fx as LambdaExpression;
      var g = (b.Body as MemberExpression).Member;

      switch(g.MemberType)
      {
        case (System.Reflection.MemberTypes.Field): 
          { 
            var gam = g.DeclaringType.GetField(g.Name);
            fmb.exp = gam;
            break;
          }
        case (System.Reflection.MemberTypes.Property):
          {
            var ggm = g.DeclaringType.GetProperty(g.Name).GetSetMethod(false);
            fmb.pi = ggm;
            break;
          }
      }
    }
  }
}
