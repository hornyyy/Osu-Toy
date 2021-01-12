using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Screens.Multi
{
    class MultiplayerConsent : OsuScreen
    {
        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        public override bool AllowBackButton => false;
        public override bool HideOverlaysOnEnter => true;

        public MultiplayerConsent()
        {
            LinkFlowContainer textFlow;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        textFlow = new LinkFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 2)
                        },
                        new TriangleButton {
                            Text = "I Understand",
                            Width = 200,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Action = () =>
                            {
                                config.Set(OsuSetting.MultiplayerConsentAcknowledged, true);
                                this.Exit();
                            }
                        }
                    }
                }
            };

            textFlow.AddText("Before you start...", t => t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Bold));
            textFlow.NewParagraph();

            textFlow.AddText(
                "Please do not use this mod while accessing public game servers with unsuspecting players.\n" +
                "This is meant for single player and/or matched play with consenting participants. Thank you!",
                t => t.Font = t.Font.With(Typeface.Torus, 24, FontWeight.SemiBold)
            );
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            this.FadeIn(250);
            Background.FadeColour(Colour4.Black, 250);
            music?.Stop();
        }

        public override bool OnExiting(IScreen next)
        {
            music?.EnsurePlayingSomething();
            this.FadeOut(250);

            return base.OnExiting(next);
        }
    }
}
