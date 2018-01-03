using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using md.stdl.Interaction;

namespace md.stdl.MouseKeyboardTest
{
    public class KeyboardRecord
    {
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            var mergedmouseman = new MouseInputManager();
            mergedmouseman.SelectDevice(-1);
            var individualmouseman = new MouseInputManager();
            individualmouseman.SelectDevice(Enumerable.Range(0, mergedmouseman.RawDevices.Count).ToArray());
            var mice = mergedmouseman.Devices.Concat(individualmouseman.Devices).ToArray();
            var mousenames = mergedmouseman.DeviceNames.Concat(individualmouseman.DeviceNames).ToArray();
            var accmice = mice.Select(currmnouse =>
            {
                var res = new AccumulatingMouseObserver();
                res.SubscribeTo(currmnouse.MouseNotifications);
                return res;
            }).ToArray();
            Dispatcher.BeginInvoke((Action) (() =>
            {
                for (int i = 0; i < accmice.Length; i++)
                {
                    Mice.Items.Add(new MouseDevice(mousenames[i], mice[i], accmice[i]));
                }
            }));

            var mergedkeyman = new KeyboardInputManager();
            mergedkeyman.SelectDevice(-1);
            var individualkeyman = new KeyboardInputManager();
            individualkeyman.SelectDevice(Enumerable.Range(0, mergedkeyman.RawDevices.Count).ToArray());
            var keyboards = mergedkeyman.Devices.Concat(individualkeyman.Devices).ToArray();
            var keyboardnames = mergedkeyman.DeviceNames.Concat(individualkeyman.DeviceNames).ToArray();
            var acckeyboards = keyboards.Select(currkb =>
            {
                var res = new AccumulatingKeyboardObserver();
                res.SubscribeTo(currkb.KeyNotifications);
                return res;
            }).ToArray();

            Dispatcher.BeginInvoke((Action) (() =>
            {
                AggregatedKeyboards = new ObservableCollection<string>(new string[acckeyboards.Length]);
                Keyboards.ItemsSource = AggregatedKeyboards;
                Observable.Interval(new TimeSpan(0, 0, 0, 0, 20)).Subscribe(t =>
                {
                    Dispatcher.BeginInvoke((Action) (() =>
                    {
                        for (int i = 0; i < AggregatedKeyboards.Count; i++)
                        {
                            AggregatedKeyboards[i] = acckeyboards[i].Keypresses
                                .Where(kvp => kvp.Value.Pressed)
                                .Aggregate(keyboardnames[i] + Environment.NewLine, (s, key) =>
                                {
                                    return s += (s == keyboardnames[i] + Environment.NewLine ? "" : ", ") +
                                        key.Key.ToString()
                                            .Repeat(Math.Max(key.Value.Count, 1))
                                            .Aggregate("", (s1, c) => s1 + c);
                                });
                            acckeyboards[i].ResetAccumulation();
                        }
                    }));
                });
            }));
        }

        public ObservableCollection<string> AggregatedKeyboards { get; set; }
    }
}
