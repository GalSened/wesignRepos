using Common.Enums;
using Common.Models;
using Common.Models.Configurations;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISignConnector
    {
        Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdfField(string certId, byte[] inputFile, string fieldName, string pincode, byte[] image, string token, CompanySigner1Details companySigner1Details);
        Task<Signer1ResCode> VerifyCredential(string certId, string pinCode,string token, CompanySigner1Details companySigner1Details);
        //Signer1ResCode VerifyCredential(string certId, string pinCode, CompanySigner1Details companySigner1Details = null);

        Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignXML(string certId, byte[] inputFile,  string pincode, string token, CompanySigner1Details companySigner1Details);

        Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignWord(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details);
        Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignExcel(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details);

        Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdf(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details);

        //(Signer1ResCode ResultCode, byte[] SignedBytes) SignTiff(string certId, byte[] inputFile, string pincode, byte[] image, string Token);
        //(Signer1ResCode ResultCode, byte[] SignedBytes) SignXMLForeclosure(string certId, byte[] inputFile, string pincode, string token);
    }
}
