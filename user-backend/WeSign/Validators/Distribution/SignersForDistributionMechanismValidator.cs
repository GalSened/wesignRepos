using Common.Enums;
using Common.Extensions;
using Common.Interfaces;
using FluentValidation;
using FluentValidation.Validators;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Distribution.Requests;

namespace WeSign.Validators.Distribution
{
    public class SignersForDistributionMechanismValidator : AbstractValidator<SignersForDistributionMechanismDTO>
    {
        private readonly IDataUriScheme _dataUriScheme;
        private Workbook _workbook;

        public SignersForDistributionMechanismValidator(IDataUriScheme dataUriScheme)
        {
            _dataUriScheme = dataUriScheme;
            RuleFor(x => x.Base64File)
                .NotEmpty()
                .Must(BeValidExcelFile)
                .WithMessage("Supported FileType are: xlsx, xls. Please specify a valid Base64File in format data:application/vnd.ms-excel;base64,.... or data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,.... ")
                .Must(BeValidWorksheetsCount)
                .WithMessage("Excel file should contain worksheet")
                .Must(BeValidWorksheetsRows)
                .WithMessage("Excel file should at least 2 rows, while first row should be the titles")
                .Custom(BeValidSignerInput);
        }

        private void BeValidSignerInput(string base64File, ValidationContext<SignersForDistributionMechanismDTO> customContext)
        {
            try
            {
                for (int j = 1; j < _workbook.Worksheets[0].Rows.Length; j++)
                {
                    var signerDataFromExcel = _workbook.Worksheets[0].Rows[j];
                    if (signerDataFromExcel.CellList.Count > 1)
                    {
                        string firstName = signerDataFromExcel.CellList[0].DisplayedText.Trim();
                        string lastName = signerDataFromExcel.CellList[1].DisplayedText.Trim();
                        string originalSignerMeans = signerDataFromExcel.CellList[2].DisplayedText.Trim();
                        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(originalSignerMeans))
                        {
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(originalSignerMeans))
                        {
                            customContext.AddFailure($"Invalid signer name - name cannot be empty, Excel row - [{j + 1}]");
                        }

                        string updateSignerMeans = originalSignerMeans;
                        if (ContactsExtenstions.IsValidPhone(originalSignerMeans.Replace("-", "")))
                        {
                            updateSignerMeans = originalSignerMeans.Replace("-", "");
                        }
                        if (!ContactsExtenstions.IsValidPhone(updateSignerMeans) && !ContactsExtenstions.IsValidEmail(updateSignerMeans))
                        {
                            customContext.AddFailure($"Invalid signer means - [{originalSignerMeans}] for signer name [{signerDataFromExcel.CellList[0].DisplayedText} {signerDataFromExcel.CellList[1].DisplayedText}], Excel row - [{j + 1}]");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                customContext.AddFailure($"Invalid signer means. {ex}");
            }
        }

        //private void BeValidSignerInput(string base64File, CustomContext customContext)
        //{
        //    try
        //    {
        //        for (int j = 1; j < _workbook.Worksheets[0].Rows.Length; j++)
        //        {
        //            var signerDataFromExcel = _workbook.Worksheets[0].Rows[j];
        //            if (signerDataFromExcel.CellList.Count > 1)
        //            {
        //                string firstName = signerDataFromExcel.CellList[0].DisplayedText.Trim();
        //                string lastName = signerDataFromExcel.CellList[1].DisplayedText.Trim();
        //                string originalSignerMeans = signerDataFromExcel.CellList[2].DisplayedText.Trim();
        //                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(originalSignerMeans))
        //                {
        //                    continue;
        //                }
        //                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName) && !string.IsNullOrWhiteSpace(originalSignerMeans))
        //                {
        //                    customContext.AddFailure($"Invalid signer name - name cannot be empty, Excel row - [{j + 1}]");
        //                }

            //                string updateSignerMeans = originalSignerMeans;
            //                if (ContactsExtenstions.IsValidPhone(originalSignerMeans.Replace("-", "")))
            //                {
            //                    updateSignerMeans = originalSignerMeans.Replace("-", "");
            //                }
            //                if (!ContactsExtenstions.IsValidPhone(updateSignerMeans) && !ContactsExtenstions.IsValidEmail(updateSignerMeans))
            //                {
            //                    customContext.AddFailure($"Invalid signer means - [{originalSignerMeans}] for signer name [{signerDataFromExcel.CellList[0].DisplayedText} {signerDataFromExcel.CellList[1].DisplayedText}], Excel row - [{j + 1}]");
            //                }
            //            }
            //        }                

            //    }
            //    catch (Exception ex)
            //    {
            //        customContext.AddFailure($"Invalid signer means. {ex}");
            //    }
            //}

        private bool BeValidWorksheetsRows(string base64File)
        {
            try
            {                
                return _workbook.Worksheets[0].Rows.Length > 1;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool BeValidWorksheetsCount(string base64File)
        {
            try
            {
                var byteArray = _dataUriScheme.GetBytes(base64File);
                Stream stream = new MemoryStream(byteArray);
                _workbook = new Workbook();
                _workbook.LoadFromStream(stream);

                return _workbook.Worksheets.Count != 0;
            }
            catch
            {
                return false;
            }
        }

        private bool BeValidExcelFile(string base64)
        {
            try
            {
                string content = _dataUriScheme.Getbase64Content(base64);
                bool isVaild = _dataUriScheme.IsValidFileType(base64, out FileType fileType) && (fileType == FileType.XLSX || fileType == FileType.XLS) ;
                return !string.IsNullOrEmpty(content) && isVaild;
            }
            catch 
            {
                return false;
            }
        }
    }
}
