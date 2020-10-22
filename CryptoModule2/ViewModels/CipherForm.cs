using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CryptoModule2.ViewModels.Interfaces;

namespace CryptoModule2.ViewModels
{
    public class CipherForm
    {
        public Page CipherPage { get; private set; }
        public string CipherName { get; }
        public CipherForm( Page page, string cipherName )
        {
            if( page.DataContext == null )
            {
                throw new ArgumentNullException( "There is no data context" );
            }
            if( !( page.DataContext is ICipherVM vm) )
            {
                throw new ArgumentException( "DataContext doesn't implement ICipherVM" );
            }
            CipherPage = page;
            CipherName = cipherName;
        }

        public void ClearForm() => ( CipherPage.DataContext as ICipherVM ).ClearForm(); 
    }
}
