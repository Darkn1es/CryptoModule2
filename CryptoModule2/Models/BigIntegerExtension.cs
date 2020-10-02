using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

    }


}
