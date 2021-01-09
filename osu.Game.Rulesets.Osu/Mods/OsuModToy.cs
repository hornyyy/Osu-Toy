// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Buttplug;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModToy : Mod, IApplicableToHealthProcessor, IApplicableToScoreProcessor,
        IApplicableToBeatmap, IApplicableToPlayer
    {
        public enum MotorMode
        {
            [Description("Do nothing")]
            None,
            [Description("Bind to health")]
            Health,
            [Description("Bind to combo")]
            Combo
        }

        public override string Name => "Toy";
        public override string Description => "Play with toys.";
        public override string Acronym => "TY";

        public override IconUsage? Icon => FontAwesome.Solid.PepperHot;
        public override ModType Type => ModType.Fun;
        public override bool Ranked => false;

        public override double ScoreMultiplier => 0.0;

        private int maxCombo = 1;
        private bool userPlaying = false;

        [SettingSource("Motor Speed Max", "Maximum speed at which the motors will vibrate.")]
        public BindableNumber<float> SpeedCap { get; } = new BindableFloat
        {
            Precision = 0.01f,
            MinValue = 0.0f,
            MaxValue = 1.0f,
            Default = 1.0f,
            Value = 1.0f,
        };

        [SettingSource("Combo Factor Max")]
        public BindableNumber<float> MaxComboFactor { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 0.0f,
            MaxValue = 1.0f,
            Default = 0.3f,
            Value = 0.3f,
        };

        [SettingSource("Motor 1 Behavior", "Defines how the first motor will react.")]
        public Bindable<MotorMode> Motor1Reaction { get; } = new Bindable<MotorMode>(MotorMode.Health);

        [SettingSource("Motor 2 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor2Reaction { get; } = new Bindable<MotorMode>(MotorMode.Combo);

        [SettingSource("Motor 3 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor3Reaction { get; } = new Bindable<MotorMode>();

        [SettingSource("Motor 4 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor4Reaction { get; } = new Bindable<MotorMode>();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += health =>
            {
                if (!userPlaying) return;

                if (Motor1Reaction.Value == MotorMode.Health)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (1 - Math.Pow(health.NewValue, 4)), 0);
                if(Motor2Reaction.Value == MotorMode.Health)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (1 - Math.Pow(health.NewValue, 4)), 1);
                if(Motor3Reaction.Value == MotorMode.Health)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (1 - Math.Pow(health.NewValue, 4)), 2);
                if(Motor4Reaction.Value == MotorMode.Health)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (1 - Math.Pow(health.NewValue, 4)), 3);
            };
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.ValueChanged += combo =>
            {
                if (!userPlaying) return;

                if(Motor1Reaction.Value == MotorMode.Combo)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (combo.NewValue / (float) maxCombo * MaxComboFactor.Value), 0);
                if(Motor2Reaction.Value == MotorMode.Combo)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (combo.NewValue / (float) maxCombo * MaxComboFactor.Value), 1);
                if(Motor3Reaction.Value == MotorMode.Combo)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (combo.NewValue / (float) maxCombo * MaxComboFactor.Value), 2);
                if(Motor4Reaction.Value == MotorMode.Combo)
                    ButtplugStuff.INSTANCE.VibrateAtSpeed(SpeedCap.Value * (combo.NewValue / (float) maxCombo * MaxComboFactor.Value), 3);
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

        public void ApplyToPlayer(Player player)
        {
            player.LocalUserPlaying.ValueChanged += playing =>
            {
                userPlaying = playing.NewValue;
                if (playing.NewValue == false)
                    ButtplugStuff.INSTANCE.StopAll();
            };
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
            client = new ButtplugClient("OsuClient");
            client.DeviceAdded += DeviceFound;

            Connect();
        }

        private async void DeviceFound(object sender, DeviceAddedEventArgs e)
        {
            await client.StopScanningAsync();
        }

        private async void Connect()
        {
            var connector = new ButtplugWebsocketConnectorOptions(new Uri("ws://127.0.0.1:12345"));

            try
            {
                await client.ConnectAsync(connector);
                await client.StartScanningAsync();
            }
            catch (ButtplugConnectorException e)
            {
                Logger.Error(e, "Failed to connect to Buttplug :(");
            }
        }

        public async void VibrateAtSpeed(double speed, uint motor = 0)
        {
            foreach (var device in client.Devices)
            {
                try
                {
                    await device.SendVibrateCmd(new Dictionary<uint, double> { [motor] = speed });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public void StopAll()
        {
            client.StopAllDevicesAsync().ContinueWith(LogExeptions, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void LogExeptions(Task t)
        {
            var aggException = t.Exception.Flatten();
            foreach (Exception exception in aggException.InnerExceptions)
                Logger.Error(exception, $"Idk some exception from buttplug {exception.Message}");
        }
    }
}
