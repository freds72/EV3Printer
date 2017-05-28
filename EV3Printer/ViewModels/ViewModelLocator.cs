using EV3Printer.Services;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace EV3Printer.ViewModels
{
    public class ViewModelLocator
    {
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public ScannerViewModel Scanner
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ScannerViewModel>();
            }
        }

        public PrinterViewModel Printer
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PrinterViewModel>();
            }
        }


        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PrinterViewModel>();
            SimpleIoc.Default.Register<ScannerViewModel>();
            SimpleIoc.Default.Register<IEV3Brick>(() => { return new EV3Brick(); }, false);
            // SimpleIoc.Default.Register<IEV3Brick>(() => { return new MockBrick(); }, false);
            SimpleIoc.Default.Register<ILogger>(() => { return new Logger(); }, false);
            SimpleIoc.Default.Register(() => { return new ScannerSettings(); }, false);
            SimpleIoc.Default.Register(() => { return new PrinterSettings(); }, false);
            SimpleIoc.Default.Register(() => { return new SystemSettings(); }, false);
        }
    }
}
