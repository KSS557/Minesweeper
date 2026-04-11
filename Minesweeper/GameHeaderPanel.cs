using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Minesweeper
{
    public class GameHeaderPanel
    {
        // Панели
        public StackPanel Panel { get; } = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Левая панель — таймер 3 цифры
        private readonly Image[] _timerDigits = new Image[3];
        // Правая панель — флаги 3 цифры
        private readonly Image[] _flagDigits = new Image[3];

        // Лицо‑кнопка
        private readonly Image _faceImage;
        public Image FaceImage => _faceImage;

        public GameHeaderPanel()
        {

            var dockPanel = new DockPanel
            {
                LastChildFill = true  // Центр растягивается
            };


            // Левая панель — таймер
            var panelTimer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };
            for (int i = 0; i < 3; i++)
            {
                _timerDigits[i] = new Image { Width = 26, Height = 46, SnapsToDevicePixels = true, };
                panelTimer.Children.Add(_timerDigits[i]);
            }
            
            

            // Правая панель — флаги
            var panelFlag = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            for (int i = 0; i < 3; i++)
            {
                _flagDigits[i] = new Image { Width = 26, Height = 46, SnapsToDevicePixels = true, };
                panelFlag.Children.Add(_flagDigits[i]);
            }

            


            // Лицо-кнопка (Image внутри Button)
            _faceImage = new Image
            {
                Width = 48,
                Height = 48,
                Margin = new Thickness(10, 0, 10, 0),
                SnapsToDevicePixels = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };


            DockPanel.SetDock(panelFlag, Dock.Right);
            dockPanel.Children.Add(panelFlag);

            // Face центр
            dockPanel.Children.Add(_faceImage);

            // Timer слева
            DockPanel.SetDock(panelTimer, Dock.Left);
            dockPanel.Children.Add(panelTimer);

            /*Panel.Children.Add(panelTimer);
            Panel.Children.Add(_faceImage);
            Panel.Children.Add(panelFlag);*/

            Panel.Children.Add(dockPanel);

            Reset();
        }

        public void Reset()
        {
            SetTime(0);
            SetFlags(0);
            FaceImage.Source = MinesweeperTextures.FaceSmile;
        }

        public void SetTime(int seconds)
        {
            int v = Math.Max(0, Math.Min(999, seconds));
            for (int i = 2; i >= 0; i--)
            {
                int digit = v % 10;
                v /= 10;
                _timerDigits[i].Source = GetNumberBitmap(digit);
            }
        }

        public void SetFlags(int remainingFlags)
        {
            int v = Math.Max(-99, Math.Min(999, remainingFlags));

            if (v >= 0)
            {
                for (int i = 2; i >= 0; i--)
                {
                    int digit = v % 10;
                    v /= 10;
                    _flagDigits[i].Source = GetNumberBitmap(digit);
                }
            }
            else
            {
                v = -v;
                int count = Math.Abs(v).ToString().Length;
                for (int i = 2; i >= 0; i--)
                {
                    int digit = v % 10;
                    v /= 10;
                    _flagDigits[i].Source = GetNumberBitmap(digit);
                }
                if (count == 1)
                {
                    _flagDigits[1].Source = GetNumberBitmap(-1);
                }
                else if (count == 2)
                {
                    _flagDigits[0].Source = GetNumberBitmap(-1);
                }
                
            }

            
        }

        private BitmapImage GetNumberBitmap(int digit)
        {
            return digit switch
            {
                -1 => MinesweeperTextures.NumberMinus,
                0 => MinesweeperTextures.Number0,
                1 => MinesweeperTextures.Number1,
                2 => MinesweeperTextures.Number2,
                3 => MinesweeperTextures.Number3,
                4 => MinesweeperTextures.Number4,
                5 => MinesweeperTextures.Number5,
                6 => MinesweeperTextures.Number6,
                7 => MinesweeperTextures.Number7,
                8 => MinesweeperTextures.Number8,
                9 => MinesweeperTextures.Number9,
                _ => MinesweeperTextures.Number0
            };
        }

        public void SetFace(BitmapImage face)
        {
            FaceImage.Source = face;
        }
    }
}
