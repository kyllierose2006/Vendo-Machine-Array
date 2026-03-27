using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetroVendingMachine
{
    // ==========================================
    // 1. THE ENTRY POINT (Main Method)
    // ==========================================
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // This line launches our specific form
            Application.Run(new RetroForm());
        }
    }

    // ==========================================
    // 2. THE DATA STRUCTURE
    // ==========================================
    public class Product
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public Product(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }
    }

    // ==========================================
    // 3. THE FORM (UI & Logic)
    // ==========================================
    public partial class RetroForm : Form
    {
        // 2D Array holding our data
        private Product[,] inventory = new Product[4, 4];

        // Player state
        private decimal currentCredit = 5.00m;

        // Retro Color Palette
        private Color colorBgDark = Color.FromArgb(20, 20, 30);    // Deep dark blue/black
        private Color colorNeonPink = Color.FromArgb(255, 0, 255);  // Magenta
        private Color colorNeonCyan = Color.FromArgb(0, 255, 255);  // Cyan
        private Color colorTerminalGreen = Color.FromArgb(57, 255, 20); // Matrix Green

        // Fonts & Controls
        private Font retroFont;
        private Font ledFont;
        private TableLayoutPanel gridPanel;
        private Label lblledDisplay;
        private Label lblCredit;

        public RetroForm()
        {
            // Form Setup
            this.Text = "N E O N - V E N D // SYSTEM ONLINE";
            this.BackColor = colorBgDark;
            this.Size = new Size(900, 650);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize fonts
            retroFont = new Font("Consolas", 9F, FontStyle.Bold);
            ledFont = new Font("Courier New", 14F, FontStyle.Bold);

            InitializeData();
            BuildRetroUI();
            UpdateLEDDisplay("INSERT COIN... OR SELECT ITEM");
            UpdateCreditDisplay();
        }

        private void InitializeData()
        {
            // Row 0: Drinks
            inventory[0, 0] = new Product("Synth-Cola", 1.50m, 5);
            inventory[0, 1] = new Product("Neon Gator", 1.75m, 4);
            inventory[0, 2] = new Product("Pixel H2O", 1.00m, 8);
            inventory[0, 3] = new Product("Cyber-Energy", 2.50m, 2);

            // Row 1: Chips
            inventory[1, 0] = new Product("Glitch Chips", 1.25m, 3);
            inventory[1, 1] = new Product("Bit-O-Rings", 1.25m, 5);
            inventory[1, 2] = new Product("Laser Doritos", 1.50m, 0); // Out of stock
            inventory[1, 3] = new Product("Data Pretzels", 1.00m, 6);

            // Row 2: Sweets
            inventory[2, 0] = new Product("Nano-Gummies", 0.75m, 10);
            inventory[2, 1] = new Product("Binary Bar", 1.00m, 7);
            inventory[2, 2] = new Product("Plasma Pop", 0.50m, 12);
            inventory[2, 3] = new Product("Void Mints", 0.75m, 4);

            // Row 3: Misc
            inventory[3, 0] = new Product("RAM Stick", 5.00m, 1);
            inventory[3, 1] = new Product("Floppy Disk", 3.50m, 2);
            inventory[3, 2] = new Product("Cassette", 4.00m, 3);
            inventory[3, 3] = new Product("Vapor Sticker", 0.25m, 20);
        }

        private void BuildRetroUI()
        {
            // Main Container
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.BackColor = colorBgDark;
            splitContainer.SplitterWidth = 1;
            splitContainer.IsSplitterFixed = true;
            splitContainer.FixedPanel = FixedPanel.Panel2;
            splitContainer.Panel1.Padding = new Padding(20);
            splitContainer.Panel2.Padding = new Padding(20);
            this.Controls.Add(splitContainer);
            splitContainer.SplitterDistance = 600;

            // LEFT SIDE: Grid Window
            Panel windowFrame = new Panel();
            windowFrame.Dock = DockStyle.Fill;
            windowFrame.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, windowFrame.ClientRectangle,
                    colorNeonPink, 3, ButtonBorderStyle.Solid,
                    colorNeonPink, 3, ButtonBorderStyle.Solid,
                    colorNeonCyan, 3, ButtonBorderStyle.Solid,
                    colorNeonCyan, 3, ButtonBorderStyle.Solid);
            };
            windowFrame.Padding = new Padding(5);

            gridPanel = new TableLayoutPanel();
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.RowCount = 4;
            gridPanel.ColumnCount = 4;
            gridPanel.BackColor = Color.Black;
            for (int i = 0; i < 4; i++) gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            for (int i = 0; i < 4; i++) gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            windowFrame.Controls.Add(gridPanel);
            splitContainer.Panel1.Controls.Add(windowFrame);

            // RIGHT SIDE: Controls
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Fill;
            controlPanel.BackColor = Color.FromArgb(30, 30, 40);
            controlPanel.BorderStyle = BorderStyle.Fixed3D;
            splitContainer.Panel2.Controls.Add(controlPanel);

            // LED Display
            lblledDisplay = new Label();
            lblledDisplay.Location = new Point(20, 20);
            lblledDisplay.Size = new Size(220, 80);
            lblledDisplay.BackColor = Color.Black;
            lblledDisplay.ForeColor = colorTerminalGreen;
            lblledDisplay.Font = ledFont;
            lblledDisplay.Text = "INITIALIZING...";
            lblledDisplay.TextAlign = ContentAlignment.MiddleCenter;
            lblledDisplay.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, lblledDisplay.ClientRectangle, colorTerminalGreen, ButtonBorderStyle.Solid);
            controlPanel.Controls.Add(lblledDisplay);

            // Credit
            Label lblCreditTitle = CreateRetroLabel("CREDIT AVAILABLE:", 20, 120, colorNeonCyan);
            controlPanel.Controls.Add(lblCreditTitle);

            lblCredit = new Label();
            lblCredit.Location = new Point(20, 145);
            lblCredit.Size = new Size(220, 40);
            lblCredit.BackColor = Color.Black;
            lblCredit.ForeColor = colorNeonCyan;
            lblCredit.Font = ledFont;
            lblCredit.Text = "$0.00";
            lblCredit.TextAlign = ContentAlignment.MiddleRight;
            controlPanel.Controls.Add(lblCredit);

            // Insert Coin Button
            Button btnAddCoin = CreateRetroButton("INSERT $1.00", 20, 210, 220, 60, colorNeonPink);
            btnAddCoin.Click += (s, e) => {
                currentCredit += 1.00m;
                UpdateCreditDisplay();
                UpdateLEDDisplay("COIN ACCEPTED.");
                RenderGridButtons();
            };
            controlPanel.Controls.Add(btnAddCoin);

            // Instructions
            Label lblInstructions = CreateRetroLabel("CLICK ITEMS ON GRID\nTO DISPENSE >_", 20, 300, colorTerminalGreen);
            controlPanel.Controls.Add(lblInstructions);

            RenderGridButtons();
        }

        private Label CreateRetroLabel(string text, int x, int y, Color color)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.AutoSize = true;
            l.Font = retroFont;
            l.ForeColor = color;
            return l;
        }

        private Button CreateRetroButton(string text, int x, int y, int w, int h, Color accentColor)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 2;
            b.FlatAppearance.BorderColor = accentColor;
            b.BackColor = Color.Black;
            b.ForeColor = accentColor;
            b.Font = retroFont;
            b.Cursor = Cursors.Hand;
            b.MouseEnter += (s, e) => { b.BackColor = accentColor; b.ForeColor = Color.Black; };
            b.MouseLeave += (s, e) => { b.BackColor = Color.Black; b.ForeColor = accentColor; };
            return b;
        }

        private void RenderGridButtons()
        {
            gridPanel.Controls.Clear();

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Product p = inventory[r, c];
                    Button btn = new Button();
                    btn.Dock = DockStyle.Fill;
                    btn.Margin = new Padding(4);
                    btn.Tag = new Point(r, c);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.Font = retroFont;
                    btn.BackColor = Color.FromArgb(10, 10, 15);

                    if (p.Quantity == 0)
                    {
                        btn.Text = $"[X] {p.Name}\nOUT OF STOCK";
                        btn.ForeColor = Color.Gray;
                        btn.FlatAppearance.BorderColor = Color.DarkRed;
                        btn.Enabled = false;
                    }
                    else
                    {
                        string coordLabel = $"[{(char)('A' + r)}{c + 1}]";
                        btn.Text = $"{coordLabel}\n{p.Name}\n${p.Price:0.00} | Q:{p.Quantity}";

                        if (currentCredit >= p.Price)
                        {
                            btn.ForeColor = colorNeonCyan;
                            btn.FlatAppearance.BorderColor = colorNeonCyan;
                            btn.Cursor = Cursors.Hand;
                        }
                        else
                        {
                            btn.ForeColor = Color.FromArgb(150, 0, 150);
                            btn.FlatAppearance.BorderColor = Color.FromArgb(150, 0, 150);
                            btn.Cursor = Cursors.No;
                        }
                    }

                    btn.Click += ProductBtn_Click;
                    gridPanel.Controls.Add(btn, c, r);
                }
            }
        }

        private void ProductBtn_Click(object sender, EventArgs e)
        {
            Button clickedBtn = (Button)sender;
            Point location = (Point)clickedBtn.Tag;
            Product selectedProduct = inventory[location.X, location.Y];

            UpdateLEDDisplay($"SELECTING {selectedProduct.Name.ToUpper()}...");

            if (selectedProduct.Quantity <= 0)
            {
                UpdateLEDDisplay("ERROR: OUT OF STOCK.");
                return;
            }

            if (currentCredit < selectedProduct.Price)
            {
                UpdateLEDDisplay("ERROR: INSUFFICIENT FUNDS.");
                return;
            }

            selectedProduct.Quantity--;
            currentCredit -= selectedProduct.Price;

            UpdateLEDDisplay($"VENDING: {selectedProduct.Name}...");
            UpdateCreditDisplay();
            RenderGridButtons();
        }

        private void UpdateLEDDisplay(string message)
        {
            lblledDisplay.Text = message;
        }

        private void UpdateCreditDisplay()
        {
            lblCredit.Text = "$" + currentCredit.ToString("0.00");
        }
    }
}