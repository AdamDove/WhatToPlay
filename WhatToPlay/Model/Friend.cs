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
        public Friend() : base() { }
        public Friend(SteamProfile profile) : base()
        {
            InitializeProperties(profile);
        }

        public void InitializeProperties(SteamProfile profile)
        {
            PropertyInfo[] properties = typeof(SteamProfile).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(profile));
            }
        }
        public void InitializeFields(SteamProfile profile)
        {
            FieldInfo[] fields = typeof(SteamProfile).GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                field.SetValue(this, field.GetValue(profile));
            }
        }
        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                };
            }
        }
        private bool isVisible;
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    NotifyPropertyChanged("IsVisible");
                };
            }
        }
    }
}