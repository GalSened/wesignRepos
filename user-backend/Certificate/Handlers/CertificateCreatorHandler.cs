using Certificate.Interfaces;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Certificate.Handlers
{
    public class CertificateCreatorHandler : ICertificateCreator
    {

        const string issuerInfo = "cn=WESIGN3-CA dc=comda dc=co dc=il";
        public byte[] Create(string dn, string password)
        {
            string subjectDn = dn;

            

            RsaKeyPairGenerator rsaKeyGenerator = new RsaKeyPairGenerator();
            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), 2048);
            rsaKeyGenerator.Init(keyGenerationParameters);
            AsymmetricCipherKeyPair pair = rsaKeyGenerator.GenerateKeyPair();

            X509Name subjectName = new X509Name(true, subjectDn);
            X509Name issuerName = new X509Name(true, issuerInfo);

            X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
            certGenerator.SetSerialNumber(BigInteger.ValueOf(DateTime.Now.Ticks));
     
            certGenerator.SetIssuerDN(issuerName);
            certGenerator.SetNotBefore(DateTime.UtcNow);
            certGenerator.SetNotAfter(DateTime.UtcNow.AddYears(4));
            certGenerator.SetSubjectDN(subjectName);
            certGenerator.SetPublicKey(pair.Public);

            certGenerator.AddExtension(X509Extensions.BasicConstraints, true,
                    new BasicConstraints(false));
            certGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(
                    KeyUsage.NonRepudiation));

            

            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", pair.Private, new SecureRandom());

            X509Certificate certificate = certGenerator.Generate(signatureFactory);

            X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
            

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();


            IDictionary attributes = new Dictionary<string, Asn1Encodable>()
            {
                ["1.3.6.1.4.1.311.17.1"] = new DerBmpString("Microsoft Enhanced RSA and AES Cryptographic Provider")
            };

            AsymmetricKeyEntry keyEntry = new AsymmetricKeyEntry(pair.Private, attributes);

            store.SetKeyEntry("private", keyEntry, new X509CertificateEntry[] { certEntry });


            using (MemoryStream stream = new MemoryStream())
            {
                store.Save(stream, password.ToCharArray(), new SecureRandom());

                var pfxBytes = stream.ToArray();
                return pfxBytes;
            }
        }
    }
}
