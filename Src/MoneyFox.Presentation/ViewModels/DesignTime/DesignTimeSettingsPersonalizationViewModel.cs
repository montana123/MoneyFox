﻿using System.Collections.Generic;
using System.Globalization;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Utilities;
using MoneyFox.ServiceLayer.ViewModels;

namespace MoneyFox.Presentation.ViewModels.DesignTime
{
    public class DesignTimeSettingsPersonalizationViewModel : ISettingsPersonalizationViewModel
    {
        public DesignTimeSettingsPersonalizationViewModel()
        {
            Resources = new LocalizedResources(typeof(Strings), CultureInfo.CurrentUICulture);
        }

        public int SelectedIndex { get; set; }
        public List<string> ThemeItems { get; } = new List<string>{Strings.ThemeLightLabel, Strings.ThemeLightLabel};
        public LocalizedResources Resources { get; }
    }
}
