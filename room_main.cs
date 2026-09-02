using GDF;
using Urho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GDF_example_mplay
{
    public class room_main : RoomTCP
    {
        private DataBase dataBase;
        private TextInput2d playerName, hostName, joinIpInput;

        public room_main() : base() { }

        protected override void Start()
        {
            this.RemoveGrid();
            this.BackgroundColor = Color.FromHex("2A2E3D"); // Dark modern theme

            // Fetch local IPv4 address of this machine
            string localIPv4 = GetLocalIPv4Address();

            // Initialize database settings
            this.dataBase = new DataBase("settings");
            if (dataBase.ExistsOnHD())
            {
                this.dataBase.Load();
            }
            else
            {
                this.dataBase.SetString("playerName", "Player");
                this.dataBase.SetString("hostName", "IPv4 LAN Game");
                this.dataBase.SetString("joinIp", localIPv4);
            }

            this.createUI(localIPv4);
        }

        private void createUI(string localIPv4)
        {
            // Title Header
            this.Text2dCreate("title", "IPv4 LAN MULTIPLAYER", 16, 0.5f, 26, true, Enums.Depth._13_Closer);

            // --- SECTION 1: PLAYER PROFILE ---
            Rectangle2d profileBg = this.Rectangle2dCreate("profileBg", 4.0f, 3.2f, 24, 3.5f, Enums.Depth._1_Background);
            profileBg.FillColor = Color.FromHex("3A3F53");

            this.Text2dCreate("lblPlayer", "Player Name:", 12, 3.8f, 16, true, Enums.Depth._13_Closer);
            playerName = this.TextInput2dCreate("nameInput", 14, 3.8f, 13, 2);
            playerName.FillColor = Color.FromHex("4E5571");
            playerName.GetText2d().SetText(this.dataBase.GetString("playerName"));

            // --- SECTION 2: HOST SERVER CARD ---
            Rectangle2d hostBg = this.Rectangle2dCreate("hostBg", 4.0f, 7.5f, 24, 5.0f, Enums.Depth._1_Background);
            hostBg.FillColor = Color.FromHex("3A3F53");

            this.Text2dCreate("lblHostHeader", "HOST A GAME", 10, 8.0f, 18, true, Enums.Depth._13_Closer);

            this.Text2dCreate("lblHost", "Server Name:", 12, 9.2f, 16, true, Enums.Depth._13_Closer);
            hostName = this.TextInput2dCreate("hostNameInput", 14, 9.2f, 13, 2);
            hostName.FillColor = Color.FromHex("4E5571");
            hostName.GetText2d().SetText(this.dataBase.GetString("hostName"));

            Button2d hostBtn = this.Button2dCreate("hostBtn", "button.png", 22, 9.2f, 5, 2);
            hostBtn.SetColor(Color.FromHex("4F80FF"));
            this.Text2dCreate("txtHostBtn", "HOST", 22.8f, 9.6f, 18, true, Enums.Depth._13_Closer);

            this.Text2dCreate("lblYourIp", "Your IPv4: " + localIPv4, 12, 11.2f, 14, true, Enums.Depth._13_Closer);

            // --- SECTION 3: JOIN SERVER CARD ---
            Rectangle2d joinBg = this.Rectangle2dCreate("joinBg", 4.0f, 13.2f, 24, 6.0f, Enums.Depth._1_Background);
            joinBg.FillColor = Color.FromHex("3A3F53");

            this.Text2dCreate("lblJoinHeader", "JOIN VIA IPv4", 10, 13.7f, 18, true, Enums.Depth._13_Closer);

            this.Text2dCreate("lblIp", "Host IPv4:", 12, 14.8f, 16, true, Enums.Depth._13_Closer);
            joinIpInput = this.TextInput2dCreate("joinIpInput", 14, 14.8f, 13, 2);
            joinIpInput.FillColor = Color.FromHex("4E5571");
            joinIpInput.GetText2d().SetText("192.168.1.20");

            // Join IPv4 Action Button
            Button2d joinBtn = this.Button2dCreate("joinBtn", "button.png", 22, 14.8f, 5, 2);
            joinBtn.SetColor(Color.FromHex("28A745"));
            this.Text2dCreate("txtJoinBtn", "JOIN", 22.9f, 15.2f, 18, true, Enums.Depth._13_Closer);

            // Quick Localhost Button
            Button2d localBtn = this.Button2dCreate("localBtn", "button.png", 14, 17.2f, 13, 1.5f);
            localBtn.SetColor(Color.FromHex("6C757D"));
            this.Text2dCreate("txtLocalBtn", "Use Localhost (127.0.0.1)", 15.2f, 17.5f, 14, true, Enums.Depth._13_Closer);
        }

        protected override void Step()
        {
            if (GetKeyPress(Key.Esc))
            {
                Game.Exit();
            }
            this.Cursor.Visible = true;

            // Auto-save input modifications
            if (this.playerName.GetInput() != this.dataBase.GetString("playerName"))
            {
                this.dataBase.SetString("playerName", this.playerName.GetInput());
                this.dataBase.Save();
            }
            if (this.hostName.GetInput() != this.dataBase.GetString("hostName"))
            {
                this.dataBase.SetString("hostName", this.hostName.GetInput());
                this.dataBase.Save();
            }
            if (this.joinIpInput.GetInput() != this.dataBase.GetString("joinIp"))
            {
                this.dataBase.SetString("joinIp", this.joinIpInput.GetInput());
                this.dataBase.Save();
            }

            // Quick set localhost
            if (this.GetButton2d("localBtn").IsClicked())
            {
                this.joinIpInput.GetText2d().SetText("127.0.0.1");
                this.dataBase.SetString("joinIp", "127.0.0.1");
                this.dataBase.Save();
            }

            // Start Host
            if (this.GetButton2d("hostBtn").IsClicked())
            {
                this.dataBase.SetBool("host", true);
                this.dataBase.Save();

                Game.GotoRoom(typeof(room_lobby));
            }

            // Start Client
            if (this.GetButton2d("joinBtn").IsClicked())
            {
                // Force strip any potential accidental formatting spaces
                string formattedIp = this.joinIpInput.GetInput().Trim();

                this.dataBase.SetBool("host", false);
                this.dataBase.SetString("joinIp", formattedIp);
                this.dataBase.Save();

                Game.GotoRoom(typeof(room_lobby));
            }
        }

        /// <summary>
        /// Helper function to automatically retrieve the active IPv4 address of the host machine.
        /// </summary>
        private string GetLocalIPv4Address()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch
            {
                return "127.0.0.1"; // Fallback to loopback IPv4
            }
        }
    }
}