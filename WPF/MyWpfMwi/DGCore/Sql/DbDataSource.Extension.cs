using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Threading.Tasks;

namespace DGCore.Sql {

  //======================  Static section  ===============================
  public partial class DbDataSource : DataSourceBase {//, IDisposable {

    interface IDbDataSourceExtension {//: IDisposable {
      ICollection GetData(bool requeryFlag);
    }

    /* Not need: use LookupTableConverter  public class DbDataSourceExtension<TItemType, TKeyType> : IDbDataSourceExtension {
          Dictionary<TKeyType, TItemType> _data;
          public ICollection GetData(bool requeryFlag) {
            return _data.Values;
          }
        }*/
    // ===============  Extension subclass  ====================
    public class DbDataSourceExtension<TItemType> : IDbDataSourceExtension {

      private const int EVENT_RECORDS_STEP = 10000;
      private DbDataSource _owner;
      private List<TItemType> _data;

      public DbDataSourceExtension(DbDataSource owner) => _owner = owner;

      public ICollection GetData(bool requeryFlag) {
        if (_data == null || requeryFlag)
          DataLoad();
        return _data;
      }

      void DataLoad() {
        // Clear data
        if (_data == null) {
          if (_owner._pdPrimaryKey == null) {
            _data = new List<TItemType>();
          }
          else {
            throw new Exception("not ready!");
            Type t = typeof(Dictionary<,>).MakeGenericType(this._owner._pdPrimaryKey.PropertyType, typeof(TItemType));
            //            this._data = Activator.CreateInstance(t);
          }
        }
        else
          _data.Clear();

        _owner._partiallyLoaded = false;
        _owner._isDataReady = false;

        // Send event
        _owner.InvokeDataEvent(this._owner, new SqlDataEventArgs(DataEventKind.Clear));
        // Load data (use background if current thread is not in background)
        if (System.Threading.Thread.CurrentThread.IsBackground) {
          // Run in current thread
          DoLoadData();
          _owner._isDataReady = true;
          _owner.InvokeDataEvent(_owner, new SqlDataEventArgs(DataEventKind.Loaded));
        }
        else {
          throw new Exception("Lovushka!");
        }
      }

      void DoLoadData() {
        // Set timeout if it needs
        if (_owner._timeoutInSecs >= 0)
          _owner._cmdData._dbCmd.CommandTimeout = this._owner._timeoutInSecs;

        // Check LookupTableTypeConverters
        var pdc = PD.MemberDescriptorUtils.GetTypeMembers(typeof(TItemType));
        var tasks = new List<Task>();
        var sqlKeys = new Dictionary<string, object>();

        foreach (PropertyDescriptor pd in pdc) {
          if (pd.Converter is Common.ILookupTableTypeConverter)
          {
            var sqlKey = ((Common.ILookupTableTypeConverter) pd.Converter).SqlKey;
            if (!sqlKeys.ContainsKey(sqlKey))
            {
                // ((Common.ILookupTableTypeConverter)pd.Converter).LoadData(this._owner); // sync
                var task = Task.Factory.StartNew(() =>
                ((Common.ILookupTableTypeConverter) pd.Converter).LoadData(this._owner));
              tasks.Add(task);
              sqlKeys.Add(sqlKey, null);
            }
          }
        }

        Task.WaitAll(tasks.ToArray());
        tasks.ForEach(t=> t.Dispose());

        Func<DbDataReader, TItemType> func = DB.DbUtils.Reader.GetDelegate_FromDataReaderToObject<TItemType>(this._owner._cmd, null);
        _owner._cmdData.Connection_Open();

        using (DbDataReader reader = this._owner._cmdData._dbCmd.ExecuteReader())
        {
          int recs = EVENT_RECORDS_STEP;
          while (reader.Read())
          {
            try
            {
              this._data.Add(func(reader));
              if ((--recs) == 0)
              {
                SqlDataEventArgs e1 = new SqlDataEventArgs(DataEventKind.Loading) { RecordCount = _data.Count };
                _owner.InvokeDataEvent(this._owner, e1);
                if (e1.CancelFlag)
                {
                  this._owner._partiallyLoaded = true;
                  this._owner._isDataReady = true;
                  this._owner._cmdData._dbCmd.Cancel();
                  break;
                }
                recs = EVENT_RECORDS_STEP;
              }
            }
            catch (Exception exception)
            {
              object[] values = new object[reader.FieldCount];
              reader.GetValues(values);
              throw;
            }
          }
        }

        _owner.InvokeDataEvent(this._owner, new SqlDataEventArgs(DataEventKind.Loading) { RecordCount = _data.Count });
      }

    }
  }

}
