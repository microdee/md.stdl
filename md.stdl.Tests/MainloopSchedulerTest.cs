using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interfaces;
using Xunit;

namespace md.stdl.Tests
{
    public class MainloopSchedulerTest
    {
        private int _currval = -1;

        [Fact]
        public void Test()
        {
            var array = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var scheduler = new MainloopScheduler();
            array.ToObservable().ObserveOn(scheduler).Subscribe(v => _currval = v);
            for (int i = 0; i < array.Length*3; i++)
            {
                if (i % 3 == 0)
                {
                    scheduler.Mainloop();
                    Assert.Equal(i/3, _currval);
                }
            }
        }
    }
}
