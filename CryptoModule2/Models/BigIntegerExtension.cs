﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoModule2.Models
{
    public static class BigIntegerExtension
    {
        public static byte[] ToByteArray( this BigInteger bigInteger, bool isSign )
        {
            if( isSign )
            {
                return bigInteger.ToByteArray();
            }
            else
            {
                List<byte> byteArray = new List<byte>( bigInteger.ToByteArray() );
                while( byteArray.Count > 1 && byteArray[ byteArray.Count - 1 ] == 0 )
                {
                    byteArray.RemoveAt( byteArray.Count - 1 );
                }
                return byteArray.ToArray();

            }

        }


        /// <summary>
        /// Тест Миллера — Рабина на простоту 
        /// </summary>
        /// <param name="n">Тестируемое число</param>
        /// <param name="testCount">Колличество итераций проверки</param>
        /// <returns></returns>        
        public static bool IsPrime( this BigInteger n, uint testCount = 40 )
        {
            if( testCount < 10 )
            {
                throw new ArgumentException( "test count must be greater 10" );
            }

            if( n.Sign != 1 )
            {
                throw new ArgumentException( "n must be positive" );
            }

            if( n == 2 )
            {
                return true;
            }
            if( n.IsEven )
            {
                return false;
            }


            ulong s = 0;
            BigInteger t = n - 1;

            while( t.IsEven )
            {
                s++;

                t = t >> 1;
            }

            for( uint i = 0; i < testCount; i++ )
            {
                BigInteger a = Helper.GenerateBigInteger( 2, n - 2 );
                BigInteger x = BigInteger.ModPow( a, t, n );
                if( ( x == BigInteger.One ) || ( x == ( n - 1 ) ) )
                {
                    continue;
                }
                for( ulong j = 0; j < ( s - 1 ); j++ )
                {
                    x = BigInteger.ModPow( x, 2, n );
                    if( x == BigInteger.One )
                    {
                        return false;
                    }
                    if( x == ( n - 1 ) )
                    {
                        break;
                    }

                }
                if( x != (n - 1) )
                {
                    return false;
                }
            }

            return true;
        }

        public static BigInteger Inverse( this BigInteger a, BigInteger module )
        {
            var result = Helper.GCDex( a, module );
            if( result.d != BigInteger.One )
            {
                throw new ApplicationException( "Обратный элемент не найден" );
            }

            BigInteger.DivRem( result.x, module, out result.x );
            if( result.x.Sign == -1 )
            {
                result.x += module;
                BigInteger.DivRem( result.x, module, out result.x );
            }
            return result.x;
        }




    }


}
