using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConveyorApp.Exceptions;

namespace ConveyorApp
{
    internal class Mechanism
    {
        private readonly Random _random = new((int)DateTime.Now.Ticks);

        public async Task DoWork(CancellationToken cts)
        {
            await Task.Delay(_random.Next(100, 5000), cts);

            if (_random.Next(0, 10) > 7)
            {
                throw new ConveyorFailureException();
            }
        }
    }
}
