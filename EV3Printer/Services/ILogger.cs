using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    public interface ILogger
    {
        void Clear();
        void Log(string msg);
        ObservableCollection<string> Logs { get; }
    }
}
