using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using md.stdl.Interaction;
using RawMouse = VVVV.Utils.IO.Mouse;
using UserControl = System.Windows.Controls.UserControl;

namespace md.stdl.MouseKeyboardTest
{
    /// <summary>
    /// Interaction logic for MouseDevice.xaml
    /// </summary>
    public partial class MouseDevice : UserControl
    {
        private static readonly Brush ButtonBrushPressed = new SolidColorBrush(new Color { R = 0, G = 0, B = 0, A = 255 });
        private static readonly Brush ButtonBrushReleased = new SolidColorBrush(new Color { R = 128, G = 128, B = 128, A = 255 });

        public MouseDevice(string name, RawMouse observableMouse, AccumulatingMouseObserver accMouse)
        {
            InitializeComponent();
            DeviceName.Text = name;
            observableMouse.MouseNotifications.Subscribe(notification =>
            {
                Dispatcher.BeginInvoke((Action) (() =>
                {
                    ImmediateEvents.Text = notification.Kind.ToString();
                    ImmediateX.Text = notification.Position.X.ToString();
                    ImmediateY.Text = notification.Position.Y.ToString();
                }));
            });
            Observable.Interval(new TimeSpan(0, 0, 0, 0, 20)).Subscribe(t =>
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    AccX.Text = accMouse.AccumulatedXDelta.ToString();
                    AccY.Text = accMouse.AccumulatedYDelta.ToString();
                    AccWH.Text = accMouse.AccumulatedHorizontalWheelDelta.ToString();
                    AccWV.Text = accMouse.AccumulatedWheelDelta.ToString();
                    ImmediateWV.Text = accMouse.MouseClicks[MouseButtons.Left].ClickCount.ToString();

                    LeftButtonIndicator.Fill = accMouse.MouseClicks[MouseButtons.Left].Pressed
                        ? ButtonBrushPressed
                        : ButtonBrushReleased;
                    MiddleButtonIndicator.Fill = accMouse.MouseClicks[MouseButtons.Middle].Pressed
                        ? ButtonBrushPressed
                        : ButtonBrushReleased;
                    RightButtonIndicator.Fill = accMouse.MouseClicks[MouseButtons.Right].Pressed
                        ? ButtonBrushPressed
                        : ButtonBrushReleased;
                    Thumb1ButtonIndicator.Fill = accMouse.MouseClicks[MouseButtons.XButton1].Pressed
                        ? ButtonBrushPressed
                        : ButtonBrushReleased;
                    Thumb2ButtonIndicator.Fill = accMouse.MouseClicks[MouseButtons.XButton2].Pressed
                        ? ButtonBrushPressed
                        : ButtonBrushReleased;

                    accMouse.ResetAccumulation();
                }));
            });
        }
    }
}
