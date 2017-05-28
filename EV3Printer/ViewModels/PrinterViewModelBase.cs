using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.ViewModels
{
    public class PrinterViewModelBase : ViewModelBase
    {
        private IEV3Brick _brick;

        private RelayCommand _newPageCommand;
        public RelayCommand NewPageCommand => _newPageCommand ?? (_newPageCommand = new RelayCommand(
            () => {
                _brick.Send("FEED;-25");
            }
        ));

        private RelayCommand _dropPageCommand;
        public RelayCommand DropPageCommand => _dropPageCommand ?? (_dropPageCommand = new RelayCommand(
            () => {
                _brick.Send("FEED;-750");
            }
        ));

        public PrinterViewModelBase(IEV3Brick brick)
        {
            _brick = brick;
        }
    }
}
