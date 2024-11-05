using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace The_Sim_4_Mod_Manager
{
    public partial class MainScreen : Form
    {
        private bool beginMove = false;
        private int currentXPosition;
        private int currentYPosition;

        private const int RoundCornerRadius = 15;
        private static readonly Color AppBarColor = Color.FromArgb(41, 44, 50);
        private static readonly Color AppBarBtnTextColor = Color.LightGray;
        private static readonly Color FormBackColor = Color.FromArgb(46, 49, 53);
        private static readonly int AppBarHeight = 42;


        // Import the SetWindowRegion API
        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        // Import the CreateRoundRectRgn API
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);


        public MainScreen()
        {
            InitializeComponent();
            this.Text = "The Sims 4 Mod Manager";
            this.FormBorderStyle = FormBorderStyle.None;
            this.Height = 720;
            this.Width = 1080;
            this.BackColor = FormBackColor;
            InitializeUI();
            SetRoundedCorners();
        }

        private void InitializeUI()
        {
            Panel appBar = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = AppBarColor,
                Height = AppBarHeight
            };
            appBar.MouseDown += AppBar_MouseDown!;
            appBar.MouseMove += AppBar_MouseMove!;
            appBar.MouseUp += AppBar_MouseUp!;


            Button exitBtn = new Button
            {
                FlatStyle = FlatStyle.Flat,
                ForeColor = AppBarBtnTextColor,
                Height = AppBarHeight,
                Width = AppBarHeight,
                Text = "X",
                Dock = DockStyle.Right,
            };
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.Font = new Font(exitBtn.Font.FontFamily, 10);
            exitBtn.Click += ExitBtn_Click!;


            appBar.Controls.Add(exitBtn);

            this.Controls.Add(appBar);
        }

        private void SetRoundedCorners()
        {
            // Create a rounded rectangle region
            IntPtr rgn = CreateRoundRectRgn(0, 0, this.Width, this.Height, RoundCornerRadius, RoundCornerRadius);
            SetWindowRgn(this.Handle, rgn, true);
        }

        private void AppBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                beginMove = true;
                currentXPosition = e.X;
                currentYPosition = e.Y;
            }
        }

        private void AppBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (beginMove)
            {
                this.Left += e.X - currentXPosition;
                this.Top += e.Y - currentYPosition;
            }
        }

        private void AppBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                currentXPosition = 0;
                currentYPosition = 0;
                beginMove = false;
            }
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

}
