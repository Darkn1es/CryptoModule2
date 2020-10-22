using CryptoModule2.Models.Ciphers.Keys;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoModule2.Models.Ciphers.Asymmetric
{
    public class Elgamal : ICipher, IFileCipher
    {
        public string CipherName => "Эль-Гамаль";

        public byte[] Decrypt( byte[] ciphertext, IKey key = null )
        {
            Validate( ciphertext, key );

            ElgamalKey elgamalKey = key as ElgamalKey;
            if( !elgamalKey.IsPrivate )
            {
                throw new ArgumentException( "Для расшифровывания нужен закрытый ключ" );
            }
            List<byte> result = new List<byte>( ciphertext.Length );

            try
            {
                int readSize = elgamalKey.MaxCipherTextSize;
                int writeSize = elgamalKey.MaxOpenTextSize;

                byte[] rBlock = new byte[ readSize + 1 ];
                Buffer.BlockCopy( ciphertext, 0, rBlock, 0, readSize );
                BigInteger r = new BigInteger( rBlock );
                BigInteger decryptConst = BigInteger.ModPow( r, elgamalKey.Parameters.P - 1 - elgamalKey.Key, elgamalKey.Parameters.P );

                for( int currentByte = readSize; currentByte < ciphertext.Length; currentByte += readSize )
                {
                    int byteCopyCount = Math.Min( readSize, ciphertext.Length - currentByte );
                    byte[] currentBlock = new byte[ byteCopyCount + 1 ]; // Добавлен 0х00 чтобы число было положительным 

                    Buffer.BlockCopy( ciphertext, currentByte, currentBlock, 0, byteCopyCount );
                    BigInteger cipherInt = new BigInteger( currentBlock );
                    BigInteger openInt;
                    BigInteger.DivRem( cipherInt * decryptConst, elgamalKey.Parameters.P, out openInt );

                    byte[] openBlock = openInt.ToByteArray( false );
                    byte[] packedBlock = new byte[ writeSize ];

                    Buffer.BlockCopy( openBlock, 0, packedBlock, 0, openBlock.Length );
                    result.AddRange( packedBlock );
                }

            }
            catch( Exception ex )
            {
                throw new Exception( "Ошибка! Проверьте входные параметры.", ex );
            }

            return result.ToArray();
        }

        public void Decrypt( string inputPath, string outputPath, IKey key, Action<double> ProgressChanged = null )
        {
            if( key is ElgamalKey == false )
            {
                throw new ArgumentException( "Key is not ElgamalKey" );
            }

            ElgamalKey elgamalKey = key as ElgamalKey;

            if( !elgamalKey.IsPrivate )
            {
                throw new ArgumentException( "Для расшифровывания нужен закрытый ключ" );
            }

            try
            {
                using FileStream instream = File.OpenRead( inputPath );
                using FileStream outstream = File.Open( outputPath, FileMode.Create );

                long fileSize = instream.Length;

                int readSize = elgamalKey.MaxCipherTextSize * 1024 + elgamalKey.MaxCipherTextSize;

                byte[] inputChunk = new byte[ readSize ];
                int size = 0;

                while( ( size = instream.Read( inputChunk, 0, readSize ) ) != 0 )
                {

                    byte[] currentBlock;
                    if( size == readSize )
                    {
                        currentBlock = inputChunk;
                    }
                    else
                    {
                        currentBlock = new byte[ size ];
                        Buffer.BlockCopy( inputChunk, 0, currentBlock, 0, size );
                    }
                    byte[] outputChunk = Decrypt( currentBlock, elgamalKey );
                    outstream.Write( outputChunk, 0, outputChunk.Length );

                    int percent = ( int )( ( instream.Position * 100f ) / fileSize );
                    ProgressChanged?.Invoke( percent );
                }
            }
            catch( Exception ex )
            {
                throw new Exception( "Ошибка! Проверьте входные параметры.", ex );
            }


        }

        public byte[] Encrypt( byte[] text, IKey key = null )
        {
            Validate( text, key );

            ElgamalKey elgamalKey = key as ElgamalKey;

            if( elgamalKey.IsPrivate )
            {
                throw new ArgumentException( "Для шифрования нужен публичный ключ" );
            }

            List<byte> result = new List<byte>( text.Length );

            try
            {
                int readSize = elgamalKey.MaxOpenTextSize;
                int writeSize = elgamalKey.MaxCipherTextSize;

                BigInteger k = Helper.GenerateBigInteger( 2, elgamalKey.Parameters.P - 3 );

                BigInteger r = BigInteger.ModPow( elgamalKey.Parameters.G, k, elgamalKey.Parameters.P );

                byte[] packedBlock = new byte[ writeSize ];

                byte[] rBlock = r.ToByteArray( false );
                Buffer.BlockCopy( rBlock, 0, packedBlock, 0, rBlock.Length );
                result.AddRange( packedBlock );

                for( int currentByte = 0; currentByte < text.Length; currentByte += readSize )
                {
                    int byteCopyCount = Math.Min( readSize, text.Length - currentByte );
                    byte[] currentBlock = new byte[ byteCopyCount + 1 ]; // Добавлен 0х00 чтобы число было положительным 

                    Buffer.BlockCopy( text, currentByte, currentBlock, 0, byteCopyCount );
                    BigInteger openInt = new BigInteger( currentBlock );
                    BigInteger cipherInt = BigInteger.ModPow( elgamalKey.Key, k, elgamalKey.Parameters.P );
                    BigInteger.DivRem( openInt * cipherInt, elgamalKey.Parameters.P, out cipherInt );

                    byte[] cipherBlock = cipherInt.ToByteArray( false );
                    packedBlock = new byte[ writeSize ];
                    Buffer.BlockCopy( cipherBlock, 0, packedBlock, 0, cipherBlock.Length );
                    result.AddRange( packedBlock );

                }


            }
            catch( Exception ex )
            {
                throw new Exception( "Ошибка! Проверьте входные параметры.", ex );
            }

            return result.ToArray();
        }

        public void Encrypt( string inputPath, string outputPath, IKey key, Action<double> ProgressChanged = null )
        {
            if( key is ElgamalKey == false )
            {
                throw new ArgumentException( "Key is not ElgamalKey" );
            }

            ElgamalKey elgamalKey = key as ElgamalKey;

            if( elgamalKey.IsPrivate )
            {
                throw new ArgumentException( "Для шифрования нужен публичный ключ" );
            }

            try
            {


                using FileStream instream = File.OpenRead( inputPath );
                using FileStream outstream = File.Open( outputPath, FileMode.Create );

                long fileSize = instream.Length;

                int readSize = elgamalKey.MaxOpenTextSize * 1024;

                byte[] inputChunk = new byte[ readSize ];
                int size = 0;

                while( ( size = instream.Read( inputChunk, 0, readSize ) ) != 0 )
                {

                    byte[] currentBlock;
                    if( size == readSize )
                    {
                        currentBlock = inputChunk;
                    }
                    else
                    {
                        currentBlock = new byte[ size ];
                        Buffer.BlockCopy( inputChunk, 0, currentBlock, 0, size );
                    }
                    byte[] outputChunk = Encrypt( currentBlock, elgamalKey );
                    outstream.Write( outputChunk, 0, outputChunk.Length );

                    int percent = ( int )( ( instream.Position * 100f ) / fileSize );
                    ProgressChanged?.Invoke( percent );
                }
            }
            catch( Exception ex)
            {

                throw new Exception( "Ошибка! Проверьте входные параметры.", ex );
            }
        }

        private void Validate( byte[] input, IKey key )
        {
            if( input == null )
            {
                throw new ArgumentNullException( "Пустые входные данные" );
            }
            if( input.Length == 0 )
            {
                throw new ArgumentNullException( "Пустые входные данные" );
            }
            if( key is ElgamalKey == false )
            {
                throw new ArgumentException( "Неверный ключ" );
            }
        }
    }
}
