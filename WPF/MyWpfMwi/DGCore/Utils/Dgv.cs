using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DGCore.Utils {
  public static class Dgv {

    public static int GetVerticalScrollWidth (Control owner){
      System.Windows.Forms.Control.ControlCollection cc = owner.Controls;
      foreach (Control c in cc) {
        if (c is VScrollBar) {
          return c.Visible ? c.Width : 0;
        }
      }
      return 0;
    }

    public static void EndEdit(Control owner) {
      foreach (Control c in owner.Controls) {
        if (c is DataGridView) {
          ((DataGridView)c).EndEdit();
        }
        else {
          EndEdit(c);
        }
      }
    }

    public static void Refresh(Control owner) {
      foreach (Control c in owner.Controls) {
        c.Refresh();
        Refresh(c);
      }
    }

    public static void GetSelectedArea(DataGridView dgv, out int[] selectedRowNumbers, out object[] selectedObjects, out DataGridViewColumn[] selectedColumnsInDisplayOrder) {
//      int[] selectedRowNumbers;
      GetSelectedArea(dgv, out selectedRowNumbers, out selectedColumnsInDisplayOrder);

      selectedObjects = new object[selectedRowNumbers.Length];
      object data = ListBindingHelper.GetList(dgv.DataSource, dgv.DataMember);
      if (data is IList) {
        IList data1 = (IList)data;
        for (int i = 0; i < selectedObjects.Length; i++) selectedObjects[i] = data1[selectedRowNumbers[i]];
      }
      else {
        throw new Exception("AAA");
      }
//      IList data = (IList)dgv.DataSource;
  //    for (int i = 0; i < selectedObjects.Length; i++) selectedObjects[i] = data[selectedRowNumbers[i]];
    }

    public static void GetSelectedArea(DataGridView dgv, out int[] selectedRowNumbers, out DataGridViewColumn[] selectedColumnsInDisplayOrder) {
      // DataSource of Datagridview must be IList
      List<int> rows = new List<int>();
      List<int> cols = new List<int>();
      DataGridViewColumn[] cc = GetColumnsInDisplayOrder(dgv, true);

      FieldInfo fi1 = typeof(DataGridView).GetField("individualSelectedCells", BindingFlags.Instance | BindingFlags.NonPublic);
      FieldInfo fi2 = typeof(DataGridView).GetField("selectedBandIndexes", BindingFlags.Instance | BindingFlags.NonPublic);
      IEnumerable cells = (IEnumerable)fi1.GetValue(dgv);
      IEnumerable bands = (IEnumerable)fi2.GetValue(dgv);

      switch (dgv.SelectionMode) {
        case DataGridViewSelectionMode.CellSelect:
          foreach (DataGridViewCell cell in cells) {
            if (!rows.Contains(cell.RowIndex)) rows.Add(cell.RowIndex);
            if (!cols.Contains(cell.ColumnIndex)) cols.Add(cell.ColumnIndex);
          }
          break;

        case DataGridViewSelectionMode.FullRowSelect:
        case DataGridViewSelectionMode.RowHeaderSelect:
          foreach (int i in bands) rows.Add(i);
          if (rows.Count > 0) {
            foreach (DataGridViewColumn c in cc) cols.Add(c.Index);
          }
          if (dgv.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect) {
            foreach (DataGridViewCell cell in cells) {
              if (!rows.Contains(cell.RowIndex)) rows.Add(cell.RowIndex);
              if (!cols.Contains(cell.ColumnIndex)) cols.Add(cell.ColumnIndex);
            }
          }
          break;

        case DataGridViewSelectionMode.FullColumnSelect:
        case DataGridViewSelectionMode.ColumnHeaderSelect:
          foreach (int i in bands) cols.Add(i);
          if (cols.Count > 0) {
            for (int i = 0; i < dgv.Rows.Count; i++) rows.Add(i);
          }
          if (dgv.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect) {
            foreach (DataGridViewCell cell in cells) {
              if (!rows.Contains(cell.RowIndex)) rows.Add(cell.RowIndex);
              if (!cols.Contains(cell.ColumnIndex)) cols.Add(cell.ColumnIndex);
            }
          }
          break;

      }
      // Prepare object list
      rows.Sort();
      // Remove new row number
      if (dgv.Rows.Count > 0 && dgv.Rows[dgv.Rows.Count - 1].IsNewRow && rows.Count > 0) {
        if (rows[rows.Count - 1] == dgv.Rows.Count - 1) rows.RemoveAt(rows.Count - 1);
      }
      // Prepare column list
      selectedColumnsInDisplayOrder = new DataGridViewColumn[cols.Count];
      int cnt = 0;
      foreach (DataGridViewColumn c in cc) {
        if (cols.Contains(c.Index)) selectedColumnsInDisplayOrder[cnt++] = c;
      }
      selectedRowNumbers = rows.ToArray();
    }


    public static void CreateComboColumnsForEnumerations(DataGridView dgv) {
      for (int i = 0; i < dgv.Columns.Count; i++) {
        DataGridViewColumn col = dgv.Columns[i];
        if (col.ValueType!=null && col.ValueType.IsEnum && col is DataGridViewTextBoxColumn) {
          // create combo column for enum types
          DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn();
          cmb.ValueType = col.ValueType;
          cmb.Name = col.Name;
          cmb.DataPropertyName = col.DataPropertyName;
          cmb.HeaderText = col.HeaderText;
          cmb.DisplayStyleForCurrentCellOnly = true;//???
          Array values = Enum.GetValues(col.ValueType);
          cmb.DataSource = values;
          cmb.MaxDropDownItems = Math.Min(22, values.Length);

          // Copy the default style
          PropertyInfo[] pis = col.DefaultCellStyle.GetType().GetProperties();
          foreach (PropertyInfo pi in pis) {
            if (pi.CanRead && pi.CanWrite) {
              pi.SetValue(cmb.DefaultCellStyle, pi.GetValue(col.DefaultCellStyle, null), null);
            }
          }

          // Measure the column width
          TypeConverter tc = TypeDescriptor.GetConverter(cmb.ValueType);
          List<string> ss = new List<string>();
          if (tc != null) {
            foreach (object o in (Array)cmb.DataSource) {
              ss.Add((string)tc.ConvertTo(o, typeof(string)));
            }
          }
          else {
            foreach (object o in values) {
              ss.Add(o.ToString());
            }
          }
          Font f = dgv.DefaultCellStyle.Font;
          float width=0;
          using (Graphics g= dgv.CreateGraphics()) {
            foreach (string s in ss) {
              width = Math.Max(width, g.MeasureString(s, f).Width);
            }
          }
          cmb.Width = Convert.ToInt32(width) + 4;

          // replace original column with new combo column
          dgv.Columns.RemoveAt(i);
          dgv.Columns.Insert(i, cmb);

        }
      }
    }

    public static PropertyDescriptorCollection GetInternalPropertyDescriptorCollection(DataGridView dgv) {
      PropertyInfo pi = typeof(DataGridView).GetProperty("DataConnection", BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
      object o = pi.GetValue(dgv, new object[0]);
      FieldInfo fiPDC = o.GetType().GetField("props", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      return (PropertyDescriptorCollection)fiPDC.GetValue(o);
    }
    public static int GetColumnIndexByPropertyName(DataGridView dgv, string propertyName) {
      foreach (DataGridViewColumn c in dgv.Columns) {
        if (c.DataPropertyName == propertyName) return c.Index;
      }
      return -1;
    }

    public static DataGridViewColumn[] GetColumnsInDisplayOrder(DataGridView dgv, bool onlyVisibleColumns)
    {
      return (dgv.Columns.Cast<DataGridViewColumn>()).Where(c => c.Visible || !onlyVisibleColumns)
        .OrderBy(c => c.DisplayIndex).ToArray();
      /*List<DataGridViewColumn> cc = new List<DataGridViewColumn>();
      foreach (DataGridViewColumn c in dgv.Columns) {
        if (c.Visible || !onlyVisibleColumns) {
          cc.Add(c);
        }
      }
      cc.Sort(DGVColumnOrderCompare);
      return cc.ToArray();*/
    }

    //============
    public static void ScrollIntoCurrentCell(DataGridView dgv) {
      // sometimes (especially when frozen columns exist) DatagridView does bad scroll into CurrentCell
      if (dgv.CurrentCell != null && dgv.CurrentCell.OwningColumn.Index >= 0) {
        // does not work correctly!  Rectangle r1 = dgv.GetColumnDisplayRectangle(dgv.CurrentCell.OwningColumn.Index, false);
        int colIndex = dgv.CurrentCell.OwningColumn.Index;
        int rowIndex = dgv.CurrentCell.OwningRow.Index;
        DataGridViewColumn c = dgv.Columns.GetPreviousColumn(dgv.CurrentCell.OwningColumn, DataGridViewElementStates.Displayed, DataGridViewElementStates.None);
        if (c == null) c = dgv.Columns.GetNextColumn(dgv.CurrentCell.OwningColumn, DataGridViewElementStates.Displayed, DataGridViewElementStates.None);
        if (c != null) {
          dgv.CurrentCell = dgv[c.Index, rowIndex];
          Application.DoEvents();
          dgv.CurrentCell = dgv[colIndex, rowIndex];
        }
      }
    }
    public static void SetNewCurrentCell(DataGridView dgv, DataGridViewCell newCurrentCell) {
      dgv.CurrentCell = newCurrentCell;
      ScrollIntoCurrentCell(dgv);
    }

    public static SizeF GetCellWrappedSize(Graphics g, string formattedText, Font font, float startWidth) {
      string[] ss = formattedText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
//      float width = 0f;
      for (int i = 0; i < ss.Length; i++) {
        if (ss[i].EndsWith("валюти")) {
        }
        startWidth = Math.Max(startWidth, g.MeasureString(ss[i], font).Width);
      }
      StringFormat sf = new StringFormat( StringFormatFlags.NoClip);
/*      SizeF s1 = g.MeasureString(formattedText, font, new PointF(0, 0), sf);
      SizeF s2 = g.MeasureString(formattedText, font, new SizeF(width, 0), sf);*/
      SizeF s3 = g.MeasureString(formattedText, font, new SizeF(startWidth + 0.5f, 0), sf);
      return g.MeasureString(formattedText, font, new SizeF(startWidth + 0.5f, 0), sf);
    }
  }
}
