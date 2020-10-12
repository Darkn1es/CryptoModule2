using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoModule2.Models
{
    public static class Helper
    {

        public static BigInteger GenerateBigInteger( BigInteger min, BigInteger max )
        {
            if( min >= max )
            {
                throw new ArgumentException( "Min >= max" );
            }
            if( min.Sign != 1 )
            {
                throw new ArgumentException( "min must be positive" );
            }
            if( max.Sign != 1 )
            {
                throw new ArgumentException( "max must be positive" );
            }

            BigInteger diff = max - min;
            int byteSize = diff.ToByteArray().Length;

            byte[] newBigIntergerChunk = new byte[ byteSize + 1 ]; //  Последний байт всегда 0х00 для положительного числа

            RNGCryptoServiceProvider rngGenerator = new RNGCryptoServiceProvider();
            rngGenerator.GetBytes( newBigIntergerChunk );

            newBigIntergerChunk[ newBigIntergerChunk.Length - 1 ] = 0;


            BigInteger result = new BigInteger( newBigIntergerChunk );

            BigInteger.DivRem( result, diff, out result );

            result += min;

            return result;

        }

        public static BigInteger GeneratePrime( int decimalNubmerCount )
        {
            if( decimalNubmerCount < 6 )
            {
                throw new ArgumentException( "Слишком маленький порядок числа" );
            }

            BigInteger min = BigInteger.Pow( 10, decimalNubmerCount );
            BigInteger max = BigInteger.Pow( 10, decimalNubmerCount + 1 );

            BigInteger p;

            while( true ) // Надо ограничть макс значением
            {
                p = Helper.GenerateBigInteger( min, max );
                if( p.IsPrime() )
                {
                    break;
                }
            }

            return p;
        }

        public static (BigInteger d, BigInteger x, BigInteger y) GCDex( BigInteger a, BigInteger b )
        {
            BigInteger x = BigInteger.Zero;
            BigInteger y = BigInteger.Zero;

            BigInteger x1 = BigInteger.Zero;
            BigInteger x2 = BigInteger.One;
            BigInteger y1 = BigInteger.One;
            BigInteger y2 = BigInteger.Zero;

            BigInteger q = BigInteger.Zero;
            BigInteger r = BigInteger.Zero;



            while( b > BigInteger.Zero )
            {
                q = BigInteger.DivRem( a, b, out r );
                x = x2 - q * x1;
                y = y2 - q * y2;

                a = b;
                b = r;

                x2 = x1;
                x1 = x;
                y2 = y1;
                y1 = y;
            }

            return (a, x2, y2);

        }


    }
}
