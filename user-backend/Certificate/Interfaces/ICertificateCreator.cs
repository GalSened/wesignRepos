using System;
using System.Collections.Generic;
using System.Text;

namespace Certificate.Interfaces
{
   
    public interface ICertificateCreator
    {
        byte[] Create(string dn, string password);
        

    }
}
