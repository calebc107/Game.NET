using System;
using System.Windows.Forms;

namespace GameNET
{
    /// <summary>
    /// Game window interface
    /// </summary>

    public class MainWindow : Form
    {
        /// <summary>
        /// Show window
        /// </summary>
        /// <param name="renderMethod">Method to run on every frame/window update</param>
        public void Run(Action renderMethod)
        {
            Show();
            while (!Engine.abort)
            {
                Application.DoEvents();
                renderMethod();
            }
        }

        /// <summary>
        /// Game window interface
        /// </summary>
        /// <param name="xres">X resolution</param>
        /// <param name="yres">Y resolution</param>
        public MainWindow(int xres, int yres) : base()
        {

            MouseEnter += Screen_MouseEnter;
            MouseLeave += Screen_MouseLeave;
            MouseMove += Screen_MouseMove;
            MouseDown += Screen_MouseDown;
            MouseUp += Screen_MouseUp;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            ResizeEnd += delegate {
                //Engine.Resize(ClientSize.Width, ClientSize.Height, (int)Engine.fps, false);
            };
            SetClientSizeCore(xres, yres);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
        }

        /// <summary>
        /// Stop game engine is window is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            Engine.Stop();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// set cursor properties on input events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Screen_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Engine.mouse.lClick = false;
            if (e.Button == MouseButtons.Right)
                Engine.mouse.rClick = false;
        }

        private void Screen_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Engine.mouse.lClick = true;
            if (e.Button == MouseButtons.Right)
                Engine.mouse.rClick = true;
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            Engine.mouse.location = new Point(e.X, e.Y);
        }

        private void Screen_MouseLeave(object sender, EventArgs e)
        {
            System.Windows.Forms.Cursor.Show();
        }

        private void Screen_MouseEnter(object sender, EventArgs e)
        {
            if (Engine.mouse.cursorIcon != null)
                System.Windows.Forms.Cursor.Hide();
        }

        /// <summary>
        /// Displays a dialog with the specefied message
        /// </summary>
        /// <param name="message">the message to show</param>
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            var keycode = Input.GetControlCode((int)e.KeyCode);
            if (e.KeyCode == Keys.Escape)
                if (!Engine.paused)
                    Engine.Pause();
                else
                    Engine.Unpause();
            if (!Engine.keys.Contains(keycode))
                Engine.keys.Add(keycode);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            var keycode = Input.GetControlCode((int)e.KeyCode);
            if (Engine.keys.Contains(keycode))
                Engine.keys.Remove(keycode);
        }
    }
}
