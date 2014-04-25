using System.Threading;

namespace EmailIpAddressChange
{
    internal class Switch
    {
        private int _state;

        public bool TurnOn()
        {
            return 0 == Interlocked.CompareExchange(ref _state, 1, 0);
        }

        public bool TurnOff()
        {
            return 1 == Interlocked.CompareExchange(ref _state, 0, 1);
        }
    }
}