using System;
using System.IO;
using System.Windows.Forms;
using GTA;

namespace GTAIVMortorCycleTrafficOnly.Scripts
{
    internal class TrafficReplacer : Script
    {
        private class ModSettings
        {
            public int Radius = 400;
            public int WorkInterval = 1000;
            public string Model = "nrg900";
            public Keys ToggleKey;
        }

        private ModSettings settings;
        private bool enabled = false;

        public TrafficReplacer()
        {
            // Bind method BasicKeyExample_KeyDown to the KeyDown event
            this.KeyDown += this.BasicKeyExample_KeyDown;
            this.Tick += OnTick;

            settings = new ModSettings();
            InitSettings();
        }

        private void OnTick(object sender, EventArgs e)
        {
            Game.WaitInCurrentScript(settings.WorkInterval);
            if (enabled)
            {
                try
                {
                    DoMotorcycles();
                }
                catch (Exception exception)
                {
                    Game.Console.Print(exception.Message + Environment.NewLine + exception.InnerException);
                }
            }
        }

        private void DoMotorcycles()
        {
            var vehicles = settings.Radius <= 0
                ? World.GetAllVehicles()
                : World.GetVehicles(Player.Character.Position, settings.Radius);

            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].Model != Model.FromString(settings.Model)
                    && !vehicles[i].isSeatFree(VehicleSeat.Driver))
                {
                    string playerCar = Player.LastVehicle is null ? "" : Player.LastVehicle.Name;
                    if (vehicles[i].Name != playerCar)
                    {
                        var lastDirection = vehicles[i].Direction;
                        var nv = vehicles[i] =
                            World.CreateVehicle(Model.FromString(settings.Model), vehicles[i].Position);
                        nv.CreatePedOnSeat(VehicleSeat.Driver);
                    }
                }
            }
        }

        private void ClearAllVehicles()
        {
            foreach (var v in World.GetAllVehicles())
            {
                string playerCar = Player.LastVehicle is null ? "" : Player.LastVehicle.Name;
                if (v.Name != playerCar)
                {
                    v.Delete();
                }
            }
        }

        private void BasicKeyExample_KeyDown(object sender, GTA.KeyEventArgs e)
        {
            if (e.Key == settings.ToggleKey)
            {
                InitSettings();
                Game.DisplayText(!enabled
                    ? "Enabling motorcycle traffic mod..."
                    : "Disabling motorcycle traffic mod...\nClearing Vehicles...");
                enabled = !enabled;
                if (!enabled)
                {
                    ClearAllVehicles();
                }
            }
        }

        private void InitSettings()
        {
            string settingsPath = "scripts/TrafficModelChangerConfig.txt";

            if (File.Exists(settingsPath))
            {
                var content = File.ReadAllLines(settingsPath);

                foreach (var line in content)
                {
                    var pair = line.Split('=');
                    if (pair.Length == 2)
                    {
                        switch (pair[0])
                        {
                            case "Model":
                                settings.Model = pair[1];
                                break;

                            case "Radius":
                                settings.Radius = int.Parse(pair[1]);
                                break;

                            case "WorkInterval":
                                settings.WorkInterval = int.Parse(pair[1]);
                                break;

                            case "ToggleKey":
                                settings.ToggleKey = (Keys)Enum.Parse(typeof(Keys), pair[1], true);
                                break;
                        }
                    }
                }
            }
            else
            {
                File.WriteAllText(settingsPath, $"Model={settings.Model}\n" +
                                                $"Radius={settings.Radius.ToString()}\n" +
                                                $"WorkInterval={settings.WorkInterval.ToString()}\n" +
                                                $"ToggleKey=77");
            }
        }
    }
}