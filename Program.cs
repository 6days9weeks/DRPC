using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DiscordRP.Properties;
using DiscordRPC;

namespace DiscordRP
{
    class Program
    {
        public static RichPresence current = new RichPresence();
        public static bool running = false;
        public static DiscordRpcClient client = null;

        public static void Print(params string[] args)
        {
            Console.WriteLine(string.Join(" ", args));
        }
        public static string Input(params string[] args)
        {
            Console.Write(string.Join(" ", args));
            return Console.ReadLine();
        }
        public static string Nullify(string txt)
        {
            return txt == "" ? null : txt;
        }
        public static string Tag(User user)
        {
            string d = user.Discriminator + "";
            while (d.Length < 4) d = "0" + d;
            return user.Username + "#" + d;
        }
        public static RichPresence FromSettings()
        {
            return new RichPresence()
            {
                State = Nullify(Settings.Default.rpState),
                Details = Nullify(Settings.Default.rpDetails),
                Assets = new Assets()
                {
                    LargeImageKey = Nullify(Settings.Default.rpLargeKey),
                    LargeImageText = Nullify(Settings.Default.rpLargeText),
                    SmallImageKey = Nullify(Settings.Default.rpSmallKey),
                    SmallImageText = Nullify(Settings.Default.rpSmallText),
                },
                Timestamps = Settings.Default.rpTimer ? new Timestamps(DateTime.Now) : null,
            };
        }
        public static void Events(DiscordRpcClient c)
        {
            c.OnReady += (obj, e) => {
                running = true;
                Print("\nConnected! User:", Tag(e.User), "(Press Enter)");
            };

            c.OnClose += (obj, e) => {
                running = false;
                client = null;
                Print("\nRPC Connection Closed", "(Press Enter)");
            };

            c.OnConnectionFailed += (obj, e) => {
                client = null;
                running = false;
                Print("\nRPC Connection Failed!", "(Press Enter)");
            };

            c.OnPresenceUpdate += (obj, e) => {
                running = true;
                Print("\nPresence Updated!", "(Press Enter)");
            };
        }
        static void Main(string[] args)
        {
            var timer = new System.Timers.Timer(150);
            timer.Elapsed += (sender, sargs) => { if (Settings.Default.rpTimer && client != null && running) client.UpdateClearTime(); };
            timer.Start();

            Print("===== Discord RPC =====");
            if (Settings.Default.autostart && Settings.Default.clientID != "none")
            {
                Print("Auto Start configuration found, connecting...");
                client = new DiscordRpcClient(Settings.Default.clientID);
                Events(client);
                client.Initialize();
                client.SetPresence(FromSettings());
            }
            Print("Type \"help\" for a list of commands");

            while(true)
            {
                string inp = Input("Command: ");
                if (inp == "") continue;

                List<string> cmdargs = new List<string>();
                foreach (var arg in inp.Split(' '))
                {
                    if (arg == "") continue;
                    cmdargs.Add(arg);
                }

                string cmd = cmdargs[0];
                cmdargs.RemoveAt(0);
                string cmdInput = cmdargs.Count == 0 ? "" : inp.Substring(cmd.Length, inp.Length - cmd.Length).Trim();

                // Print("CMD:", cmd, ", CmdInput:", cmdInput);

                if (cmd == "id")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Client ID too!");
                        continue;
                    }

                    Settings.Default.clientID = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Client ID to", cmdInput + "!");
                }
                else if (cmd == "state" || cmd == "st")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid State Text too!");
                        continue;
                    }

                    Settings.Default.rpState = cmdInput;
                    Settings.Default.Save();
                    Print("Configured State Text to", cmdInput + "!");
                }
                else if (cmd == "detail" || cmd == "details" || cmd == "dt")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Details Text too!");
                        continue;
                    }

                    Settings.Default.rpDetails = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Details Text to", cmdInput + "!");
                }
                else if (cmd == "largetext" || cmd == "largetxt" || cmd == "ltxt")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Large Image Text too!");
                        continue;
                    }

                    Settings.Default.rpLargeText = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Large Image Text to", cmdInput + "!");
                }
                else if (cmd == "smalltext" || cmd == "smalltxt" || cmd == "stxt")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Small Image Text too!");
                        continue;
                    }

                    Settings.Default.rpSmallText = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Small Image Text to", cmdInput + "!");
                }
                else if (cmd == "largekey" || cmd == "largeimg" || cmd == "limg" || cmd == "lkey" || cmd == "largeimage")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Large Image Key too!");
                        continue;
                    }

                    Settings.Default.rpLargeKey = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Large Image Key to", cmdInput + "!");
                }
                else if (cmd == "smallkey" || cmd == "smallimg" || cmd == "simg" || cmd == "skey" || cmd == "smallimage")
                {
                    if (cmdInput == "")
                    {
                        Print("Invalid Usage. Please input a valid Small Image Key too!");
                        continue;
                    }

                    Settings.Default.rpSmallKey = cmdInput;
                    Settings.Default.Save();
                    Print("Configured Small Image Key to", cmdInput + "!");
                }
                else if (cmd == "run" || cmd == "start")
                {
                    if (running || client != null)
                    {
                        Print("RPC Connection already established!");
                        continue;
                    }

                    if(Settings.Default.clientID == "" || Settings.Default.clientID == "none")
                    {
                        Print("Add a valid Client ID first using \"id <insert id here>\" command!");
                        continue;
                    }

                    client = new DiscordRpcClient(Settings.Default.clientID);
                    Events(client);
                    client.Initialize();
                    client.SetPresence(FromSettings());
                    Settings.Default.Save();

                }
                else if (cmd == "info" || cmd == "current")
                {
                    Print("========================================");
                    Print("Running:    " + (running ? "Yes" : "No"));
                    Print("Client ID:  " + Nullify(Settings.Default.clientID) ?? "None");
                    Print("State:      " + Nullify(Settings.Default.rpState) ?? "None");
                    Print("Details:    " + Nullify(Settings.Default.rpDetails) ?? "None");
                    Print("Large Text: " + Nullify(Settings.Default.rpLargeText) ?? "None");
                    Print("Large Img:  " + Nullify(Settings.Default.rpLargeKey) ?? "None");
                    Print("Small Text: " + Nullify(Settings.Default.rpSmallText) ?? "None");
                    Print("Small Img:  " + Nullify(Settings.Default.rpSmallKey) ?? "None");
                    Print("Timer?:     " + (Settings.Default.rpTimer ? "Enabled" : "Disabled"));
                    Print("========================================");
                }
                else if (cmd == "timer" || cmd == "t")
                {
                    Settings.Default.rpTimer = !Settings.Default.rpTimer;
                    Settings.Default.Save();
                    Print("RPC Timer:", Settings.Default.rpTimer ? "Enabled" : "Disabled");
                }
                else if (cmd == "update" || cmd == "u" || cmd == "up")
                {
                    if (!running || client == null)
                    {
                        Print("RPC not connected");
                        continue;
                    }

                    client.SetPresence(FromSettings());
                }
                else if(cmd == "autostart" || cmd == "as" || cmd == "auto")
                {
                    Settings.Default.autostart = !Settings.Default.autostart;
                    Settings.Default.Save();
                    Print("RPC AutoStart:", Settings.Default.autostart ? "Enabled" : "Disabled");
                }
                else if(cmd == "dc" || cmd == "end" || cmd == "disconnect" || cmd == "stop")
                {
                    if(!running || client == null)
                    {
                        Print("RPC is not even running!");
                        continue;
                    }

                    client.Dispose();
                    client = null;
                    running = false;
                }
                else if(cmd == "x" || cmd == "exit" || cmd == "q" || cmd == "quit")
                {
                    Settings.Default.Save();
                    if (client != null) client.Dispose();
                    Environment.Exit(0);
                }
                else if(cmd == "help" || cmd == "cmds" || cmd == "commands" || cmd == "h" || cmd == "hlp" || cmd == "halp")
                {
                    Print("============== Commands ================");
                    Print("info:       See current configuration   ");
                    Print("id:         Set Client ID               ");
                    Print("state:      Set State Text              ");
                    Print("details:    Set Details                 ");
                    Print("largetext:  Set Large Image Text        ");
                    Print("largeimg:   Set Large Image Key         ");
                    Print("smalltext:  Set Small Image Text        ");
                    Print("smallimg:   Set Small Image Key         ");
                    Print("timer:      Toggle Timer                ");
                    Print("autostart:  Toggle AutoStart on Startup ");
                    Print("clear:      Clear Discord RP            ");
                    Print("stop:       Stop/DC from Discord RP     ");
                    Print("exit:       Exit the application        ");
                    Print("========================================");
                    Print("/============================================\\");
                    Print("| Note: All the commands which set something |");
                    Print("| or toggle, store the data on your PC. So   |");
                    Print("| you don't have to enter each time.         |");
                    Print("\\============================================/");
                }
                else if(cmd == "clear" || cmd == "cls" || cmd == "cl")
                {
                    if(!running || client == null)
                    {
                        Print("RPC Not connected!");
                        continue;
                    }

                    client.ClearPresence();
                    Print("Presence Cleared.");
                }
                else Print("Invalid Command!");
            }
        }
    }
}
