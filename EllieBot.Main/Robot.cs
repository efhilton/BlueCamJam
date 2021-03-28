﻿using EllieBot.IO;
using EllieBot.Brain;
using EllieBot.Brain.Commands;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EllieBot {

    public class Robot {
        private Communications.NervousSystem comms;
        private readonly ICommandProcessor commandProcessor;
        private readonly RobotConfig configs;
        private readonly Action<string> logger;

        public static Robot Instance { get; private set; }

        private Robot(ICommandProcessor cmdProcessor,
                     RobotConfig configs,
                     Action<string> logger = null) {
            this.commandProcessor = cmdProcessor;
            this.configs = configs;
            this.logger = logger;
        }

        public Task Initialize() {
            this.comms = new Communications.NervousSystem();
            this.comms.ConnectAsync(this.configs.MqttServer, this.configs.MqttPort).Wait();

            return this.comms.SubscribeAsync(this.configs.MqttTopicForCommands, this.OnDataReceived, this.OnConnection, this.OnDisconnection);
        }

        internal static Robot CreateInstance(ICommandProcessor proc, RobotConfig configs, Action<string> logger = null) {
            if (Instance != null) {
                return Instance;
            }
            Instance = new Robot(proc, configs, logger);
            return Instance;
        }

        public Task PublishAsync(string message) {
            return this.comms.PublishAsync(this.configs.MqttTopicForCommands, message);
        }

        private Task OnDisconnection(MqttClientDisconnectedEventArgs arg) {
            return Task.Run(() => {
                this.logger?.Invoke("Client Disconnected");
            });
        }

        private Task OnConnection(MqttClientConnectedEventArgs arg) {
            return Task.Run(() => {
                this.logger?.Invoke("Client Connected");
            });
        }

        private Task OnDataReceived(MqttApplicationMessageReceivedEventArgs arg) {
            return Task.Run(() => {
                string Payload = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
                try {
                    CommandPacket cmd = JsonConvert.DeserializeObject<CommandPacket>(Payload);
                    this.commandProcessor.QueueExecute(cmd);
                } catch (Exception) {
                    this.logger?.Invoke($"Ignored: {Payload}");
                }
            });
        }
    }
}
