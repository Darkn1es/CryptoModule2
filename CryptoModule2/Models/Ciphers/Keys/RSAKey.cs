using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoModule2.Models.Ciphers.Keys
{
    public class RSAKey : IKey
    {
        public BigInteger Modulus { get; private set; } = BigInteger.Zero;
        public BigInteger Exponent { get; private set; } = BigInteger.Zero;
        public bool IsPrivate { get; private set; } = false;

        public readonly int MaxOpenTextSize;
        public readonly int MaxCipherTextSize;

        public RSAKey( bool isPrivate, string exponent, string modulus) : 
            this(isPrivate, BigInteger.Parse(exponent), BigInteger.Parse(modulus))
        {

        }

        public RSAKey( bool isPrivate, BigInteger exponent, BigInteger modulus )
        {
            IsPrivate = isPrivate;

            if( exponent == null )
            {
                throw new ArgumentNullException( "Exponent is null" );
            }
            if( modulus == null )
            {
                throw new ArgumentNullException( "Modulus is null" );
            }

            if( exponent <= BigInteger.One )
            {
                throw new ArgumentException( "Экспонента должна быть больше единицы" );
            }
            if( modulus <= 255 )
            {
                throw new ArgumentException( "Модуль должна быть больше 255" );
            }
            if( exponent >= modulus )
            {
                BigInteger.DivRem( exponent, modulus, out exponent );
            }
            Exponent = exponent;
            Modulus = modulus;

            int modulusByteCount = Modulus.ToByteArray( false ).Length;

            MaxOpenTextSize = modulusByteCount - 1;
            MaxCipherTextSize = modulusByteCount;
        }


    }
}
