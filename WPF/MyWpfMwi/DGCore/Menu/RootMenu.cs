using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DGCore.Menu
{

    public enum DbProvider
    {
        SqlClient,
        MySqlClient,
        OleDb,
        File
    }

    public partial class RootMenu : SubMenu
    {
        private MainObject _mainObject;

        public string ApplicationTitle => _mainObject.Title;

        public RootMenu(string jsonFileName) : base("Root")
        {
            Init(jsonFileName);
        }

        private void Init(string jsonFileName)
        {
            var errors = new List<Exception>();
            var settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) => errors.Add(args.ErrorContext.Error)
            };
            try
            {
                _mainObject = JsonConvert.DeserializeObject<MainObject>(File.ReadAllText(jsonFileName), settings);
            }
            catch (Exception ex) { }

            if (errors.Count != 0)
            {
                MessageBox.Show($@"Список помилок конфігурації. Файл:{jsonFileName}" + Environment.NewLine +
                                string.Join(Environment.NewLine, errors.Select(e => e.Message).Distinct()));
                Utils.Tips.ExitApplication();
            }

            _mainObject.Normalize();

            var isFirstConnectionString = true;
            foreach (var kvp in _mainObject.DbConnections)
            {
                var cs = kvp.Value.Provider + ";" + kvp.Value.CS.Trim();
                DB.DbCmd._standardConnections.Add(kvp.Key, cs);
                if (isFirstConnectionString)
                {
                    Misc.AppSettings.settingsStorage = cs;
                    isFirstConnectionString = false;
                }
            }

            var menuItems = new Dictionary<int, SubMenu>();
            foreach (var mo in _mainObject.FlatMenu)
            {
                if (mo.IsSubmenu)
                {
                    var item = new SubMenu(mo.Label);
                    menuItems.Add(mo.Id, item);
                    if (mo.ParentId.HasValue) // parent is root
                        menuItems[mo.ParentId.Value].Items.Add(item);
                    else
                        Items.Add(item);
                }
                else
                {
                    if (mo.ParentId.HasValue)
                        menuItems[mo.ParentId.Value].Items.Add(mo);
                    else
                        Items.Add(mo);
                }
            }
        }

    }
}
