using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DGCore.UserSettings
{
  public static class UserSettingsUtils
  { // SettingDuple (for two level keys)
    // Setting stores object by settingKind + settingKey + settingID
    // Each SettingKind has own file (for file mode)

    // ===========================   Public Section =============================
    public static void Init<T>(IUserSettingSupport<T> o, string startupLayoutName)
    { // return value: was Setting apply
      var keys = GetKeysFromDb(o);
      var username = Utils.Tips.GetFullUserName();
      var temp = keys.Where(s => string.Equals(s, startupLayoutName, StringComparison.InvariantCultureIgnoreCase)).ToArray();
      if (temp.Length == 0)
      {
        temp = keys.Where(s => string.Equals(s, username, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        if (temp.Length == 0)
          temp = keys.Where(s => string.Equals(s, "DEFAULT", StringComparison.InvariantCultureIgnoreCase)).ToArray();
      }

      if (temp.Length == 0)
        o.SetSetting(o.GetBlankSetting());
      else
        SetSetting(o, temp[0]);
    }
    public static int SaveChangedSettings(List<UserSettingsDbObject> data, IUserSettingProperties properties)
    {
      var changedItems = new List<UserSettingsDbObject>();
      var deletedItems = new List<UserSettingsDbObject>();
      foreach (var o1 in data)
      {
        if (o1.IsChanged || o1.IsDeleted)
        {
          if (o1.IsDeleted && o1.IsEditable) deletedItems.Add(o1);
          else if (o1.IsChanged && o1.IsEditable) changedItems.Add(o1);
          else return -1;
        }
      }
      switch (_storageKind)
      {
        case StorageKind.File: throw new Exception("Not ready");
        case StorageKind.SqlClient:
          using (var conn = new SqlConnection(PathOrConnection))
          {
            conn.Open();
            foreach (var o1 in deletedItems)
            {
              using (var cmd = new SqlCommand("DELETE _UserSettings WHERE [kind]=@kind and [key]=@key and [id]=@id", conn))
              {
                cmd.Parameters.AddRange(new[]
                {
                  new SqlParameter("@kind", properties.SettingKind),
                  new SqlParameter("@key", properties.SettingKey),
                  new SqlParameter("@id", o1.SettingId)
                });
                cmd.ExecuteNonQuery();
                data.Remove(o1);
              }
            }
            foreach (var o1 in changedItems)
            {
              using (var cmd = new SqlCommand("UPDATE _UserSettings SET alloweditothers=@alloweditothers, allowviewothers=@allowviewothers, updated=@updated, dupdated=@dupdated WHERE [kind]=@kind and [key]=@key and [id]=@id", conn))
              {
                cmd.Parameters.AddRange(new[]
                {
                  new SqlParameter("@alloweditothers", o1.AllowEditOthers),
                  new SqlParameter("@allowviewothers", o1.AllowViewOthers),
                  new SqlParameter("@updated", Utils.Tips.GetFullUserName()),
                  new SqlParameter("@dupdated", DateTime.Now), new SqlParameter("@kind", properties.SettingKind),
                  new SqlParameter("@key", properties.SettingKey), new SqlParameter("@id", o1.SettingId)
                });
                cmd.ExecuteNonQuery();
                o1.OriginalAllowEditOthers = o1.AllowEditOthers;
                o1.OriginalAllowViewOthers = o1.AllowViewOthers;
              }
            }
          }
          break;
      }
      return deletedItems.Count + changedItems.Count;
    }

    public static bool SaveNewSetting(IUserSettingProperties o, string settingId, bool allowViewOthers, bool allowEditOthers)
    {
      if (o == null)
      {
        MessageBox.Show(@"Обєкт налаштування не може бути пустим", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }
      var methodType = o.GetType().GetInterface("IUserSettingSupport`1").GetGenericArguments()[0];
      var saveMethodInfo = typeof(UserSettingsUtils).GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(x => x.Name == "SaveNewSetting" && x.IsGenericMethod).MakeGenericMethod(methodType);
      return (bool)saveMethodInfo.Invoke(null, new object[] { o, settingId, allowViewOthers, allowEditOthers });
    }
    public static bool SaveNewSetting<T>(IUserSettingSupport<T> o, string settingId, bool allowViewOthers, bool allowEditOthers)
    {
      if (!CheckSettingId(o, settingId))
        return false;

      var keys = GetKeysFromDb(o);
      var s = settingId.ToUpper();
      var flagExist = keys.Any(s1 => s1.ToUpper() == s);
      if (flagExist)
      {
        if (MessageBox.Show($@"Налаштування з кодом '{settingId}' уже існує. Перезаписати її?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
          return false;
      }
      switch (_storageKind)
      {
        case StorageKind.File:
          throw new Exception("Not ready");
          //csFastSerializer.Utils.File_SaveObject(_settings[o.SettingKind], GetFileName(o.SettingKind));
        case StorageKind.SqlClient:
          using (var conn = new SqlConnection(PathOrConnection))
          {
            conn.Open();
            bool? dbAllowEdit = null;
            using (var cmd = new SqlCommand("SELECT AllowEditOthers, created from _UserSettings where kind=@kind and [key]=@key and id=@id", conn))
            {
              cmd.Parameters.AddRange(new[]
              {
                new SqlParameter("@kind", o.SettingKind), new SqlParameter("@key", o.SettingKey),
                new SqlParameter("@id", settingId), new SqlParameter("@owner", Utils.Tips.GetFullUserName())
              });

              using (var dr = cmd.ExecuteReader())
              {
                while (dr.Read())
                {
                  var x1 = Convert.ToInt32(dr["AllowEditOthers"]);
                  var x2 = dr["created"].ToString().ToUpper();
                  dbAllowEdit = (x1 != 0 || x2 == Utils.Tips.GetFullUserName());
                  break;
                }
              }
            }
            if (!dbAllowEdit.HasValue)
            {
              using (var cmd = new SqlCommand("INSERT into _UserSettings ([kind], [key], [id], data, alloweditothers, allowviewothers, created, dcreated) VALUES (@kind, @key, @id, @data, @alloweditothers, @allowviewothers, @created, @dcreated)", conn))
              {
                cmd.Parameters.AddRange(new[]
                {
                  new SqlParameter("@kind", o.SettingKind), new SqlParameter("@key", o.SettingKey),
                  new SqlParameter("@id", settingId),
                  new SqlParameter("@data", JsonConvert.SerializeObject(o.GetSettings())),
                  new SqlParameter("@alloweditothers", allowEditOthers),
                  new SqlParameter("@allowviewothers", allowViewOthers),
                  new SqlParameter("@created", Utils.Tips.GetFullUserName()), new SqlParameter("@dcreated", DateTime.Now)
                });
                cmd.ExecuteNonQuery();
              }
            }
            else if (dbAllowEdit == true)
            {
              using (var cmd = new SqlCommand("UPDATE _UserSettings SET data=@data, alloweditothers=@alloweditothers, allowviewothers=@allowviewothers, updated=@updated, dupdated=@dupdated WHERE [kind]=@kind and [key]=@key and [id]=@id", conn))
              {
                cmd.Parameters.AddRange(new[]
                {
                  new SqlParameter("@data", JsonConvert.SerializeObject(o.GetSettings())),
                  new SqlParameter("@alloweditothers", allowEditOthers),
                  new SqlParameter("@allowviewothers", allowViewOthers),
                  new SqlParameter("@updated", Utils.Tips.GetFullUserName()), new SqlParameter("@dupdated", DateTime.Now),
                  new SqlParameter("@kind", o.SettingKind), new SqlParameter("@key", o.SettingKey),
                  new SqlParameter("@id", settingId)
                });
                cmd.ExecuteNonQuery();
              }
            }
            else
            {
              MessageBox.Show($@"Ви не маєте права перезаписати налаштування з кодом '{settingId}'.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              return false;
            }
          }
          break;
      }
      return true;
    }
    public static void SetSetting<T>(IUserSettingSupport<T> o, string settingId)
    {
      if (!CheckSettingId(o, settingId))
        return;

      switch (_storageKind)
      {
        case StorageKind.File:
          throw new Exception("Not ready");
        case StorageKind.SqlClient:
          using (var conn = new SqlConnection(PathOrConnection))
          using (var cmd = new SqlCommand("SELECT data from _UserSettings where kind=@kind and [key]=@key and id=@id and (AllowViewOthers=1 or created=@owner)", conn))
          {
            conn.Open();
            cmd.Parameters.AddRange(new[]
            {
              new SqlParameter("@kind", o.SettingKind), new SqlParameter("@key", o.SettingKey),
              new SqlParameter("@id", settingId), new SqlParameter("@owner", Utils.Tips.GetFullUserName())
            });
            using (var dr = cmd.ExecuteReader())
            {
              while (dr.Read())
              {
                var o1 = JsonConvert.DeserializeObject<T>(dr.GetString(0));
                o.SetSetting(o1);
                return;
              }
            }
          }
          break;
      }
    }
    public static void DeleteSetting<T>(IUserSettingSupport<T> o, string settingId)
    {
      if (!CheckSettingId(o, settingId))
        return;

      if (MessageBox.Show($@"Вилучити налаштування з кодом '{settingId}'?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        return;

      switch (_storageKind)
      {
        case StorageKind.File:
          //csFastSerializer.Utils.File_SaveObject(_settings[o.SettingKind], GetFileName(o.SettingKind));
          throw new Exception("Not ready");
        case StorageKind.SqlClient:
          using (var conn = new SqlConnection(PathOrConnection))
          using (var cmd = new SqlCommand("DELETE _UserSettings where [kind]=@kind and [key]=@key and [id]=@id", conn))
          {
            conn.Open();
            cmd.Parameters.AddRange(new[]
            {
              new SqlParameter("@kind", o.SettingKind), new SqlParameter("@key", o.SettingKey),
              new SqlParameter("@id", settingId)
            });
            cmd.ExecuteNonQuery();
          }
          break;
      }
    }

    // ===========================   Private Section =============================
    private enum StorageKind { File, SqlClient };
    private static readonly string PathOrConnection; // In DesignMode equals to null
    private static readonly StorageKind _storageKind =
    (Misc.AppSettings.settingsStorage.Trim().ToUpper().StartsWith("SQLCLIENT")
      ? StorageKind.SqlClient
      : StorageKind.File);

    // Init
    static UserSettingsUtils()
    {
      // Create data folder
      if (!Utils.Tips.IsDesignMode)
        if (_storageKind == StorageKind.SqlClient)
          PathOrConnection = Misc.AppSettings.settingsStorage
            .Substring(Misc.AppSettings.settingsStorage.IndexOf(";", StringComparison.Ordinal) + 1).Trim();
        else
        {
          PathOrConnection = Misc.AppSettings.settingsStorage;
          if (!Directory.Exists(PathOrConnection))
            Directory.CreateDirectory(PathOrConnection);
        }
    }

    static bool CheckSettingId<T>(IUserSettingSupport<T> o, string settingId)
    {
      if (string.IsNullOrEmpty(o.SettingKind) || string.IsNullOrEmpty(o.SettingKey))
      {
        MessageBox.Show(@"Налаштування недоступні", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      if (string.IsNullOrEmpty(settingId))
      {
        MessageBox.Show(@"Назва налаштування не може бути пустою", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      if (o is Control)
        Utils.Dgv.EndEdit((Control)o);

      return true;
    }

    public static List<UserSettingsDbObject> GetUserSettingDbObjects(IUserSettingProperties properties)
    {
      var oo = new List<UserSettingsDbObject>();
      switch (_storageKind)
      {
        case StorageKind.File:
          throw new Exception("Not ready");
        case StorageKind.SqlClient:
          using (var conn = new SqlConnection(PathOrConnection))
          using (var cmd = new SqlCommand("SELECT id, AllowViewOthers, AllowEditOthers, created, " +
                                          "dcreated, updated, dupdated from _UserSettings " +
                                          "WHERE kind=@kind and [key]=@key and (AllowViewOthers=1 or created=@owner) " +
                                          "ORDER BY isnull(dupdated,dcreated) desc", conn))
          {
            conn.Open();
            cmd.Parameters.AddRange(new[]
            {
              new SqlParameter("@kind", properties.SettingKind),
              new SqlParameter("@key", properties.SettingKey),
              new SqlParameter("@owner", Utils.Tips.GetFullUserName())
            });
            using (var dr = cmd.ExecuteReader())
              while (dr.Read())
                oo.Add(new UserSettingsDbObject(dr));
          }
          break;
      }
      return oo;
    }

    public static List<string> GetKeysFromDb(IUserSettingProperties properties)
    {
      var oo = GetUserSettingDbObjects(properties);
      return oo.Select(o => o.SettingId).ToList();
    }

  }

}

