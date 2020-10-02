using CryptoModule2.Models.Ciphers;
using CryptoModule2.Models.Ciphers.Asymmetric;
using CryptoModule2.Models.Ciphers.Keys;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoModule2.ViewModels
{
    class RSApageVM : BindableBase, ICipherVM
    {


        #region Binding properties

        #region fields
        private string _inputText = "";
        private string _outputText = "";

        private string _p = "";
        private string _q = "";

        private string _modulus = "448823";
        private string _publicExponent = "305269";
        private string _privateExponent = "224077";

        private bool _isDoingCipher = false;

        private double _currentProgress = 0.0f;

        #endregion

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                RaisePropertyChanged( nameof( InputText ) );
            }
        }
        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                RaisePropertyChanged( nameof( OutputText ) );
            }
        }
        public string P
        {
            get => _p;
            set
            {
                _p = value;
                RaisePropertyChanged( nameof( P ) );
            }
        }
        public string Q
        {
            get => _q;
            set
            {
                _q = value;
                RaisePropertyChanged( nameof( Q ) );
            }
        }

        public string Modulus
        {
            get => _modulus;
            set
            {
                _modulus = value;
                RaisePropertyChanged( nameof( Modulus ) );
            }
        }
        public string PublicExponent
        {
            get => _publicExponent;
            set
            {
                _publicExponent = value;
                RaisePropertyChanged( nameof( PublicExponent ) );
            }
        }
        public string PrivateExponent
        {
            get => _privateExponent;
            set
            {
                _privateExponent = value;
                RaisePropertyChanged( nameof( PrivateExponent ) );
            }
        }

        public bool IsDoingCipher
        {
            get => _isDoingCipher;
            set
            {
                _isDoingCipher = value;
                RaisePropertyChanged( nameof( IsDoingCipher ) );
            }
        }

        public double CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                RaisePropertyChanged( nameof( CurrentProgress ) );
            }
        }

        #endregion


        #region Private fields

        private RSA _rsa = new RSA();

        #endregion

        #region Buttons handlers

        public DelegateCommand GenerateCommand { get; } = new DelegateCommand( () =>
         {

         } );


        public DelegateCommand ConvertKeyCommand { get; } = new DelegateCommand( () =>
         {

         } );

        public DelegateCommand EncryptCommand { get; } = new DelegateCommand( () =>
         {

         } );

        public DelegateCommand DecryptCommand { get; } = new DelegateCommand( () =>
         {

         } );

        public DelegateCommand EncryptFileCommand { get; }

        public DelegateCommand DecryptFileCommand { get; }

        #endregion


        public RSApageVM()
        {
            #region Buttons handlers

            EncryptFileCommand = new DelegateCommand( () =>
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if( openFileDialog.ShowDialog() == true )
                    {
                        string inputPath = openFileDialog.FileName;
                        string outputPath = openFileDialog.FileName + ".enc";

                        RSAKey key = GetPublicKey();

                        IsDoingCipher = true;
                        Thread thread = new Thread( () =>
                         {
                             _rsa.Encrypt( inputPath, outputPath, key, percent =>
                             {
                                 CurrentProgress = percent;
                             } );
                             CurrentProgress = 100f;

                             MessageBox.Show( "Файл успешно зашифрован" );
                             IsDoingCipher = false;
                             CurrentProgress = 0f;

                         } );
                        thread.Start();
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.Message );
                }

            } );

            DecryptFileCommand = new DelegateCommand( () =>
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if( openFileDialog.ShowDialog() == true )
                    {
                        string inputPath = openFileDialog.FileName;
                        string outputPath = openFileDialog.FileName.Replace( ".enc", "" );

                        RSAKey key = GetPrivateKey();

                        IsDoingCipher = true;
                        Thread thread = new Thread( () =>
                        {
                            _rsa.Decrypt( inputPath, outputPath, key, percent =>
                            {
                                CurrentProgress = percent;
                            } );
                            CurrentProgress = 100f;

                            MessageBox.Show( "Файл успешно расшифрован" );
                            IsDoingCipher = false;
                            CurrentProgress = 0f;

                        } );
                        thread.Start();
                    }
                }
                catch( Exception ex )
                {
                    MessageBox.Show( ex.Message );
                }

            } );



            #endregion
        }

        public void ClearForm()
        {
            InputText = "";
            OutputText = "";
            P = "";
            Q = "";
            Modulus = "";
            PublicExponent = "";
            PrivateExponent = "";
        }

        private RSAKey GetPrivateKey()
        {
            return new RSAKey( true, PrivateExponent, Modulus );
        }

        private RSAKey GetPublicKey()
        {
            return new RSAKey( false, PublicExponent, Modulus );
        }
    }
}
