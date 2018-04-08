using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using md.stdl.Time;

namespace md.stdl.DelayTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private float _value = 1.0f;
        private float _delayVal = 0.0f;
        private Delay<float> _delay = new Delay<float>(TimeSpan.FromSeconds(3));

        private void OnInitialized(object sender, EventArgs e)
        {
            Observable.Interval(TimeSpan.FromMilliseconds(10)).Subscribe(t =>
            {
                _value = Math.Max(0.0f, _value - 0.01f);

                _delayVal = _delay.Update(_value, TimeSpan.FromSeconds(1.5));

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CurrentValueVis.Value = _value * 100;
                    DelayedValueVis.Value = _delayVal * 100;
                    Timer.Text = _delay.Timer.Elapsed.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo);
                    if(_delay.Samples == null) return;
                    Count.Text = _delay.Samples.Count.ToString();
                    if(_delay.Samples.Count == 0) return;
                    FirstValueVis.Value = _delay.Samples[0].Frame * 100;
                    if (_delay.Samples.Count == 1) return;
                    LastValueVis.Value = _delay.Samples.Last().Frame * 100;
                }));
            });
        }

        private void OnBang(object sender, RoutedEventArgs e)
        {
            _value = 1.0f;
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            _value = 1.0f;
            _delay = new Delay<float>(TimeSpan.FromSeconds(3));
        }
    }
}
