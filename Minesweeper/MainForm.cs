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
        private int[] FieldWidths = {
            10, 16, 30
        };
        private int[] FieldHeights =
        {
            10, 16, 16
        };
        private int[] MineCounts =
        {
            10, 40, 99
        };

        private int _fieldWidth = 30;
        private int _fieldHeight = 16;
        private int _mineCount = 99;
        private int _buttonSize = 24;

        private int[,] _field;
        private Button[,] _buttons;
        private Label[] _mineButtons;
        private Label[] _timeButtons;

        private bool _gameStarted;
        private bool _gameOver;
        private int _currentMines;

        private Timer _timer;
        private DateTime _startDateTime;


        public MainForm()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            _gameOver = false;

            Bitmap image = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().
                    GetManifestResourceStream("Minesweeper.mines.png"));
            for (int i = 0; i < 14; i++)
            {
                Bitmap bitmap = new Bitmap(24,24);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(image, new Point(0, -i*24));
                imageList.Images.Add(bitmap);
            }

            image = new Bitmap(
                System.Reflection.Assembly.GetEntryAssembly().
                    GetManifestResourceStream("Minesweeper.numbers.png"));
            for (int i = 0; i < 12; i++)
            {
                Bitmap bitmap = new Bitmap(24,46);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.ScaleTransform(2,2);
                graphics.DrawImage(image, new Point(-i*12, 0));
                numbers.Images.Add(bitmap);
            }
            _mineButtons = new Label[3];
            _timeButtons = new Label[3];
            for (int i = 0; i < 3; i++)
            {
                _mineButtons[i] = new Label();
                _mineButtons[i].FlatStyle = FlatStyle.Flat;
                _mineButtons[i].ImageList = numbers;
                _mineButtons[i].ImageIndex = 10;
                _mineButtons[i].Location = new Point(i*24,0);
                _mineButtons[i].Size = new Size(24, 46);
                minePanel.Controls.Add(_mineButtons[i]);


                _timeButtons[i] = new Label();
                _timeButtons[i].FlatStyle = FlatStyle.Flat;
                _timeButtons[i].ImageList = numbers;
                _timeButtons[i].ImageIndex = 10;
                _timeButtons[i].Location = new Point(i*24,0);
                _timeButtons[i].Size = new Size(24, 46);
                timePanel.Controls.Add(_timeButtons[i]);
            }

            
            GenerateButtons();
        }

        private void NewGame()
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    gameField.Controls.Remove(_buttons[i,j]);
                }
            }

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
                Reveal(i,j);
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
            int count = 0;
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    if (_buttons[i, j].ImageIndex < 9)
                    {
                        count++;
                    }
                }
            }

            if (count == _fieldHeight * _fieldWidth - _mineCount)
            {
                _gameOver = true;
                int winTime = DateTime.Now.Subtract(_startDateTime).Seconds;
                MessageBox.Show("You won. Your time is " + winTime + " seconds.");
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

        private int ToIndex(int i, int j)
        {
            if (!CheckCoords(i, j)) return -1;
            return i + j * _fieldWidth;
        }

        private void EmptyField()
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    _field[i, j] = 0;
                }
            }
        }

        private void Reveal(int x, int y)
        {
            switch (_field[x,y])
            {
                case 9:
                    if (_buttons[x,y].ImageIndex != 11) _buttons[x, y].ImageIndex = 9;
                    break;
                default:
                    if (_buttons[x, y].ImageIndex == 11)
                    {
                        _buttons[x, y].ImageIndex = 13;
                    }
                    else
                    {
                        _buttons[x, y].ImageIndex = _field[x, y];
                    }

                    break;
            }
        }

        private void RevealMap()
        {
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    Reveal(i,j);
                }
            }
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
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    if (_field[i, j] != 9)
                    {
                        _field[i, j] = CountMines(i, j);
                    }
                }
            }
        }

        private void StartGame(int x, int y)
        {
            EmptyField();
            List<int> occupied = new List<int>();
            occupied.Add(ToIndex(x, y));
            Random random = new Random();
            for (int i = 0; i < _mineCount; i++)
            {
                int newIndex;
                do
                {
                    newIndex = random.Next(_fieldWidth * _fieldHeight);
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
                _mineButtons[0].ImageIndex = 10;
                c = Math.Abs(c);
                if (c > 99) c = 99;
            }
            else
            {
                _mineButtons[0].ImageIndex = c / 100 % 10;
            }
            _mineButtons[1].ImageIndex = c / 10 % 10;
            _mineButtons[2].ImageIndex = c / 1 % 10;
            
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
                _timeButtons[0].ImageIndex = c / 100 % 10;
                _timeButtons[1].ImageIndex = c / 10 % 10;
                _timeButtons[2].ImageIndex = c / 1 % 10;
                
            };
            _timeButtons[0].ImageIndex = 10;
            _timeButtons[1].ImageIndex = 10;
            _timeButtons[2].ImageIndex = 10;
            _currentMines = 0;
            _gameStarted = false;
            UpdateMineCount();
            Size = new Size(44 + _fieldWidth * _buttonSize, 146 + _fieldHeight * _buttonSize);
            _field = new int[_fieldWidth, _fieldHeight];
            _buttons = new Button[_fieldWidth, _fieldHeight];
            for (int j = 0; j < _fieldHeight; j++)
            {
                for (int i = 0; i < _fieldWidth; i++)
                {
                    _field[i, j] = 0;
                    _buttons[i, j] = new Button();
                    var x = i;
                    var y = j;

                    _buttons[i,j].MouseUp += delegate(object sender, MouseEventArgs args)
                    {
                        if (args.Button == MouseButtons.Right)
                        {
                            ButtonRightClick(_buttons[x, y], x, y);
                        }
                    };
                    _buttons[i, j].Click += delegate(Object target, EventArgs eventAgrs)
                    {
                        ButtonClick(_buttons[x, y], x, y);
                    };
                    _buttons[i, j].KeyUp += delegate(Object target, KeyEventArgs eventArgs)
                    {
                        switch (eventArgs.KeyCode)
                        {
                            case Keys.Up:
                                _buttons[x, Math.Max(y - 1, 0)].Focus();
                                eventArgs.Handled = true;
                                break;
                            case Keys.Down:
                                _buttons[x, Math.Min(y + 1, _fieldHeight - 1)].Focus();
                                eventArgs.Handled = true;
                                break;
                            case Keys.Left:
                                _buttons[Math.Max(x - 1, 0), y].Focus();
                                eventArgs.Handled = true;
                                break;
                            case Keys.Right:
                                _buttons[Math.Min(x + 1, _fieldWidth - 1), y].Focus();
                                eventArgs.Handled = true;
                                break;
                        }
                    };
                    _buttons[i, j].Size = new Size(_buttonSize, _buttonSize);
                    _buttons[i, j].Location = new Point(i * this._buttonSize, j * _buttonSize);
                    _buttons[i, j].FlatStyle = FlatStyle.Flat;
                    _buttons[i, j].FlatAppearance.BorderSize = 0;
                    _buttons[i,j].FlatAppearance.MouseOverBackColor = Color.Blue;
                    _buttons[i,j].ImageList = imageList;
                    _buttons[i, j].ImageIndex = 12;
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
            _fieldHeight = FieldHeights[difficulty];
            _fieldWidth = FieldWidths[difficulty];
            _mineCount = MineCounts[difficulty];
            GenerateButtons();
        }

        private void easyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(0);
            easyToolStripMenuItem.Checked = true;
            mToolStripMenuItem.Checked = false;
            masterToolStripMenuItem.Checked = false;
        }

        private void mToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(1);
            easyToolStripMenuItem.Checked = false;
            mToolStripMenuItem.Checked = true;
            masterToolStripMenuItem.Checked = false;
        }

        private void masterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Setup(2);
            easyToolStripMenuItem.Checked = false;
            mToolStripMenuItem.Checked = false;
            masterToolStripMenuItem.Checked = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}