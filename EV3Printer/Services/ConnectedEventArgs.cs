namespace EV3Printer.Services
{
   public class ConnectedEventArgs
    {
        public bool State { get; private set; }
        public ConnectedEventArgs(bool state)
        {
            State = state;
        }
    }
}