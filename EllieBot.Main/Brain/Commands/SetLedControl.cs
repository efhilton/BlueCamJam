﻿using EllieBot.IO.Devices;
using System;
using System.Collections.Generic;

namespace EllieBot.Brain.Commands {

    internal class SetLedControl : ICommandExecutor {
        private const string ON = "led.on";
        private const string OFF = "led.off";
        private readonly Dictionary<string, IBlinkable> Blinkables;
        private readonly Action<string> Logger;

        public SetLedControl(IBlinkable[] blinkables, Action<string> logger = null) {
            this.Logger = logger;
            this.Blinkables = new Dictionary<string, IBlinkable>();
            if (blinkables == null) {
                return;
            }
            foreach (IBlinkable b in blinkables) {
                if (!string.IsNullOrWhiteSpace(b.UniqueId)) {
                    string id = b.UniqueId.Trim().ToUpper();
                    this.Blinkables.Add(id, b);
                    this.Logger?.Invoke($"Registered LED: {id}");
                }
            }
        }

        public string[] Commands => new string[] { ON, OFF };

        public void Execute(RobotCommand command) {
            if (string.IsNullOrWhiteSpace(command.Command) || command.Arguments == null || command.Arguments.Length != 1) {
                return;
            }

            string deviceId = command.Arguments[0];
            if (string.IsNullOrWhiteSpace(deviceId)) {
                return;
            }

            this.Blinkables.TryGetValue(deviceId.ToUpper().Trim(), out IBlinkable device);
            if (command.Command.Trim().Equals(ON, StringComparison.OrdinalIgnoreCase)) {
                device.TurnOn();
            } else {
                device.TurnOff();
            }
        }
    }
}
