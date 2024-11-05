using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private static readonly int AppBarHeight = 40;

        private Label folderLabel;
        private Button selectFolderButton;
        private Button backButton; // �s�W��^���s
        private Panel folderPanel;

        // �Ψ��x�s��Ƨ����v�����|
        private Stack<string> folderHistory = new Stack<string>();

        // Import the SetWindowRegion API
        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public MainScreen()
        {
            InitializeComponent();
            this.Text = "The Sims 4 Mod Manager";
            this.FormBorderStyle = FormBorderStyle.None;
            this.Height = 768;
            this.Width = 1366;
            this.BackColor = FormBackColor;
            InitializeUI();
            SetRoundedCorners();
        }

        private void InitializeUI()
        {
            // ����l�� folderLabel
            folderLabel = new Label
            {
                Text = "No Select",
                AutoSize = true,
                Location = new Point(20, 708),
                Font = new Font(FontFamily.GenericSansSerif, 10),
                ForeColor = Color.White
            };

            // �A��l�ƿ�ܸ�Ƨ������s
            selectFolderButton = new Button
            {
                Text = "Select Folder",
                Location = new Point(20, 728),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 10),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = AppBarColor
            };
            selectFolderButton.FlatAppearance.BorderSize = 0;
            selectFolderButton.Click += SelectFolderButton_Click;

            // �s�W��^���s
            backButton = new Button
            {
                Text = "Back",
                Location = new Point(150, 728),
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 10),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = AppBarColor
            };
            backButton.FlatAppearance.BorderSize = 0;
            backButton.Click += BackButton_Click; // �]�m��^���s���I���ƥ�

            // �إߨó]�m appBar
            Panel appBar = new Panel
            {
                Dock = DockStyle.Top,
                BackColor = AppBarColor,
                Height = AppBarHeight
            };
            appBar.MouseDown += AppBar_MouseDown;
            appBar.MouseMove += AppBar_MouseMove;
            appBar.MouseUp += AppBar_MouseUp;

            Button exitBtn = new Button
            {
                FlatStyle = FlatStyle.Flat,
                ForeColor = AppBarBtnTextColor,
                Height = AppBarHeight,
                Width = AppBarHeight,
                Text = "X",
                Dock = DockStyle.Right,
                Font = new Font(FontFamily.GenericSansSerif, 10)
            };
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.Click += ExitBtn_Click;
            appBar.Controls.Add(exitBtn);

            this.Controls.Add(appBar);

            // �Ψ���ܸ�Ƨ��M�ɮת����O
            folderPanel = new Panel
            {
                Location = new Point(20, 60),
                AutoScroll = true,
                Size = new Size(1200, 500),
                BackColor = Color.FromArgb(50, 50, 50)
            };

            // �[�J����ܪ��
            this.Controls.Add(folderLabel);
            this.Controls.Add(selectFolderButton);
            this.Controls.Add(backButton); // �[�J��^���s
            this.Controls.Add(folderPanel);
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    folderLabel.Text = selectedFolder;

                    // �M�ž��v����ܿ�ܪ���Ƨ�
                    folderHistory.Clear(); // �M�����v���|
                    DisplayFolderContents(selectedFolder); // ��ܿ�ܪ���Ƨ����e
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (folderHistory.Count > 1) // �T�O���ܤ֨�Ӹ�Ƨ�
            {
                folderHistory.Pop(); // ������e��Ƨ�
                string previousFolder = folderHistory.Pop(); // ����W�Ÿ�Ƨ�
                DisplayFolderContents(previousFolder); // ��ܤW�Ÿ�Ƨ����e
            }
        }

        private void DisplayFolderContents(string folderPath)
        {
            // �M�Ť��e�����e
            folderPanel.Controls.Clear();
            currentYPosition = 0; // ���m Y �b��m

            // ����Ӹ�Ƨ����Ҧ��l��Ƨ�
            var directories = Directory.GetDirectories(folderPath);
            foreach (var directory in directories)
            {
                // �Ы� Label ����ܸ�Ƨ��W��
                Label folderLabel = new Label
                {
                    Text = Path.GetFileName(directory),
                    AutoSize = true,
                    Location = new Point(0, currentYPosition),
                    ForeColor = Color.White,
                    Font = new Font(FontFamily.GenericSansSerif, 10),
                    Tag = directory // �N���|�s�x�b Tag �ݩʤ��A��K����ϥ�
                };

                folderLabel.Click += FolderLabel_Click; // �I���ƥ�A�ΨӮi�}�l��Ƨ����e

                // �N Label �[�J�� folderPanel
                folderPanel.Controls.Add(folderLabel);
                currentYPosition += folderLabel.Height + 5; // ��s Y �b��m
            }

            // ����Ӹ�Ƨ����Ҧ��ɮ�
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                // �Ы� Label ������ɮצW��
                Label fileLabel = new Label
                {
                    Text = Path.GetFileName(file),
                    AutoSize = true,
                    Location = new Point(0, currentYPosition),
                    ForeColor = Color.LightGray,
                    Font = new Font(FontFamily.GenericSansSerif, 10)
                };

                // �N Label �[�J�� folderPanel
                folderPanel.Controls.Add(fileLabel);
                currentYPosition += fileLabel.Height + 5; // ��s Y �b��m
            }

            // �N��e��Ƨ����J���v���|
            if (folderHistory.Count == 0 || folderHistory.Peek() != folderPath) // �T�O������
            {
                folderHistory.Push(folderPath); // �N��e��Ƨ����J���v
            }
        }

        private void FolderLabel_Click(object sender, EventArgs e)
        {
            // ���I����Ƨ� Label �ɡA�������|����ܤl��Ƨ����e
            Label clickedLabel = sender as Label;
            if (clickedLabel != null)
            {
                string folderPath = clickedLabel.Tag.ToString(); // �����Ƨ����|
                DisplayFolderContents(folderPath); // �i�ܤl��Ƨ����e
            }
        }

        private void SetRoundedCorners()
        {
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
                beginMove = false;
            }
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
