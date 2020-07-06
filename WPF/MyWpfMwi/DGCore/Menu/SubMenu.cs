﻿using System.Collections.Generic;

namespace DGCore.Menu
{
    public class SubMenu
    {
        // readonly string _text;
        public string Label { get; }
        public List<object> Items { get; } = new List<object>();
        public SubMenu(string text)
        {
            Label = text;
        }
        public override string ToString() => Label;
    }

}
