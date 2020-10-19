using CryptoModule2.Models.Ciphers;
using CryptoModule2.Models.Ciphers.Asymmetric;
using CryptoModule2.Models.Ciphers.Keys;
using CryptoModule2.Models.Ciphers.Parameters;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CryptoModule2.ViewModels
{
    public class ElgamalVM : BindableBase
    {
        #region fields
        private ElgamalParameters _parameters;
        private AsymmetricCipherKeyPair _aliceKey;
        private AsymmetricCipherKeyPair _bobKey;

        private string _aliceText = "";
        private string _bobText = "";
        private string _cipherText = "";

        private double _currentProgress = 0f;
        private bool _isDoingCipher = false;
        private Random _rand = new Random();
        private Elgamal _elgamal = new Elgamal();

        #endregion

        #region Properties

        public ElgamalParameters Parameters
        {
            get => _parameters;
            private set
            {
                _parameters = value;
                RaisePropertyChanged( nameof( Parameters ) );
                ClearTexts();
            }
        }

        public AsymmetricCipherKeyPair AliceKey
        {
            get => _aliceKey;
            private set
            {
                _aliceKey = value;
                RaisePropertyChanged( nameof( AliceKey ) );
                ClearTexts();
            }
        }

        public AsymmetricCipherKeyPair BobKey
        {
            get => _bobKey;
            private set
            {
                _bobKey = value;
                RaisePropertyChanged( nameof( BobKey ) );
                ClearTexts();
            }

        }

        public string AliceText
        {
            get => _aliceText;
            set
            {
                _aliceText = value;
                RaisePropertyChanged( nameof( AliceText ) );
            }
        }

        public string BobText
        {
            get => _bobText;
            set
            {
                _bobText = value;
                RaisePropertyChanged( nameof( BobText ) );
            }
        }

        public string CipherText
        {
            get => _cipherText;
            set
            {
                _cipherText = value;
                RaisePropertyChanged( nameof( CipherText ) );
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

        public bool IsDoingCipher
        {
            get => _isDoingCipher;
            set
            {
                _isDoingCipher = value;
                RaisePropertyChanged( nameof( IsDoingCipher ) );
            }
        }

        #endregion


        #region Commands
        public DelegateCommand GenerateParametersCommand { get; }
        public DelegateCommand<bool?> GenerateKeyCommand { get; }
        public DelegateCommand<bool?> SendMessageCommand { get; }
        public DelegateCommand<bool?> EncryptFileCommand { get; }
        public DelegateCommand<bool?> DecryptFileCommand { get; }

        #endregion
        public ElgamalVM()
        {
            GenerateAll();

            GenerateParametersCommand = new DelegateCommand( () =>
            {
                GenerateAll();
            } );

            GenerateKeyCommand = new DelegateCommand<bool?>( isAlice =>
            {
                if( isAlice == true )
                {
                    AliceKey = ElgamalKey.GenerateKeyPair( Parameters );
                }
                else if( isAlice == false )
                {
                    BobKey = ElgamalKey.GenerateKeyPair( Parameters );
                }
            } );

            SendMessageCommand = new DelegateCommand<bool?>( isAlice =>
            {
                try
                {
                    Debug.Flush();
                    string text = "";
                    AsymmetricCipherKeyPair key;

                    if( isAlice == true )
                    {
                        text = AliceText;
                        key = BobKey;
                    } else
                    {
                        text = BobText;
                        key = AliceKey;
                    }

                    byte[] openBlock = Encoding.Default.GetBytes( text );
                    byte[] cipherBlock = _elgamal.Encrypt( openBlock, key.PublicKey );
                    byte[] gotBlock = _elgamal.Decrypt( cipherBlock, key.PrivateKey );

                    CipherText = BitConverter.ToString( cipherBlock ).Replace( "-", string.Empty );

                    if( isAlice == true )
                    {
                        BobText = Encoding.Default.GetString( gotBlock );
                    }
                    else if( isAlice == false )
                    {
                        AliceText = Encoding.Default.GetString( gotBlock );
                    }
            }
                catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }

        } );

            EncryptFileCommand = new DelegateCommand<bool?>( (isAlice) =>
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    if( openFileDialog.ShowDialog() == true )
                    {
                        string inputPath = openFileDialog.FileName;
                        string outputPath = openFileDialog.FileName + ".enc";

                        IKey key = isAlice == true ? BobKey.PublicKey : AliceKey.PublicKey; 

                        IsDoingCipher = true;
                        Thread thread = new Thread( () =>
                        {
                            _elgamal.Encrypt( inputPath, outputPath, key, percent =>
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
                catch( Exception )
                {
                    MessageBox.Show( "Ошибка! Проверьте входные параметры" );
                }

            } );

            DecryptFileCommand = new DelegateCommand<bool?>( ( isAlice ) =>
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    SaveFileDialog saveFileDialog = new SaveFileDialog();


                    if( openFileDialog.ShowDialog() == true )
                    {
                        string inputPath = openFileDialog.FileName;
                        if( saveFileDialog.ShowDialog() == true )
                        {
                            string outputPath = saveFileDialog.FileName;

                            IKey key = isAlice == true ? AliceKey.PrivateKey : BobKey.PrivateKey;

                            IsDoingCipher = true;
                            Thread thread = new Thread( () =>
                            {
                                _elgamal.Decrypt( inputPath, outputPath, key, percent =>
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
                }
                catch( Exception )
                {
                    MessageBox.Show( "Ошибка! Проверьте входные параметры" );
                }

            } );


        }

        private void GenerateAll()
        {
            Parameters = ElgamalParameters.Generate( _rand.Next( 7, 30 ) );
            AliceKey = ElgamalKey.GenerateKeyPair( Parameters );
            BobKey = ElgamalKey.GenerateKeyPair( Parameters );
        }

        private void ClearTexts()
        {
            AliceText = "";
            BobText = "";
            CipherText = "";
        }
    }
}
