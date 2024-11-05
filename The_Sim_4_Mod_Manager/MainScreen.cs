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
        private Button backButton; // 新增返回按鈕
        private Panel folderPanel;

        // 用來儲存資料夾歷史的堆疊
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
            // 先初始化 folderLabel
            folderLabel = new Label
            {
                Text = "No Select",
                AutoSize = true,
                Location = new Point(20, 708),
                Font = new Font(FontFamily.GenericSansSerif, 10),
                ForeColor = Color.White
            };

            // 再初始化選擇資料夾的按鈕
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

            // 新增返回按鈕
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
            backButton.Click += BackButton_Click; // 設置返回按鈕的點擊事件

            // 建立並設置 appBar
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

            // 用來顯示資料夾和檔案的面板
            folderPanel = new Panel
            {
                Location = new Point(20, 60),
                AutoScroll = true,
                Size = new Size(1200, 500),
                BackColor = Color.FromArgb(50, 50, 50)
            };

            // 加入控制項至表單
            this.Controls.Add(folderLabel);
            this.Controls.Add(selectFolderButton);
            this.Controls.Add(backButton); // 加入返回按鈕
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

                    // 清空歷史並顯示選擇的資料夾
                    folderHistory.Clear(); // 清除歷史堆疊
                    DisplayFolderContents(selectedFolder); // 顯示選擇的資料夾內容
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (folderHistory.Count > 1) // 確保有至少兩個資料夾
            {
                folderHistory.Pop(); // 移除當前資料夾
                string previousFolder = folderHistory.Pop(); // 獲取上級資料夾
                DisplayFolderContents(previousFolder); // 顯示上級資料夾內容
            }
        }

        private void DisplayFolderContents(string folderPath)
        {
            // 清空之前的內容
            folderPanel.Controls.Clear();
            currentYPosition = 0; // 重置 Y 軸位置

            // 獲取該資料夾內所有子資料夾
            var directories = Directory.GetDirectories(folderPath);
            foreach (var directory in directories)
            {
                // 創建 Label 來顯示資料夾名稱
                Label folderLabel = new Label
                {
                    Text = Path.GetFileName(directory),
                    AutoSize = true,
                    Location = new Point(0, currentYPosition),
                    ForeColor = Color.White,
                    Font = new Font(FontFamily.GenericSansSerif, 10),
                    Tag = directory // 將路徑存儲在 Tag 屬性中，方便後續使用
                };

                folderLabel.Click += FolderLabel_Click; // 點擊事件，用來展開子資料夾內容

                // 將 Label 加入到 folderPanel
                folderPanel.Controls.Add(folderLabel);
                currentYPosition += folderLabel.Height + 5; // 更新 Y 軸位置
            }

            // 獲取該資料夾內所有檔案
            var files = Directory.GetFiles(folderPath);
            foreach (var file in files)
            {
                // 創建 Label 來顯示檔案名稱
                Label fileLabel = new Label
                {
                    Text = Path.GetFileName(file),
                    AutoSize = true,
                    Location = new Point(0, currentYPosition),
                    ForeColor = Color.LightGray,
                    Font = new Font(FontFamily.GenericSansSerif, 10)
                };

                // 將 Label 加入到 folderPanel
                folderPanel.Controls.Add(fileLabel);
                currentYPosition += fileLabel.Height + 5; // 更新 Y 軸位置
            }

            // 將當前資料夾推入歷史堆疊
            if (folderHistory.Count == 0 || folderHistory.Peek() != folderPath) // 確保不重複
            {
                folderHistory.Push(folderPath); // 將當前資料夾推入歷史
            }
        }

        private void FolderLabel_Click(object sender, EventArgs e)
        {
            // 當點擊資料夾 Label 時，獲取其路徑並顯示子資料夾內容
            Label clickedLabel = sender as Label;
            if (clickedLabel != null)
            {
                string folderPath = clickedLabel.Tag.ToString(); // 獲取資料夾路徑
                DisplayFolderContents(folderPath); // 展示子資料夾內容
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
