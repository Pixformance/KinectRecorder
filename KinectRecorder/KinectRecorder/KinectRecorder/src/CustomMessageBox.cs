using System;
using System.Drawing;
using System.Windows.Forms;

namespace KinectRecorder.src
{
    public class CustomMessageBox : Form
    {
        private Label _message = new Label();
        private Button button = new Button();

        public CustomMessageBox()
        {
        }

        public CustomMessageBox(string title, string body)
        {
            ClientSize = new Size(400, 200);
            ControlBox = false;
            Text = title;

            button.FlatStyle = FlatStyle.Popup;
            button.Location = new Point(170, 150);
            button.Size = new Size(70, 25);
            button.Text = "Cancel";
            button.BackColor = Color.LightGray;

            _message.Location = new Point(10, 10);
            _message.Text = body;
            _message.Font = DefaultFont;
            _message.AutoSize = true;

            BackColor = Color.White;
            ShowIcon = false;

            Controls.Add(button);
            Controls.Add(_message);
        }

        public void UpdateMessage(string msg)
        {
            _message.Text += msg;
        }

        public void ResetMessage()
        {
            _message.Text = "Capture In Progress\n";
        }

        public void UpdateButton(string buttonMessage, EventHandler clickHandler)
        {
            button.Text = buttonMessage;

            button.Click += clickHandler;
        }

        public void UnsubscribeButtonEvent(EventHandler clickHandler)
        {
            button.Click -= clickHandler;
        }
    }
}
