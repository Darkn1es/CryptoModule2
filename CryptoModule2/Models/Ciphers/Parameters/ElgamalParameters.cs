using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CryptoModule2.Models.Ciphers.Parameters
{
    public class ElgamalParameters
    {
        public BigInteger P { get; }
        public BigInteger G { get; }

        private ElgamalParameters(BigInteger p, BigInteger g)
        {
            P = p;
            G = g;
        }

        public static ElgamalParameters Generate( int decimalNubmerCount )
        {
            DHParameters temp = DHParameters.Generate( decimalNubmerCount ); // Нужно бы алгоритм генерации P, G вынести в отдельный класс
            return new ElgamalParameters( temp.P, temp.G );
        }
    }
}
