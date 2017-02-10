using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TinySteamWrapper;

namespace WhatToPlay.Model
{
    public class Friend : SteamProfile
    {
        public Friend(SteamProfile profile) : base()
        {
            PropertyInfo[] properties = typeof(SteamProfile).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(profile));
            }
        }
        public bool IsSelected { get; set; }
    }
}
