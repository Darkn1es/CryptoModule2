using CryptoModule2.Models.HashFunctions;
using CryptoModule2.ViewModels.Interfaces;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoModule2.ViewModels
{
    public class HashVM : BindableBase, ICipherVM
    {

        private readonly IHash _hash;

        private string _inputText = "";
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                RaisePropertyChanged( nameof( InputText ) );
            }
        }

        private string _outputText = "";
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                RaisePropertyChanged( nameof( OutputText ) );
            }
        }

        public DelegateCommand HashTextCommand { get; }
        public DelegateCommand HashFileCommand { get; }

        public HashVM(IHash hash)
        {
            _hash = hash;

            HashTextCommand = new DelegateCommand( () =>
             {
                 byte[] text = Encoding.Default.GetBytes( InputText );
                 byte[] digest = _hash.Hash( text );
                 OutputText = BitConverter.ToString( digest ).Replace( "-", string.Empty );
             } );

            HashFileCommand = new DelegateCommand( () =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if( openFileDialog.ShowDialog() == true )
                {
                    byte[] digest = _hash.Hash( openFileDialog.FileName );
                    OutputText = BitConverter.ToString( digest ).Replace( "-", string.Empty );
                }

            } );

        }

        public void ClearForm()
        {
            InputText = "";
            OutputText = "";
        }
    }
}
