using Caliburn.Micro;
using SinkMyBattleshipWPF.ViewModels;
using System.Windows;

namespace SinkMyBattleshipWPF
{
    public class Bootstrapper: BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
