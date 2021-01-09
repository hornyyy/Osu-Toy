using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Overlays.Settings.Sections.Toy
{
    class IntifaceSettings : SettingsSubsection
    {
        protected override string Header => "Intiface";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsTextBox
                {
                    LabelText = "Intiface address (restart required)",
                    Current = config.GetBindable<string>(OsuSetting.IntifaceAddress)
                }
            };
        }
    }
}
