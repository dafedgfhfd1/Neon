using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EventGameWinForms
{
    public partial class Form1 : Form
    {
        // === НАСТРОЙКИ ИГРЫ ===
        private const int PlayerSpeed = 6;
        private const int FallSpeed = 4;
        private const double SpawnRateSec = 1.5;

        // === СОСТОЯНИЕ ===
        private bool isPlaying = false;
        private int score = 0;
        private DateTime lastSpawnTime;

        // === ОБЪЕКТЫ ===
        private Rectangle playerRect;
        private List<GameItem> items = new List<GameItem>();

        // === УПРАВЛЕНИЕ ===
        private bool moveLeft = false;
        private bool moveRight = false;

        // === ТАЙМЕР ===
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();

        // Класс для хранения данных об объекте (бонус или враг)
        private class GameItem
        {
            public Rectangle Rect;
            public bool IsBonus;
        }

        public Form1()
        {
            InitializeComponent();

            // Настройка формы
            this.Text = "Event Game";
            this.Size = new Size(800, 650);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.BackColor = Color.White; 

            // Инициализация игрока
            playerRect = new Rectangle(375, 500, 50, 50);
            lastSpawnTime = DateTime.Now;

            // Настройка таймера
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameLoop;

            // Подписка на события
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.MouseDown += Form1_MouseDown;

            gameTimer.Start();
        }

        // === ИГРОВОЙ ЦИКЛ ===
        private void GameLoop(object? sender, EventArgs e)
        {
            if (!isPlaying) return;

            // Движение игрока
            if (moveLeft) playerRect.X -= PlayerSpeed;
            if (moveRight) playerRect.X += PlayerSpeed;

            // Ограничение границ экрана
            if (playerRect.X < 0) playerRect.X = 0;
            if (playerRect.Right > this.ClientSize.Width)
                playerRect.X = this.ClientSize.Width - playerRect.Width;

            // Спавн объектов
            double secondsSinceLastSpawn = (DateTime.Now - lastSpawnTime).TotalSeconds;
            if (secondsSinceLastSpawn >= SpawnRateSec)
            {
                SpawnObject();
                lastSpawnTime = DateTime.Now;
            }

            // Движение и удаление упавших объектов
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                item.Rect.Y += FallSpeed;
                if (item.Rect.Y > this.ClientSize.Height)
                    items.RemoveAt(i);
            }

            // Проверка столкновений
            CheckCollisions();

            // Перерисовка экрана
            this.Invalidate();
        }

        private void SpawnObject()
        {
            bool isBonus = random.NextDouble() < 0.7; // 70% бонус
            int x = random.Next(0, this.ClientSize.Width - 40);

            GameItem newItem = new GameItem
            {
                Rect = new Rectangle(x, -40, 40, 40),
                IsBonus = isBonus
            };
            items.Add(newItem);
        }

        private void CheckCollisions()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (playerRect.IntersectsWith(item.Rect))
                {
                    if (item.IsBonus)
                    {
                        score += 10;
                    }
                    else
                    {
                        EndGame();
                        return;
                    }
                    items.RemoveAt(i);
                }
            }
        }

        // === ОТРИСОВКА ===
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (!isPlaying)
            {
                // Рисуем меню
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                
                g.DrawString("НАЧАТЬ ИГРУ", new Font("Arial", 48, FontStyle.Bold),
                    Brushes.Black,
                    new RectangleF(0, this.Height * 0.35f, this.Width, 100),
                    sf);

              
                g.DrawString("(Кликни мышкой)", new Font("Arial", 18),
                    Brushes.DimGray,
                    new RectangleF(0, this.Height * 0.35f + 70, this.Width, 50),
                    sf);
                return;
            }

            // Рисуем игрока (синий квадрат)
            g.FillRectangle(Brushes.DodgerBlue, playerRect);
            g.DrawRectangle(Pens.Black, playerRect); // Черная обводка

            // Рисуем объекты
            foreach (var item in items)
            {
                if (item.IsBonus)
                    g.FillEllipse(Brushes.LimeGreen, item.Rect);
                else
                    g.FillRectangle(Brushes.Crimson, item.Rect);
            }

            // Рисуем счет
            g.DrawString($"Score: {score}", new Font("Consolas", 24, FontStyle.Bold),
                Brushes.Black, new Point(20, 20));
        }

        // === УПРАВЛЕНИЕ ===
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) moveLeft = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) moveRight = true;
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) moveLeft = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) moveRight = false;
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (!isPlaying) StartGame();
        }

        // === ЛОГИКА ИГРЫ ===
        private void StartGame()
        {
            score = 0;
            isPlaying = true;
            items.Clear();
            playerRect.X = this.ClientSize.Width / 2 - 25;
            lastSpawnTime = DateTime.Now;
        }

        private void EndGame()
        {
            isPlaying = false;
            MessageBox.Show($"Игра окончена!\nТвой счет: {score}", "Game Over",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}