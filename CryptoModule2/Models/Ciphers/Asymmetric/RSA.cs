using CryptoModule2.Models.Ciphers.Keys;
using System;
using System.Collections.Generic;
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

            return ProcessRSA( ciphertext, rsaKey, false );

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

                double percent = ( double )instream.Position / fileSize;
                ProgressChanged?.Invoke( percent );
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

            return ProcessRSA( text, rsaKey, true );

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

            byte[] inputChunk = new byte[ rsaKey.MaxOpenTextSize ];
            byte[] outputChunk = new byte[ rsaKey.MaxCipherTextSize ];

            while( instream.Read( inputChunk, 0, rsaKey.MaxOpenTextSize ) != 0 )
            {
                outputChunk = Encrypt( inputChunk, rsaKey );
                outstream.Write( outputChunk, 0, outputChunk.Length );

                double percent = ( double )instream.Position / fileSize;
                ProgressChanged?.Invoke( percent );
            }
        }

        private byte[] ProcessRSA( byte[] inputBlock, RSAKey rsaKey, bool needPadding = true )
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



            for( int currentByte = 0; currentByte < inputBlock.Length; currentByte += readSize )
            {
                int byteCopyCount = Math.Min( readSize, inputBlock.Length - currentByte );

                byte[] currentBlock = new byte[ byteCopyCount + 1 ]; // Добавлен 0х00 чтобы число было положительным 
                Buffer.BlockCopy( inputBlock, currentByte, currentBlock, 0, byteCopyCount );

                BigInteger openInt = new BigInteger( currentBlock );
                BigInteger cipherInt = BigInteger.ModPow( openInt, rsaKey.Exponent, rsaKey.Modulus );
                byte[] cipherBlock = cipherInt.ToByteArray( false );


                byte[] packedBlock = new byte[ writeSize ];
                Buffer.BlockCopy( cipherBlock, 0, packedBlock, 0, cipherBlock.Length );

                result.AddRange( packedBlock );

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
