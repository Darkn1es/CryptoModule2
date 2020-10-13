using CryptoModule2.Views.Pages;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CryptoModule2.ViewModels
{
    class MainVM : BindableBase
    {
        private Page _currentPageContent;
        public Page CurrentPageContent
        {
            get
            {
                return _currentPageContent;
            }
            private set
            {
                _currentPageContent = value;
                RaisePropertyChanged( nameof( CurrentPageContent ) );
            }
        }

        private Page _rsaPage = new RSAPage();
        private Page _dhPage = new DHPage();
        private Page _shamirPage = new ShamirPage();



        public MainVM()
        {
            CurrentPageContent = _rsaPage;
        }
    }

}

