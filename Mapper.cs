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
    public static FieldMapper<T> Create(bool overwrite=true)
    {
      var m = new FieldMapper<T>();
      m.overwrite = overwrite;
      return m;
    }

    private List<FieldMapBase> _mappings = new List<FieldMapBase>();
    private bool _overwrite = false;
    public bool overwrite { get { return _overwrite; } set { _overwrite = value; } }

    /// <summary>
    /// map a field in the source to a list in the target object type.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    public FieldMapper<T> list<K>(Expression<Func<T,List<K>>> x, string fieldname)
    { return list(x, false, null, fieldname); }

    /// <summary>
    /// map a field in the source to a list in the target object type.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="ignoreNulls">ignore null source values.</param>
    /// <param name="fieldname">source field name</param>
    /// <returns></returns>
    public FieldMapper<T> list<K>(Expression<Func<T,List<K>>> x, bool ignoreNulls, string fieldname)
    { return list(x, ignoreNulls, null, fieldname); }

    /// <summary>
    /// map a field in the source to a list in the target object type.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="ignoreNulls">ignore null source values</param>
    /// <param name="filter">pass the source value through a filter function</param>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    public FieldMapper<T> list<K>(Expression<Func<T,List<K>>> x, bool ignoreNulls, Func<T,K,K> filter, string fieldname)
    {
      var m = new ListMapping<T,K>();
      _extractAccess(m,x);
      m.fieldName.Add(fieldname);
      _mappings.Add(m);
      m.overwrite = this.overwrite;
      m.ignoreNulls = ignoreNulls;
      m.filter = filter;

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
      m.overwrite = this.overwrite;
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
      m.overwrite = this.overwrite;
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

    /// <summary>
    /// convert a source value (or values) from strings to a target type in the given object type.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="convtr">conversion function to be called.</param>
    /// <param name="filter">filter the converted value gets passed to before it's commited to the object</param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public FieldMapper<T> convert<K>(Expression<Func<T,K>> x, Func<string[],K> convtr, Func<T,K,K> filter, params string[] fields)
    {
      var m = new FieldMapping<T,K>();
      m.converter = convtr;
      m.filter = filter;
      m.fieldName.AddRange(fields);
      _extractAccess(m,x);
      _mappings.Add(m);
      m.overwrite = this.overwrite;
      return this;
    }

    /// <summary>
    /// convert a source value (or values) from strings to a target type in the given object type.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="convtr">conversion function to be called.</param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public FieldMapper<T> convert<K>(Expression<Func<T,K>> x, Func<string[],K> convtr, params string[] fields)
    { return convert(x, convtr, null, fields); }

    /// <summary>
    /// map a set of source values from strings to another FieldMapper
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="mapper">mapper that will convert the field-values into an object</param>
    /// <param name="fieldnames"></param>
    /// <returns></returns>
    public FieldMapper<T> map<K>(Expression<Func<T,K>> x, FieldMapper<K> mapper, params string[] fieldnames) 
      where K : class,new()
    { return map(x, mapper, null, fieldnames); }

    /// <summary>
    /// map a set of source values from strings to another FieldMapper.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="x"></param>
    /// <param name="mapper">mapper object that will process the source fields into an object</param>
    /// <param name="filter">function which the converted object gets passed to before it's set on the target type.</param>
    /// <param name="fieldnames"></param>
    /// <returns></returns>
    public FieldMapper<T> map<K>(Expression<Func<T,K>> x, FieldMapper<K> mapper, Func<T,K,K> filter, params string[] fieldnames) 
      where K : class,new()
    {
      var m = new RecMapMapping<T,K>();
      m.fieldName.AddRange(fieldnames);
      m.filter = filter;
      m.mapper = mapper;
      _extractAccess(m, x);
      m.overwrite = this.overwrite;
      _mappings.Add(m);
      return this;
    }

    /// <summary>
    /// convert a dictionary of fields into an object using the source to target field mappings.
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    /// <remarks>
    /// this does the conversion all at one time and will generate a new object every time.
    /// </remarks>
    public T convert(IReadOnlyDictionary<string,string> record)
    {
      T obj = new T();
      return convert(obj, record);
    }

    /// <summary>
    /// takes an existing object and processes the given source fields mapping them to the given target object
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="record"></param>
    /// <returns></returns>
    public T convert(T obj, IReadOnlyDictionary<string,string> record)
    {
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

    /// <summary>
    /// create a new instance of the target type.
    /// </summary>
    /// <returns></returns>
    public T create() { return new T(); }

    /// <summary>
    /// specifies one value for the source field, and processes the source-field mappings for the target object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void set(T obj, string field, string value)
    {
      var m = _mappings.Where(x => x.fieldName.Contains(field)).FirstOrDefault();
      if (m != null) { m.set(obj, new string[] { value }, true); }
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
            var ggm = g.DeclaringType.GetProperty(g.Name);
            fmb.pi = ggm;
            break;
          }
      }
    }
  }
}
