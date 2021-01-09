// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Buttplug;
using FFmpeg.AutoGen;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModToy : Mod, IApplicableToHealthProcessor, IApplicableFailOverride, IApplicableToScoreProcessor,
        IApplicableToBeatmap
    {
        public override string Name => "Toy";
        public override string Description => "Play with toys.";
        public override string Acronym => "TY";

        public override IconUsage? Icon => FontAwesome.Solid.Adjust;
        public override ModType Type => ModType.Fun;

        public override bool Ranked => false;

        public bool RestartOnFail => false;

        public override double ScoreMultiplier => 0.0;

        private int maxCombo = 1;

        private const double SPEED_CAP = 1.0;

        public bool PerformFail()
        {
            return false;
        }

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += health =>
            {
                ButtplugStuff.INSTANCE.VibrateAtSpeed(SPEED_CAP * (1 - Math.Pow(health.NewValue, 4)));
            };
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.ValueChanged += combo =>
            {
                ButtplugStuff.INSTANCE.VibrateAtSpeed(SPEED_CAP * (combo.NewValue / (maxCombo / (float) 3)), 1);
            };
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            return rank;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            maxCombo = beatmap.HitObjects.Count;
        }
    }

    public class ButtplugStuff
    {
        private static readonly object padlock = new object();
        private static ButtplugStuff instance;

        public static ButtplugStuff INSTANCE
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null) instance = new ButtplugStuff();
                    return instance;
                }
            }
        }

        private readonly ButtplugClient client;

        public ButtplugStuff()
        {
            var connector = new ButtplugWebsocketConnectorOptions(new Uri("ws://127.0.0.1:12345"));
            client = new ButtplugClient("OsuClient");

            try
            {
                client.ConnectAsync(connector).ContinueWith(logExeptions, TaskContinuationOptions.OnlyOnFaulted);
                Logger.Error(null, "Connected!!!");
            }
            catch (ButtplugConnectorException)
            {
                Logger.Error(null, "Failed to Connect");
            }
        }

        #region Disposal

        ~ButtplugStuff()
        {
            try
            {
                if (client.Connected)
                {
                    client.StopAllDevicesAsync().ContinueWith(logExeptions, TaskContinuationOptions.OnlyOnFaulted);
                    client.DisconnectAsync().ContinueWith(logExeptions, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
        }

        #endregion

        public void VibrateAtSpeed(double speed, uint motor = 0)
        {
            foreach (var device in client.Devices)
            {
                try
                {
                    device.SendVibrateCmd(new Dictionary<uint, double> {[motor] = speed})
                        .ContinueWith(logExeptions, TaskContinuationOptions.OnlyOnFaulted);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private void logExeptions(Task t)
        {
            var aggException = t.Exception.Flatten();
            foreach (Exception exception in aggException.InnerExceptions)
                Logger.Error(exception, $"Idk some exception from buttplug {exception.Message}");
        }
    }
}
