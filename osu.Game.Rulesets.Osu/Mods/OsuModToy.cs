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
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModToy : Mod, IApplicableToHealthProcessor, IApplicableToScoreProcessor,
        IApplicableToBeatmap, IApplicableToPlayer, IApplicableToDrawableHitObjects
    {
        public enum MotorBehavior
        {
            [Description("Do nothing")] None,
            [Description("Bind to health")] Health,
            [Description("Bind to combo")] Combo,
            [Description("Bind to accuracy")] Accuracy,
            [Description("Bind to hit")] Hit
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

        private const int MOTOR_COUNT = 4;

        [SettingSource("Motor 1 Behavior", "Defines how the first motor will react.")]
        public Bindable<MotorBehavior> Motor1Behavior { get; } = new Bindable<MotorBehavior>(MotorBehavior.Health);

        [SettingSource("Invert Motor 1")] public BindableBool Motor1Invert { get; } = new BindableBool(true);

        [SettingSource("Motor 2 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorBehavior> Motor2Behavior { get; } = new Bindable<MotorBehavior>(MotorBehavior.Combo);

        [SettingSource("Invert Motor 2")] public BindableBool Motor2Invert { get; } = new BindableBool();

        [SettingSource("Motor 3 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorBehavior> Motor3Behavior { get; } = new Bindable<MotorBehavior>();

        [SettingSource("Invert Motor 3")] public BindableBool Motor3Invert { get; } = new BindableBool();

        [SettingSource("Motor 4 Behavior", "Defines how the second motor will react.")]
        public Bindable<MotorBehavior> Motor4Behavior { get; } = new Bindable<MotorBehavior>();

        [SettingSource("Invert Motor 4")] public BindableBool Motor4Invert { get; } = new BindableBool();

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            healthProcessor.Health.ValueChanged += health =>
            {
                if (!userPlaying) return;

                double speed = SpeedCap.Value * Math.Pow(health.NewValue, 4);

                for (uint i = 1; i <= MOTOR_COUNT; i++)
                {
                    var behavior = (Bindable<MotorBehavior>) GetType().GetProperty($"Motor{i}Behavior").GetValue(this);
                    var invert = (BindableBool) GetType().GetProperty($"Motor{i}Invert").GetValue(this);

                    if (behavior.Value == MotorBehavior.Health)
                        if (invert.Value)
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(1 - speed, i - 1);
                        else
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(speed, i - 1);
                }
            };
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            scoreProcessor.Combo.ValueChanged += combo =>
            {
                if (!userPlaying) return;

                float speed = SpeedCap.Value * Math.Max(1, combo.NewValue / (float) maxCombo * MaxComboFactor.Value);

                for (uint i = 1; i <= MOTOR_COUNT; i++)
                {
                    var behavior = (Bindable<MotorBehavior>) GetType().GetProperty($"Motor{i}Behavior").GetValue(this);
                    var invert = (BindableBool) GetType().GetProperty($"Motor{i}Invert").GetValue(this);

                    if (behavior.Value == MotorBehavior.Combo)
                        if (invert.Value)
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(1 - speed, i - 1);
                        else
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(speed, i - 1);
                }
            };

            scoreProcessor.Accuracy.ValueChanged += accuracy =>
            {
                if (!userPlaying) return;

                double speed = SpeedCap.Value * accuracy.NewValue;

                for (uint i = 1; i <= MOTOR_COUNT; i++)
                {
                    var behavior = (Bindable<MotorBehavior>) GetType().GetProperty($"Motor{i}Behavior").GetValue(this);
                    var invert = (BindableBool) GetType().GetProperty($"Motor{i}Invert").GetValue(this);

                    if (behavior.Value == MotorBehavior.Accuracy)
                        if (invert.Value)
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(1 - speed, i - 1);
                        else
                            ButtplugStuff.INSTANCE.VibrateAtSpeed(speed, i - 1);
                }
            };
        }

        // This probably doesn't work rn lol
        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (DrawableHitObject drawableHitObject in drawables)
            {
                drawableHitObject.State.ValueChanged += async state =>
                {
                    if (!userPlaying) return;

                    double speed = SpeedCap.Value;

                    for (uint i = 1; i <= MOTOR_COUNT; i++)
                    {
                        var behavior = (Bindable<MotorBehavior>) GetType().GetProperty($"Motor{i}Behavior").GetValue(this);
                        var invert = (BindableBool) GetType().GetProperty($"Motor{i}Invert").GetValue(this);

                        if (behavior.Value == MotorBehavior.Hit)
                            if (invert.Value)
                                ButtplugStuff.INSTANCE.VibrateAtSpeed(1 - speed, i - 1);
                            else
                                ButtplugStuff.INSTANCE.VibrateAtSpeed(speed, i - 1);
                        await Task.Delay(100);
                        ButtplugStuff.INSTANCE.VibrateAtSpeed(0, i - 1);
                    }
                };
            }
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

        public static float BoolToFloat(bool boolean)
        {
            if (boolean) return 1f;
            return -1f;
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
                    if (motor >
                        device.AllowedMessages[ServerMessage.Types.MessageAttributeType.VibrateCmd].FeatureCount - 1)
                        continue;

                    await device.SendVibrateCmd(new Dictionary<uint, double> {[motor] = speed});
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
