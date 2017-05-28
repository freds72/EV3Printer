using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    public class Logger : ILogger
    {
        private ObservableCollection<string> _logs = new ObservableCollection<string>();

        public ObservableCollection<string> Logs
        {
            get
            {
                return _logs;
            }
        }

        public void Clear()
        {
            _logs.Clear();
        }

        public void Log(string msg)
        {
            _logs.Add(msg);
        }
    }
}
