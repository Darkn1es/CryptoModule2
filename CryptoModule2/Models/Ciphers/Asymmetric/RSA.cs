using CryptoModule2.Models.Ciphers.Keys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace CryptoModule2.Models.Ciphers.Asymmetric
{
    public class RSA : ICipher, IFileCipher
    {
        public string CipherName => "RSA";

        public byte[] Decrypt( byte[] ciphertext, IKey key = null )
        {
            ValidateInput( ciphertext, key );

            var rsaKey = key as RSAKey; // Проверяется в ValidateInput
            if( !rsaKey.IsPrivate )
            {
                throw new ArgumentException( "Ключ должен быть приватным" );
            }

            return ProcessRSA( ciphertext, rsaKey );

        }

        public void Decrypt( string inputPath, string outputPath, IKey key, Action<double> ProgressChanged = null )
        {
            if( key is RSAKey == false )
            {
                throw new ArgumentException( "Key is not RSAKey" );
            }

            RSAKey rsaKey = key as RSAKey;

            using FileStream instream = File.OpenRead( inputPath );
            using FileStream outstream = File.Open( outputPath, FileMode.Create );

            long fileSize = instream.Length;

            byte[] inputChunk = new byte[ rsaKey.MaxCipherTextSize ];
            byte[] outputChunk = new byte[ rsaKey.MaxOpenTextSize ];

            while( instream.Read( inputChunk, 0, rsaKey.MaxCipherTextSize ) != 0 )
            {
                outputChunk = Decrypt( inputChunk, rsaKey );
                outstream.Write( outputChunk, 0, outputChunk.Length );

                long percent = ( instream.Position * 100 ) / fileSize;

                ProgressChanged?.Invoke( (int)percent );
            }
        }

        public byte[] Encrypt( byte[] text, IKey key = null )
        {
            ValidateInput( text, key );
            var rsaKey = key as RSAKey; // Проверяется в ValidateInput
            if( rsaKey.IsPrivate )
            {
                throw new ArgumentException( "Ключ должен быть публичным" );
            }

            return ProcessRSA( text, rsaKey );

        }

        public void Encrypt( string inputPath, string outputPath, IKey key, Action<double> ProgressChanged = null )
        {
            if( key is RSAKey == false )
            {
                throw new ArgumentException( "Key is not RSAKey" );
            }

            RSAKey rsaKey = key as RSAKey;

            using FileStream instream = File.OpenRead( inputPath );
            using FileStream outstream = File.Open( outputPath, FileMode.Create );

            long fileSize = instream.Length;

            byte[] inputChunk = new byte[ rsaKey.MaxOpenTextSize * 1024 * 1024 * 4];
            byte[] outputChunk = new byte[ rsaKey.MaxCipherTextSize ];

            while( instream.Read( inputChunk, 0, rsaKey.MaxOpenTextSize * 1024 * 1024 * 4 ) != 0 )
            {
                outputChunk = Encrypt( inputChunk, rsaKey );
                outstream.Write( outputChunk, 0, outputChunk.Length );

                long percent = (instream.Position * 100) / fileSize;
                ProgressChanged?.Invoke( (int)percent );
            }
        }

        private byte[] ProcessRSA( byte[] inputBlock, RSAKey rsaKey)
        {
            List<byte> result = new List<byte>( inputBlock.Length );

            int readSize = 0;
            int writeSize = 0;
            if( rsaKey.IsPrivate )
            {
                readSize = rsaKey.MaxCipherTextSize;
                writeSize = rsaKey.MaxOpenTextSize;
            }
            else
            {
                readSize = rsaKey.MaxOpenTextSize;
                writeSize = rsaKey.MaxCipherTextSize;
            }
            long time = 0;
            try
            {
                for( int currentByte = 0; currentByte < inputBlock.Length; currentByte += readSize )
                {
                    time = 0;
                    Debug.WriteLine( "Block" );
                    var watch = Stopwatch.StartNew();
                    int byteCopyCount = Math.Min( readSize, inputBlock.Length - currentByte );

                    time = watch.ElapsedTicks;
                    Debug.WriteLine( $"1 - {watch.ElapsedTicks}" );

                    byte[] currentBlock = new byte[ byteCopyCount + 1 ]; // Добавлен 0х00 чтобы число было положительным 
                    
                    Debug.WriteLine( $"2 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;

                    Buffer.BlockCopy( inputBlock, currentByte, currentBlock, 0, byteCopyCount );

                    Debug.WriteLine( $"3 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;

                    BigInteger openInt = new BigInteger( currentBlock );
                    BigInteger cipherInt = BigInteger.ModPow( openInt, rsaKey.Exponent, rsaKey.Modulus );
                    Debug.WriteLine( $"4 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;

                    byte[] cipherBlock = cipherInt.ToByteArray( false );
                    Debug.WriteLine( $"5 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;

                    byte[] packedBlock = new byte[ writeSize ];
                    Debug.WriteLine( $"6 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;
                    Buffer.BlockCopy( cipherBlock, 0, packedBlock, 0, cipherBlock.Length );
                    Debug.WriteLine( $"7 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;

                    result.AddRange( packedBlock );
                    Debug.WriteLine( $"8 - {watch.ElapsedTicks - time}" );
                    time = watch.ElapsedTicks;
                    watch.Stop();
                    Debug.WriteLine( "END" );


                }
            }
            catch( Exception ex)
            {

                throw new Exception( "Ошибка! Проверьте входные параметры.", ex );
            }



            return result.ToArray();
        }

        private void ValidateInput( byte[] inputData, IKey key )
        {
            if( inputData == null )
            {
                throw new ArgumentNullException( "Пустые входные данные" );
            }
            if( inputData.Length == 0 )
            {
                throw new ArgumentNullException( "Пустые входные данные" );
            }
            if( key is RSAKey == false )
            {
                throw new ArgumentException( "Неверный ключ" );
            }

        }
    }
}
