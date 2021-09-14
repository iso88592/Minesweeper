using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Minesweeper.Properties;

namespace Minesweeper
{
    public partial class MainForm : Form
    {
        private readonly int[] _fieldWidths =
        {
            10, 16, 30
        };

        private readonly int[] _fieldHeights =
        {
            10, 16, 16
        };

        private readonly int[] _mineCounts =
        {
            10, 40, 99
        };

        private int _fieldWidth = 30;
        private int _fieldHeight = 16;
        private int _mineCount = 99;
        private const int ButtonSize = 24;

        private int[,] _field;
        private Button[,] _buttons;
        private Label[] _mineLabels;
        private Label[] _timeLabels;

        private bool _gameStarted;
        private bool _gameOver;
        private int _currentMines;

        private Timer _timer;
        private DateTime _startDateTime;

        private Random _random;

        private void InitGame()
        {
            _gameOver = false;
            _random = new Random();
        }

        public MainForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            InitGame();
            ExtractImages();
            GenerateLabels();
            GenerateButtons();
        }

        private Label CreateLabel(int i)
        {
            return new Label
            {
                FlatStyle = FlatStyle.Flat,
                ImageList = numbers,
                ImageIndex = 10,
                Location = new Point(i * 24, 0),
                Size = new Size(24, 46)
            };
        }

        private void GenerateLabels()
        {
            _mineLabels = new Label[3];
            _timeLabels = new Label[3];
            for (int i = 0; i < 3; i++)
            {
                minePanel.Controls.Add(_mineLabels[i] = CreateLabel(i));
                timePanel.Controls.Add(_timeLabels[i] = CreateLabel(i));
            }
        }

        private void ExtractImages()
        {
            Bitmap image = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Minesweeper.mines.png"));
            for (int i = 0; i < 14; i++)
            {
                Bitmap bitmap = new Bitmap(24, 24);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, new Point(0, -i * 24));
                imageList.Images.Add(bitmap);
            }

            image = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Minesweeper.numbers.png"));
            for (int i = 0; i < 12; i++)
            {
                Bitmap bitmap = new Bitmap(24, 46);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.ScaleTransform(2, 2);
                graphics.DrawImage(image, new Point(-i * 12, 0));
                numbers.Images.Add(bitmap);
            }
        }

        private void EachField(Action<int, int> action)
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    action.Invoke(i, j);
                }
            }
        }

        private void NewGame()
        {
            _timer.Stop();
            EachField((i, j) => gameField.Controls.Remove(_buttons[i, j]));

            _gameOver = false;
            _gameStarted = false;
        }

        private void ButtonRightClick(Button button, int i, int j)
        {
            if (_gameOver) return;
            if (button.ImageIndex < 11) return;
            if (button.ImageIndex == 12)
            {
                _currentMines--;
                button.ImageIndex = 11;
                UpdateMineCount();
            }
            else
            {
                _currentMines++;
                button.ImageIndex = 12;
                UpdateMineCount();
            }
        }

        private void ButtonMiddleClick(Button button, int i, int j)
        {
            if (button.ImageIndex >= 9) return;
            int flagCount = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (!CheckCoords(x+i, y+j)) continue;
                    if (_buttons[x + i, y + j].ImageIndex == 11)
                    {
                        flagCount++;
                    }
                } 
            }

            if (flagCount != _field[i, j]) return;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    if (!CheckCoords(x+i, y+j)) continue;
                    if (_buttons[x + i, y + j].ImageIndex == 12)
                    {
                        ButtonClick(_buttons[x+i,y+j], x+i, y+j);
                    }
                } 
            }
            
        }

        private void ButtonClick(Button button, int i, int j)
        {
            if (_gameOver) return;
            if (button.ImageIndex != 12) return;
            if (!_gameStarted)
            {
                StartGame(i, j);
            }

            if (_field[i, j] == 9)
            {
                _gameOver = true;
                RevealMap();
                _buttons[i, j].ImageIndex = 10;
            }
            else
            {
                Reveal(i, j);
                CheckVictory();
            }

            if (_field[i, j] == 0)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        if (!CheckCoords(x + i, y + j)) continue;
                        ButtonClick(_buttons[x + i, y + j], x + i, y + j);
                    }
                }
            }
        }

        private void CheckVictory()
        {
            int count = _buttons.Cast<Button>().Count(button => button.ImageIndex < 9);

            if (count == _fieldHeight * _fieldWidth - _mineCount)
            {
                _gameOver = true;
                int winTime = DateTime.Now.Subtract(_startDateTime).Seconds;
                MessageBox.Show($"You won. Your time is {winTime} seconds.");
            }
        }

        private bool CheckCoords(int x, int y)
        {
            if (x < 0) return false;
            if (x >= _fieldWidth) return false;
            if (y < 0) return false;
            if (y >= _fieldHeight) return false;
            return true;
        }

        private void EmptyField()
        {
            EachField((i, j) => { _field[i, j] = 0;});
        }

        private void Reveal(int x, int y)
        {
            if (_field[x, y] == 9)
            {
                if (_buttons[x, y].ImageIndex != 11) _buttons[x, y].ImageIndex = 9;
            }
            else
            {
                _buttons[x, y].ImageIndex = _buttons[x, y].ImageIndex == 11 ? 13 : _field[x, y];
            }
        }

        private void RevealMap()
        {
            EachField(Reveal);
        }

        private int CountMines(int x, int y)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (!CheckCoords(i + x, j + y)) continue;
                    if (_field[i + x, j + y] == 9) count++;
                }
            }
            return count;
        }

        private void FillField()
        {
            EachField((i, j) =>
            {
                if (_field[i, j] != 9)
                {
                    _field[i, j] = CountMines(i, j);
                }
            });
        }

        private void StartGame(int x, int y)
        {
            EmptyField();
            List<int> occupied = new List<int> {x + y * _fieldWidth};
            for (int i = 0; i < _mineCount; i++)
            {
                int newIndex;
                do
                {
                    newIndex = _random.Next(_fieldWidth * _fieldHeight);
                } while (occupied.Contains(newIndex));

                occupied.Add(newIndex);
                _field[newIndex % _fieldWidth, newIndex / _fieldWidth] = 9;
            }

            FillField();
            _gameStarted = true;
            _timer.Start();
            _startDateTime = DateTime.Now;
        }

        void UpdateMineCount()
        {
            int c = _mineCount + _currentMines;
            if (c < 0)
            {
                _mineLabels[0].ImageIndex = 10;
                c = Math.Abs(c);
                if (c > 99) c = 99;
            }
            else
            {
                _mineLabels[0].ImageIndex = c / 100 % 10;
            }

            _mineLabels[1].ImageIndex = c / 10 % 10;
            _mineLabels[2].ImageIndex = c / 1 % 10;
        }

        private void GenerateButtons()
        {
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += delegate(object sender, EventArgs args)
            {
                if (_gameOver) _timer.Stop();
                int c = DateTime.Now.Subtract(_startDateTime).Seconds;
                if (c > 999) c = 999;
                _timeLabels[0].ImageIndex = c / 100 % 10;
                _timeLabels[1].ImageIndex = c / 10 % 10;
                _timeLabels[2].ImageIndex = c / 1 % 10;
            };
            _timeLabels[0].ImageIndex = 10;
            _timeLabels[1].ImageIndex = 10;
            _timeLabels[2].ImageIndex = 10;
            _currentMines = 0;
            _gameStarted = false;
            UpdateMineCount();
            Size = new Size(44 + _fieldWidth * ButtonSize, 146 + _fieldHeight * ButtonSize);
            _field = new int[_fieldWidth, _fieldHeight];
            _buttons = new Button[_fieldWidth, _fieldHeight];
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    _field[i, j] = 0;
                    var x = i;
                    var y = j;

                    _buttons[i, j] = new Button
                    {
                        Size = new Size(ButtonSize, ButtonSize),
                        Location = new Point(i * ButtonSize, j * ButtonSize),
                        FlatStyle = FlatStyle.Flat,
                        FlatAppearance = { BorderSize = 0},
                        ImageList = imageList,
                        ImageIndex = 12,
                    };
                    
                    _buttons[i, j].MouseUp += delegate(object sender, MouseEventArgs args)
                    {
                        if (args.Button == MouseButtons.Right)
                        {
                            ButtonRightClick(_buttons[x, y], x, y);
                        }

                        if (args.Button == MouseButtons.Middle)
                        {
                            ButtonMiddleClick(_buttons[x, y], x, y);
                        }
                    };
                    _buttons[i, j].Click += delegate(Object target, EventArgs eventAgrs)
                    {
                        ButtonClick(_buttons[x, y], x, y);
                    };
                    gameField.Controls.Add(_buttons[i, j]);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NewGame();
            GenerateButtons();
        }

        private void Setup(int difficulty)
        {
            NewGame();
            _fieldHeight = _fieldHeights[difficulty];
            _fieldWidth = _fieldWidths[difficulty];
            _mineCount = _mineCounts[difficulty];
            GenerateButtons();
        }

        private void ClearDifficultySelection()
        {
            easyToolStripMenuItem.Checked = false;
            mToolStripMenuItem.Checked = false;
            masterToolStripMenuItem.Checked = false;
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(0);
            ClearDifficultySelection();
            easyToolStripMenuItem.Checked = true;
        }

        private void mToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(1);
            ClearDifficultySelection();
            mToolStripMenuItem.Checked = true;
        }

        private void masterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(2);
            ClearDifficultySelection();
            masterToolStripMenuItem.Checked = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}