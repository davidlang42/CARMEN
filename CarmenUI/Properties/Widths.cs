﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.Properties
{
    /// <summary>
    /// Based on the autogenerated Settings class
    /// </summary>
    internal sealed class Widths : ApplicationSettingsBase
    {
        private static Widths defaultInstance = (Widths)Synchronized(new Widths());

        public static Widths Default => defaultInstance;

        private Widths()
        {
            AllocateRolesGrid ??= new();
        }

        /// <summary>The width in pixels of columns in the AllocateRoles grid view</summary>
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public Dictionary<string, int> AllocateRolesGrid
        {
            get => (Dictionary<string, int>)this[nameof(AllocateRolesGrid)];
            private set
            {
                this[nameof(AllocateRolesGrid)] = value;
            }
        }
    }
}
