// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Buttplug;
using FFmpeg.AutoGen;
using osu.Framework.Audio.Track;
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
        IApplicableToBeatmap, IApplicableToTrack
    {
        public enum MotorMode
        {
            [Description("Not available / Do Nothing")]
            None,
            [Description("Bind to Health")]
            Health,
            [Description("Bind to Combo")]
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

        [SettingSource("Speed Cap - Limit motor max speed", "Maximum speed at which the motors will vibrate.")]
        public BindableNumber<float> SpeedCap { get; } = new BindableFloat
        {
            Precision = 0.01f,
            MinValue = 0.0f,
            MaxValue = 1.0f,
            Default = 1.0f,
            Value = 1.0f,
        };

        [SettingSource("Max Combo Factor", "Maximum speed at which the motors will vibrate.")]
        public BindableNumber<float> MaxComboFactor { get; } = new BindableFloat
        {
            Precision = 0.1f,
            MinValue = 0.0f,
            MaxValue = 1.0f,
            Default = 0.3f,
            Value = 0.3f,
        };

        [SettingSource("Motor 1", "Defines how the first motor will react.")]
        public Bindable<MotorMode> Motor1Reaction { get; } = new Bindable<MotorMode>(MotorMode.Health);

        [SettingSource("Motor 2", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor2Reaction { get; } = new Bindable<MotorMode>(MotorMode.Combo);

        [SettingSource("Motor 3", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor3Reaction { get; } = new Bindable<MotorMode>();

        [SettingSource("Motor 4", "Defines how the second motor will react.")]
        public Bindable<MotorMode> Motor4Reaction { get; } = new Bindable<MotorMode>();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += health =>
            {
                if(Motor1Reaction.Value == MotorMode.Health)
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


        public void ApplyToTrack(ITrack track)
        {
            track.Completed += ButtplugStuff.INSTANCE.StopAll;
            track.Failed += ButtplugStuff.INSTANCE.StopAll;
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

        public void StopAll()
        {
            client.StopAllDevicesAsync().ContinueWith(logExeptions, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void logExeptions(Task t)
        {
            var aggException = t.Exception.Flatten();
            foreach (Exception exception in aggException.InnerExceptions)
                Logger.Error(exception, $"Idk some exception from buttplug {exception.Message}");
        }
    }
}
