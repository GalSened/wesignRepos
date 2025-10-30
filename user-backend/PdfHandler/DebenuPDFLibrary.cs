namespace PdfHandler
{
    using Common.Interfaces.PDF;
    using DebenuPDFLibraryDLL1711;
    using Microsoft.AspNetCore.Hosting;
    using PdfHandler.Interfaces;
    using Serilog;
    using System;
    using System.IO.Abstractions;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    public class DebenuPDFLibrary : IDebenuPdfLibrary, IDisposable
    {
        private readonly int _instanceID = 0;
        //private DebenuNativeMethods _dll = null;
        private DLL _dll = null;

        private string SR(IntPtr data)
        {
            int size = _dll.DebenuPDFLibraryStringResultLength(_instanceID);
            byte[] result = new byte[size * 2];
            Marshal.Copy(data, result, 0, size * 2);
            return Encoding.Unicode.GetString(result);
        }

        private bool LoadLibrary()
        {
            if (LibraryLoaded())
            {
                //string licenseKeyVersion11 = "j99sg9g441z9e58b89sa91j4y";
                string licenseKeyVersion17 = "j44kt6wr31g67686d8w79rj9y";

                UnlockKey(licenseKeyVersion17);
                if (Unlocked() != 0)
                {
                    return true;
                }
                ReleaseLibrary();
            }
            return false;
        }

        public DebenuPDFLibrary(IHostingEnvironment env, IFileSystem fileSystem, ILogger logger, string dllFileName = "DebenuPDFLibrary64DLL1711.dll")
        {
            dllFileName = Environment.Is64BitOperatingSystem ? dllFileName : "DebenuPDFLibraryDLL1711.dll";

            if (env.IsDevelopment() || env.EnvironmentName == "DevLite")
            {
                var parts = RuntimeInformation.FrameworkDescription.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string version = "3.1";

                if (parts != null && parts.Length > 0)
                {
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (Char.IsNumber(parts[i][0]))
                        {
                            version = parts[i];
                            break;
                        }
                    }                   
                    if (version[0] == '4')
                    {
                        version = GetNetCoreVersion();
                    }
                    version = version.Substring(0, 3);
                }

                dllFileName = fileSystem.Path.Combine(env.ContentRootPath,
                                                @"bin\Debug\net" + version + @"\ExternalDLLs\" + dllFileName);

                //var productName = String.Join(" ", parts, 0, i);
                //string a = String.Join(" ", productName, " ", GetNetCoreVersion());

                //dllFileName = fileSystem.Path.Combine(env.ContentRootPath,
                //                                      @"bin\Debug\netcoreapp3.1\ExternalDLLs\" + dllFileName);
            }
            if (env.IsProduction())
            {
                dllFileName = fileSystem.Path.Combine(env.ContentRootPath, @"ExternalDLLs\" + dllFileName);
            }

            _dll = new DLL(dllFileName);
            if (_dll.dllHandle != IntPtr.Zero)
            {
                _instanceID = _dll.DebenuPDFLibraryCreateLibrary();
                _dll.RegisterForShutdown(_instanceID);
                LoadLibrary();
            }
            else
            {
                if (dllFileName.Contains("DebenuPDFLibraryDLL1711"))
                {
                    dllFileName = dllFileName.Replace("DebenuPDFLibraryDLL1711", "DebenuPDFLibrary64DLL1711");
                    _dll = new DLL(dllFileName);
                    if (_dll.dllHandle != IntPtr.Zero)
                    {
                        _instanceID = _dll.DebenuPDFLibraryCreateLibrary();
                        _dll.RegisterForShutdown(_instanceID);
                        LoadLibrary();
                    }
                    else
                    {
                        logger.Error("DebenuPDFLibrary - Failed While Load Internal DLL. dllFileName = [{DllFileName}] ", dllFileName);
                        _dll = null;
                    }
                }
                else if (dllFileName.Contains("DebenuPDFLibrary64DLL1711"))
                {
                    dllFileName = dllFileName.Replace("DebenuPDFLibrary64DLL1711", "DebenuPDFLibraryDLL1711");
                    _dll = new DLL(dllFileName);
                    if (_dll.dllHandle != IntPtr.Zero)
                    {
                        _instanceID = _dll.DebenuPDFLibraryCreateLibrary();
                        _dll.RegisterForShutdown(_instanceID);
                        LoadLibrary();
                    }
                    else
                    {
                        logger.Error("DebenuPDFLibrary - Failed While Load Internal DLL. dllFileName = [{DllFileName}] ", dllFileName);
                        _dll = null;
                    }

                }
                
            }
        }

        private string GetNetCoreVersion()
        {
            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
                return assemblyPath[netCoreAppIndex + 1];
            return null;
        }

        public void Dispose()
        {
            ReleaseLibrary();
            GC.SuppressFinalize(this);
        }

        public bool LibraryLoaded()
        {
            return _dll != null;
        }

        public void ReleaseLibrary()
        {
            if (_dll != null)
            {
                
                _dll.Release();
            }

            _dll = null;
        }


        public int AddArcToPath(double CenterX, double CenterY, double TotalAngle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddArcToPath(_instanceID, CenterX, CenterY, TotalAngle);
        }

        public int AddBoxToPath(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddBoxToPath(_instanceID, Left, Top, Width, Height);
        }

        public int AddCJKFont(int CJKFontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddCJKFont(_instanceID, CJKFontID);
        }

        public int AddCurveToPath(double CtAX, double CtAY, double CtBX, double CtBY, double EndX,
            double EndY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddCurveToPath(_instanceID, CtAX, CtAY, CtBX, CtBY, EndX,
                    EndY);
        }

        public int AddEmbeddedFile(string FileName, string MIMEType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddEmbeddedFile(_instanceID, FileName, MIMEType);
        }

        public int AddFileAttachment(string Title, int EmbeddedFileID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFileAttachment(_instanceID, Title, EmbeddedFileID);
        }

        public int AddFormFieldChoiceSub(int Index, string SubName, string DisplayName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFormFieldChoiceSub(_instanceID, Index, SubName,
                    DisplayName);
        }

        public int AddFormFieldSub(int Index, string SubName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFormFieldSub(_instanceID, Index, SubName);
        }

        public int AddFormFieldSubEx(int Index, string SubName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFormFieldSubEx(_instanceID, Index, SubName, Options);
        }

        public int AddFormFont(int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFormFont(_instanceID, FontID);
        }

        public int AddFreeTextAnnotation(double Left, double Top, double Width, double Height,
            string Text, int Angle, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFreeTextAnnotation(_instanceID, Left, Top, Width,
                    Height, Text, Angle, Options);
        }

        public int AddFreeTextAnnotationEx(double Left, double Top, double Width, double Height,
            string Text, int Angle, int Options, int Transparency)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddFreeTextAnnotationEx(_instanceID, Left, Top, Width,
                    Height, Text, Angle, Options, Transparency);
        }

        public int AddGlobalJavaScript(string PackageName, string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddGlobalJavaScript(_instanceID, PackageName, JavaScript);
        }

        public int AddImageFromFile(string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddImageFromFile(_instanceID, FileName, Options);
        }

        public int AddImageFromFileOffset(string FileName, int Offset, int DataLength, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddImageFromFileOffset(_instanceID, FileName, Offset,
                    DataLength, Options);
        }

        public int AddImageFromString(byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryAddImageFromString(_instanceID, bufferID, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int AddLGIDictToPage(string DictContent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLGIDictToPage(_instanceID, DictContent);
        }

        public int AddLineToPath(double EndX, double EndY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLineToPath(_instanceID, EndX, EndY);
        }

        public int AddLinkToDestination(double Left, double Top, double Width, double Height,
            int DestID, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToDestination(_instanceID, Left, Top, Width, Height,
                    DestID, Options);
        }

        public int AddLinkToEmbeddedFile(double Left, double Top, double Width, double Height,
            int EmbeddedFileID, string Title, string Contents, int IconType, int Transpareny)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToEmbeddedFile(_instanceID, Left, Top, Width,
                    Height, EmbeddedFileID, Title, Contents, IconType, Transpareny);
        }

        public int AddLinkToFile(double Left, double Top, double Width, double Height,
            string FileName, int Page, double Position, int NewWindow, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToFile(_instanceID, Left, Top, Width, Height,
                    FileName, Page, Position, NewWindow, Options);
        }

        public int AddLinkToFileDest(double Left, double Top, double Width, double Height,
            string FileName, string NamedDest, double Position, int NewWindow, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToFileDest(_instanceID, Left, Top, Width, Height,
                    FileName, NamedDest, Position, NewWindow, Options);
        }

        public int AddLinkToFileEx(double Left, double Top, double Width, double Height,
            string FileName, int DestPage, int NewWindow, int Options, int Zoom, int DestType,
            double DestLeft, double DestTop, double DestRight, double DestBottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToFileEx(_instanceID, Left, Top, Width, Height,
                    FileName, DestPage, NewWindow, Options, Zoom, DestType, DestLeft, DestTop,
                    DestRight, DestBottom);
        }

        public int AddLinkToJavaScript(double Left, double Top, double Width, double Height,
            string JavaScript, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToJavaScript(_instanceID, Left, Top, Width, Height,
                    JavaScript, Options);
        }

        public int AddLinkToLocalFile(double Left, double Top, double Width, double Height,
            string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToLocalFile(_instanceID, Left, Top, Width, Height,
                    FileName, Options);
        }

        public int AddLinkToPage(double Left, double Top, double Width, double Height, int Page,
            double Position, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToPage(_instanceID, Left, Top, Width, Height, Page,
                    Position, Options);
        }

        public int AddLinkToWeb(double Left, double Top, double Width, double Height, string Link,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddLinkToWeb(_instanceID, Left, Top, Width, Height, Link,
                    Options);
        }

        public int AddNoteAnnotation(double Left, double Top, int AnnotType, double PopupLeft,
            double PopupTop, double PopupWidth, double PopupHeight, string Title, string Contents,
            double Red, double Green, double Blue, int Open)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddNoteAnnotation(_instanceID, Left, Top, AnnotType,
                    PopupLeft, PopupTop, PopupWidth, PopupHeight, Title, Contents, Red, Green, Blue,
                    Open);
        }

        public int AddOpenTypeFontFromFile(string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddOpenTypeFontFromFile(_instanceID, FileName, Options);
        }

        public int AddOpenTypeFontFromString(byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryAddOpenTypeFontFromString(_instanceID, bufferID,
                    Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int AddPageLabels(int Start, int Style, int Offset, string Prefix)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddPageLabels(_instanceID, Start, Style, Offset, Prefix);
        }

        public int AddPageMatrix(double xscale, double yscale, double xoffset, double yoffset)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddPageMatrix(_instanceID, xscale, yscale, xoffset,
                    yoffset);
        }

        public int AddRGBSeparationColor(string ColorName, double Red, double Green, double Blue,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddRGBSeparationColor(_instanceID, ColorName, Red, Green,
                    Blue, Options);
        }

        public int AddRelativeLinkToFile(double Left, double Top, double Width, double Height,
            string FileName, int Page, double Position, int NewWindow, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddRelativeLinkToFile(_instanceID, Left, Top, Width,
                    Height, FileName, Page, Position, NewWindow, Options);
        }

        public int AddRelativeLinkToFileDest(double Left, double Top, double Width, double Height,
            string FileName, string NamedDest, double Position, int NewWindow, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddRelativeLinkToFileDest(_instanceID, Left, Top, Width,
                    Height, FileName, NamedDest, Position, NewWindow, Options);
        }

        public int AddRelativeLinkToFileEx(double Left, double Top, double Width, double Height,
            string FileName, int DestPage, int NewWindow, int Options, int Zoom, int DestType,
            double DestLeft, double DestTop, double DestRight, double DestBottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddRelativeLinkToFileEx(_instanceID, Left, Top, Width,
                    Height, FileName, DestPage, NewWindow, Options, Zoom, DestType, DestLeft,
                    DestTop, DestRight, DestBottom);
        }

        public int AddRelativeLinkToLocalFile(double Left, double Top, double Width, double Height,
            string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddRelativeLinkToLocalFile(_instanceID, Left, Top, Width,
                    Height, FileName, Options);
        }

        public int AddSVGAnnotationFromFile(double Left, double Top, double Width, double Height,
            string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddSVGAnnotationFromFile(_instanceID, Left, Top, Width,
                    Height, FileName, Options);
        }

        public int AddSWFAnnotationFromFile(double Left, double Top, double Width, double Height,
            string FileName, string Title, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddSWFAnnotationFromFile(_instanceID, Left, Top, Width,
                    Height, FileName, Title, Options);
        }

        public int AddSeparationColor(string ColorName, double C, double M, double Y, double K,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddSeparationColor(_instanceID, ColorName, C, M, Y, K,
                    Options);
        }

        public int AddSignProcessOverlayText(int SignProcessID, double XPos, double YPos,
            double TextSize, string LayerName, string OverlayText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddSignProcessOverlayText(_instanceID, SignProcessID, XPos,
                    YPos, TextSize, LayerName, OverlayText);
        }

        public int AddStampAnnotation(double Left, double Top, double Width, double Height,
            int StampType, string Title, string Contents, double Red, double Green, double Blue,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddStampAnnotation(_instanceID, Left, Top, Width, Height,
                    StampType, Title, Contents, Red, Green, Blue, Options);
        }

        public int AddStampAnnotationFromImage(double Left, double Top, double Width, double Height,
            string FileName, string Title, string Contents, double Red, double Green, double Blue,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddStampAnnotationFromImage(_instanceID, Left, Top, Width,
                    Height, FileName, Title, Contents, Red, Green, Blue, Options);
        }

        public int AddStampAnnotationFromImageID(double Left, double Top, double Width,
            double Height, int ImageID, string Title, string Contents, double Red, double Green,
            double Blue, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddStampAnnotationFromImageID(_instanceID, Left, Top,
                    Width, Height, ImageID, Title, Contents, Red, Green, Blue, Options);
        }

        public int AddStandardFont(int StandardFontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddStandardFont(_instanceID, StandardFontID);
        }

        public int AddSubsettedFont(string FontName, int CharsetIndex, string SubsetChars)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddSubsettedFont(_instanceID, FontName, CharsetIndex,
                    SubsetChars);
        }

        public int AddTextMarkupAnnotation(int MarkupType, double Left, double Top, double Width,
            double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTextMarkupAnnotation(_instanceID, MarkupType, Left, Top,
                    Width, Height);
        }

        public int AddToFileList(string ListName, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddToFileList(_instanceID, ListName, FileName);
        }

        public int AddToUnicodeFontGroup(string FontGroupName, int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddToUnicodeFontGroup(_instanceID, FontGroupName, FontID);
        }

        public int AddTrueTypeFont(string FontName, int Embed)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTrueTypeFont(_instanceID, FontName, Embed);
        }

        public int AddTrueTypeFontFromFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTrueTypeFontFromFile(_instanceID, FileName);
        }

        public int AddTrueTypeFontFromFileEx(string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTrueTypeFontFromFileEx(_instanceID, FileName, Options);
        }

        public int AddTrueTypeFontFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryAddTrueTypeFontFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int AddTrueTypeFontFromStringEx(byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryAddTrueTypeFontFromStringEx(_instanceID, bufferID,
                    Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int AddTrueTypeSubsettedFont(string FontName, string SubsetChars, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTrueTypeSubsettedFont(_instanceID, FontName,
                    SubsetChars, Options);
        }

        public int AddTrueTypeSubsettedFontFromFile(string FileName, string SubsetChars, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddTrueTypeSubsettedFontFromFile(_instanceID, FileName,
                    SubsetChars, Options);
        }

        public int AddTrueTypeSubsettedFontFromString(byte[] Source, string SubsetChars, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryAddTrueTypeSubsettedFontFromString(_instanceID,
                    bufferID, SubsetChars, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int AddType1Font(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddType1Font(_instanceID, FileName);
        }

        public int AddU3DAnnotationFromFile(double Left, double Top, double Width, double Height,
            string FileName, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddU3DAnnotationFromFile(_instanceID, Left, Top, Width,
                    Height, FileName, Options);
        }

        public int AddUnicodeFont(string FontName, int EncodingOptions, int EmbedOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddUnicodeFont(_instanceID, FontName, EncodingOptions,
                    EmbedOptions);
        }

        public int AddUnicodeFontFromFile(string FontFileName, int EncodingOptions, int EmbedOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAddUnicodeFontFromFile(_instanceID, FontFileName,
                    EncodingOptions, EmbedOptions);
        }

        public int AnalyseFile(string InputFileName, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAnalyseFile(_instanceID, InputFileName, Password);
        }

        public int AnnotationCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAnnotationCount(_instanceID);
        }

        public int AnsiStringResultLength()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
        }

        public int AppendSpace(double RelativeSpace)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAppendSpace(_instanceID, RelativeSpace);
        }

        public int AppendTableColumns(int TableID, int NewColumnCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAppendTableColumns(_instanceID, TableID, NewColumnCount);
        }

        public int AppendTableRows(int TableID, int NewRowCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAppendTableRows(_instanceID, TableID, NewRowCount);
        }

        public int AppendText(string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAppendText(_instanceID, Text);
        }

        public int AppendToFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAppendToFile(_instanceID, FileName);
        }

        public byte[] AppendToString(int AppendMode)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryAppendToString(_instanceID, AppendMode);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int ApplyStyle(string StyleName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryApplyStyle(_instanceID, StyleName);
        }

        public int AttachAnnotToForm(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryAttachAnnotToForm(_instanceID, Index);
        }

        public int BalanceContentStream()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryBalanceContentStream(_instanceID);
        }

        public int BalancePageTree(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryBalancePageTree(_instanceID, Options);
        }

        public int BeginPageUpdate()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryBeginPageUpdate(_instanceID);
        }

        public int CapturePage(int Page)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCapturePage(_instanceID, Page);
        }

        public int CapturePageEx(int Page, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCapturePageEx(_instanceID, Page, Options);
        }

        public int CharWidth(int CharCode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCharWidth(_instanceID, CharCode);
        }

        public int CheckFileCompliance(string InputFileName, string Password, int ComplianceTest,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCheckFileCompliance(_instanceID, InputFileName, Password,
                    ComplianceTest, Options);
        }

        public int CheckObjects()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCheckObjects(_instanceID);
        }

        public int CheckPageAnnots()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCheckPageAnnots(_instanceID);
        }

        public int CheckPassword(string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCheckPassword(_instanceID, Password);
        }

        public int ClearFileList(string ListName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClearFileList(_instanceID, ListName);
        }

        public int ClearImage(int ImageID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClearImage(_instanceID, ImageID);
        }

        public int ClearPageLabels()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClearPageLabels(_instanceID);
        }

        public int ClearTextFormatting()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClearTextFormatting(_instanceID);
        }

        public int CloneOutlineAction(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCloneOutlineAction(_instanceID, OutlineID);
        }

        public int ClonePages(int StartPage, int EndPage, int RepeatCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClonePages(_instanceID, StartPage, EndPage, RepeatCount);
        }

        public int CloseOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCloseOutline(_instanceID, OutlineID);
        }

        public int ClosePath()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryClosePath(_instanceID);
        }

        public int CombineContentStreams()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCombineContentStreams(_instanceID);
        }

        public int CompareOutlines(int FirstOutlineID, int SecondOutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCompareOutlines(_instanceID, FirstOutlineID,
                    SecondOutlineID);
        }

        public int CompressContent()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCompressContent(_instanceID);
        }

        public int CompressFonts(int Compress)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCompressFonts(_instanceID, Compress);
        }

        public int CompressImages(int Compress)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCompressImages(_instanceID, Compress);
        }

        public int CompressPage()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCompressPage(_instanceID);
        }

        public int ContentStreamCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryContentStreamCount(_instanceID);
        }

        public int ContentStreamSafe()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryContentStreamSafe(_instanceID);
        }

        public int CopyPageRanges(int DocumentID, string RangeList)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCopyPageRanges(_instanceID, DocumentID, RangeList);
        }

        public int CopyPageRangesEx(int DocumentID, string RangeList, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCopyPageRangesEx(_instanceID, DocumentID, RangeList,
                    Options);
        }

        public int CreateNewObject()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCreateNewObject(_instanceID);
        }

        public int CreateRenderer()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCreateRenderer(_instanceID);
        }

        public int CreateTable(int RowCount, int ColumnCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryCreateTable(_instanceID, RowCount, ColumnCount);
        }

        public int DAAppendFile(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAAppendFile(_instanceID, FileHandle);
        }

        public int DACapturePage(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDACapturePage(_instanceID, FileHandle, PageRef);
        }

        public int DACapturePageEx(int FileHandle, int PageRef, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDACapturePageEx(_instanceID, FileHandle, PageRef, Options);
        }

        public int DACloseFile(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDACloseFile(_instanceID, FileHandle);
        }

        public int DADrawCapturedPage(int FileHandle, int DACaptureID, int DestPageRef,
            double PntLeft, double PntBottom, double PntWidth, double PntHeight)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDADrawCapturedPage(_instanceID, FileHandle, DACaptureID,
                    DestPageRef, PntLeft, PntBottom, PntWidth, PntHeight);
        }

        public int DADrawRotatedCapturedPage(int FileHandle, int DACaptureID, int DestPageRef,
            double PntLeft, double PntBottom, double PntWidth, double PntHeight, double Angle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDADrawRotatedCapturedPage(_instanceID, FileHandle,
                    DACaptureID, DestPageRef, PntLeft, PntBottom, PntWidth, PntHeight, Angle);
        }

        public int DAEmbedFileStreams(int FileHandle, string RootPath)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAEmbedFileStreams(_instanceID, FileHandle, RootPath);
        }

        public string DAExtractPageText(int FileHandle, int PageRef, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAExtractPageText(_instanceID, FileHandle, PageRef,
                    Options));
        }

        public int DAExtractPageTextBlocks(int FileHandle, int PageRef, int ExtractOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAExtractPageTextBlocks(_instanceID, FileHandle, PageRef,
                    ExtractOptions);
        }

        public int DAFindPage(int FileHandle, int Page)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAFindPage(_instanceID, FileHandle, Page);
        }

        public int DAGetAnnotationCount(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetAnnotationCount(_instanceID, FileHandle, PageRef);
        }

        public int DAGetFormFieldCount(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetFormFieldCount(_instanceID, FileHandle);
        }

        public string DAGetFormFieldTitle(int FileHandle, int FieldIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetFormFieldTitle(_instanceID, FileHandle, FieldIndex));
        }

        public string DAGetFormFieldValue(int FileHandle, int FieldIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetFormFieldValue(_instanceID, FileHandle, FieldIndex));
        }

        public byte[] DAGetImageDataToString(int FileHandle, int ImageListID, int ImageIndex)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryDAGetImageDataToString(_instanceID, FileHandle,
                    ImageListID, ImageIndex);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public double DAGetImageDblProperty(int FileHandle, int ImageListID, int ImageIndex,
            int PropertyID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetImageDblProperty(_instanceID, FileHandle, ImageListID,
                    ImageIndex, PropertyID);
        }

        public int DAGetImageIntProperty(int FileHandle, int ImageListID, int ImageIndex,
            int PropertyID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetImageIntProperty(_instanceID, FileHandle, ImageListID,
                    ImageIndex, PropertyID);
        }

        public int DAGetImageListCount(int FileHandle, int ImageListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetImageListCount(_instanceID, FileHandle, ImageListID);
        }

        public string DAGetInformation(int FileHandle, string Key)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetInformation(_instanceID, FileHandle, Key));
        }

        public int DAGetObjectCount(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetObjectCount(_instanceID, FileHandle);
        }

        public byte[] DAGetObjectToString(int FileHandle, int ObjectNumber)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryDAGetObjectToString(_instanceID, FileHandle,
                    ObjectNumber);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public double DAGetPageBox(int FileHandle, int PageRef, int BoxIndex, int Dimension)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageBox(_instanceID, FileHandle, PageRef, BoxIndex,
                    Dimension);
        }

        public byte[] DAGetPageContentToString(int FileHandle, int PageRef)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryDAGetPageContentToString(_instanceID, FileHandle,
                    PageRef);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int DAGetPageCount(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageCount(_instanceID, FileHandle);
        }

        public double DAGetPageHeight(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageHeight(_instanceID, FileHandle, PageRef);
        }

        public int DAGetPageImageList(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageImageList(_instanceID, FileHandle, PageRef);
        }

        public int DAGetPageLayout(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageLayout(_instanceID, FileHandle);
        }

        public int DAGetPageMode(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageMode(_instanceID, FileHandle);
        }

        public double DAGetPageWidth(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetPageWidth(_instanceID, FileHandle, PageRef);
        }

        public string DAGetTextBlockAsString(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetTextBlockAsString(_instanceID, TextBlockListID,
                    Index));
        }

        public double DAGetTextBlockBound(int TextBlockListID, int Index, int BoundIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockBound(_instanceID, TextBlockListID, Index,
                    BoundIndex);
        }

        public double DAGetTextBlockCharWidth(int TextBlockListID, int Index, int CharIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockCharWidth(_instanceID, TextBlockListID,
                    Index, CharIndex);
        }

        public double DAGetTextBlockColor(int TextBlockListID, int Index, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockColor(_instanceID, TextBlockListID, Index,
                    ColorComponent);
        }

        public int DAGetTextBlockColorType(int TextBlockListID, int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockColorType(_instanceID, TextBlockListID,
                    Index);
        }

        public int DAGetTextBlockCount(int TextBlockListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockCount(_instanceID, TextBlockListID);
        }

        public string DAGetTextBlockFontName(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetTextBlockFontName(_instanceID, TextBlockListID,
                    Index));
        }

        public double DAGetTextBlockFontSize(int TextBlockListID, int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAGetTextBlockFontSize(_instanceID, TextBlockListID, Index);
        }

        public string DAGetTextBlockText(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDAGetTextBlockText(_instanceID, TextBlockListID, Index));
        }

        public int DAHasPageBox(int FileHandle, int PageRef, int BoxIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAHasPageBox(_instanceID, FileHandle, PageRef, BoxIndex);
        }

        public int DAHidePage(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAHidePage(_instanceID, FileHandle, PageRef);
        }

        public int DAMovePage(int FileHandle, int PageRef, int TargetPageRef, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAMovePage(_instanceID, FileHandle, PageRef, TargetPageRef,
                    Options);
        }

        public int DANewPage(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDANewPage(_instanceID, FileHandle);
        }

        public int DANewPages(int FileHandle, int PageCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDANewPages(_instanceID, FileHandle, PageCount);
        }

        public int DANormalizePage(int FileHandle, int PageRef, int NormalizeOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDANormalizePage(_instanceID, FileHandle, PageRef,
                    NormalizeOptions);
        }

        public int DAOpenFile(string InputFileName, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAOpenFile(_instanceID, InputFileName, Password);
        }

        public int DAOpenFileReadOnly(string InputFileName, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAOpenFileReadOnly(_instanceID, InputFileName, Password);
        }

        public int DAPageRotation(int FileHandle, int PageRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAPageRotation(_instanceID, FileHandle, PageRef);
        }

        public int DAReleaseImageList(int FileHandle, int ImageListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAReleaseImageList(_instanceID, FileHandle, ImageListID);
        }

        public int DAReleaseTextBlocks(int TextBlockListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAReleaseTextBlocks(_instanceID, TextBlockListID);
        }

        public int DARemoveUsageRights(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDARemoveUsageRights(_instanceID, FileHandle);
        }

        public int DARenderPageToDC(int FileHandle, int PageRef, double DPI, IntPtr DC)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDARenderPageToDC(_instanceID, FileHandle, PageRef, DPI, DC);
        }

        public int DARenderPageToFile(int FileHandle, int PageRef, int Options, double DPI,
            string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDARenderPageToFile(_instanceID, FileHandle, PageRef,
                    Options, DPI, FileName);
        }

        public byte[] DARenderPageToString(int FileHandle, int PageRef, int Options, double DPI)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryDARenderPageToString(_instanceID, FileHandle,
                    PageRef, Options, DPI);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int DARotatePage(int FileHandle, int PageRef, int Angle, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDARotatePage(_instanceID, FileHandle, PageRef, Angle,
                    Options);
        }

        public int DASaveAsFile(int FileHandle, string OutputFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASaveAsFile(_instanceID, FileHandle, OutputFileName);
        }

        public int DASaveImageDataToFile(int FileHandle, int ImageListID, int ImageIndex,
            string ImageFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASaveImageDataToFile(_instanceID, FileHandle, ImageListID,
                    ImageIndex, ImageFileName);
        }

        public int DASetInformation(int FileHandle, string Key, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetInformation(_instanceID, FileHandle, Key, NewValue);
        }

        public int DASetPageBox(int FileHandle, int PageRef, int BoxIndex, double X1, double Y1,
            double X2, double Y2)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetPageBox(_instanceID, FileHandle, PageRef, BoxIndex,
                    X1, Y1, X2, Y2);
        }

        public int DASetPageLayout(int FileHandle, int NewPageLayout)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetPageLayout(_instanceID, FileHandle, NewPageLayout);
        }

        public int DASetPageMode(int FileHandle, int NewPageMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetPageMode(_instanceID, FileHandle, NewPageMode);
        }

        public int DASetPageSize(int FileHandle, int PageRef, double PntWidth, double PntHeight)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetPageSize(_instanceID, FileHandle, PageRef, PntWidth,
                    PntHeight);
        }

        public int DASetRenderArea(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetRenderArea(_instanceID, Left, Top, Width, Height);
        }

        public int DASetTextExtractionArea(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetTextExtractionArea(_instanceID, Left, Top, Width,
                    Height);
        }

        public int DASetTextExtractionOptions(int OptionID, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetTextExtractionOptions(_instanceID, OptionID, NewValue);
        }

        public int DASetTextExtractionScaling(int Options, double Horizontal, double Vertical)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetTextExtractionScaling(_instanceID, Options,
                    Horizontal, Vertical);
        }

        public int DASetTextExtractionWordGap(double NewWordGap)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDASetTextExtractionWordGap(_instanceID, NewWordGap);
        }

        public int DAShiftedHeader(int FileHandle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDAShiftedHeader(_instanceID, FileHandle);
        }

        public int Decrypt()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDecrypt(_instanceID);
        }

        public int DecryptFile(string InputFileName, string OutputFileName, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDecryptFile(_instanceID, InputFileName, OutputFileName,
                    Password);
        }

        public int DeleteAnalysis(int AnalysisID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeleteAnalysis(_instanceID, AnalysisID);
        }

        public int DeleteAnnotation(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeleteAnnotation(_instanceID, Index);
        }

        public int DeleteContentStream()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeleteContentStream(_instanceID);
        }

        public int DeleteFormField(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeleteFormField(_instanceID, Index);
        }

        public int DeleteOptionalContentGroup(int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeleteOptionalContentGroup(_instanceID,
                    OptionalContentGroupID);
        }

        public int DeletePageLGIDict(int DictIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeletePageLGIDict(_instanceID, DictIndex);
        }

        public int DeletePages(int StartPage, int PageCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDeletePages(_instanceID, StartPage, PageCount);
        }

        public int DestroyRenderer()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDestroyRenderer(_instanceID);
        }

        public int DocJavaScriptAction(string ActionType, string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDocJavaScriptAction(_instanceID, ActionType, JavaScript);
        }

        public int DocumentCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDocumentCount(_instanceID);
        }

        public int DrawArc(double XPos, double YPos, double Radius, double StartAngle,
            double EndAngle, int Pie, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawArc(_instanceID, XPos, YPos, Radius, StartAngle,
                    EndAngle, Pie, DrawOptions);
        }

        public int DrawBarcode(double Left, double Top, double Width, double Height, string Text,
            int Barcode, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawBarcode(_instanceID, Left, Top, Width, Height, Text,
                    Barcode, Options);
        }

        public int DrawBox(double Left, double Top, double Width, double Height, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawBox(_instanceID, Left, Top, Width, Height, DrawOptions);
        }

        public int DrawCapturedPage(int CaptureID, double Left, double Top, double Width,
            double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawCapturedPage(_instanceID, CaptureID, Left, Top, Width,
                    Height);
        }

        public int DrawCapturedPageMatrix(int CaptureID, double M11, double M12, double M21,
            double M22, double MDX, double MDY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawCapturedPageMatrix(_instanceID, CaptureID, M11, M12,
                    M21, M22, MDX, MDY);
        }

        public int DrawCircle(double XPos, double YPos, double Radius, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawCircle(_instanceID, XPos, YPos, Radius, DrawOptions);
        }

        public int DrawDataMatrixSymbol(double Left, double Top, double ModuleSize, string Text,
            int Encoding, int SymbolSize, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawDataMatrixSymbol(_instanceID, Left, Top, ModuleSize,
                    Text, Encoding, SymbolSize, Options);
        }

        public int DrawEllipse(double XPos, double YPos, double Width, double Height,
            int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawEllipse(_instanceID, XPos, YPos, Width, Height,
                    DrawOptions);
        }

        public int DrawEllipticArc(double XPos, double YPos, double Width, double Height,
            double StartAngle, double EndAngle, int Pie, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawEllipticArc(_instanceID, XPos, YPos, Width, Height,
                    StartAngle, EndAngle, Pie, DrawOptions);
        }

        public int DrawFontGroupText(string FontGroupName, double XPos, double YPos, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawFontGroupText(_instanceID, FontGroupName, XPos, YPos,
                    Text);
        }

        public int DrawHTMLText(double Left, double Top, double Width, string HTMLText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawHTMLText(_instanceID, Left, Top, Width, HTMLText);
        }

        public string DrawHTMLTextBox(double Left, double Top, double Width, double Height,
            string HTMLText)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDrawHTMLTextBox(_instanceID, Left, Top, Width, Height,
                    HTMLText));
        }

        public string DrawHTMLTextBoxMatrix(double Width, double Height, string HTMLText, double M11,
            double M12, double M21, double M22, double MDX, double MDY)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryDrawHTMLTextBoxMatrix(_instanceID, Width, Height,
                    HTMLText, M11, M12, M21, M22, MDX, MDY));
        }

        public int DrawHTMLTextMatrix(double Width, string HTMLText, double M11, double M12,
            double M21, double M22, double MDX, double MDY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawHTMLTextMatrix(_instanceID, Width, HTMLText, M11, M12,
                    M21, M22, MDX, MDY);
        }

        public int DrawImage(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawImage(_instanceID, Left, Top, Width, Height);
        }

        public int DrawImageMatrix(double M11, double M12, double M21, double M22, double MDX,
            double MDY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawImageMatrix(_instanceID, M11, M12, M21, M22, MDX, MDY);
        }

        public int DrawIntelligentMailBarcode(double Left, double Top, double BarWidth,
            double FullBarHeight, double TrackerHeight, double SpaceWidth, string BarcodeData,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawIntelligentMailBarcode(_instanceID, Left, Top,
                    BarWidth, FullBarHeight, TrackerHeight, SpaceWidth, BarcodeData, Options);
        }

        public int DrawLine(double StartX, double StartY, double EndX, double EndY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawLine(_instanceID, StartX, StartY, EndX, EndY);
        }

        public int DrawMultiLineText(double XPos, double YPos, string Delimiter, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawMultiLineText(_instanceID, XPos, YPos, Delimiter, Text);
        }

        public int DrawPDF417Symbol(double Left, double Top, string Text, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawPDF417Symbol(_instanceID, Left, Top, Text, Options);
        }

        public int DrawPDF417SymbolEx(double Left, double Top, string Text, int Options,
            int FixedColumns, int FixedRows, int ErrorLevel, double ModuleSize,
            double HeightWidthRatio)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawPDF417SymbolEx(_instanceID, Left, Top, Text, Options,
                    FixedColumns, FixedRows, ErrorLevel, ModuleSize, HeightWidthRatio);
        }

        public int DrawPath(int PathOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawPath(_instanceID, PathOptions);
        }

        public int DrawPathEvenOdd(int PathOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawPathEvenOdd(_instanceID, PathOptions);
        }

        public int DrawPostScriptXObject(int PSRef)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawPostScriptXObject(_instanceID, PSRef);
        }

        public int DrawQRCode(double Left, double Top, double SymbolSize, string Text,
            int EncodeOptions, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawQRCode(_instanceID, Left, Top, SymbolSize, Text,
                    EncodeOptions, DrawOptions);
        }

        public int DrawRotatedBox(double Left, double Bottom, double Width, double Height,
            double Angle, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedBox(_instanceID, Left, Bottom, Width, Height,
                    Angle, DrawOptions);
        }

        public int DrawRotatedCapturedPage(int CaptureID, double Left, double Bottom, double Width,
            double Height, double Angle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedCapturedPage(_instanceID, CaptureID, Left,
                    Bottom, Width, Height, Angle);
        }

        public int DrawRotatedImage(double Left, double Bottom, double Width, double Height,
            double Angle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedImage(_instanceID, Left, Bottom, Width, Height,
                    Angle);
        }

        public int DrawRotatedMultiLineText(double XPos, double YPos, double Angle, string Delimiter,
            string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedMultiLineText(_instanceID, XPos, YPos, Angle,
                    Delimiter, Text);
        }

        public int DrawRotatedText(double XPos, double YPos, double Angle, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedText(_instanceID, XPos, YPos, Angle, Text);
        }

        public int DrawRotatedTextBox(double Left, double Top, double Width, double Height,
            double Angle, string Text, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedTextBox(_instanceID, Left, Top, Width, Height,
                    Angle, Text, Options);
        }

        public int DrawRotatedTextBoxEx(double Left, double Top, double Width, double Height,
            double Angle, string Text, int Options, int Border, int Radius, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRotatedTextBoxEx(_instanceID, Left, Top, Width, Height,
                    Angle, Text, Options, Border, Radius, DrawOptions);
        }

        public int DrawRoundedBox(double Left, double Top, double Width, double Height,
            double Radius, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRoundedBox(_instanceID, Left, Top, Width, Height,
                    Radius, DrawOptions);
        }

        public int DrawRoundedRotatedBox(double Left, double Bottom, double Width, double Height,
            double Radius, double Angle, int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawRoundedRotatedBox(_instanceID, Left, Bottom, Width,
                    Height, Radius, Angle, DrawOptions);
        }

        public int DrawScaledImage(double Left, double Top, double Scale)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawScaledImage(_instanceID, Left, Top, Scale);
        }

        public int DrawSpacedText(double XPos, double YPos, double Spacing, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawSpacedText(_instanceID, XPos, YPos, Spacing, Text);
        }

        public double DrawTableRows(int TableID, double Left, double Top, double Height,
            int FirstRow, int LastRow)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawTableRows(_instanceID, TableID, Left, Top, Height,
                    FirstRow, LastRow);
        }

        public int DrawText(double XPos, double YPos, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawText(_instanceID, XPos, YPos, Text);
        }

        public int DrawTextArc(double XPos, double YPos, double Radius, double Angle, string Text,
            int DrawOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawTextArc(_instanceID, XPos, YPos, Radius, Angle, Text,
                    DrawOptions);
        }

        public int DrawTextBox(double Left, double Top, double Width, double Height, string Text,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawTextBox(_instanceID, Left, Top, Width, Height, Text,
                    Options);
        }

        public int DrawTextBoxMatrix(double Width, double Height, string Text, int Options,
            double M11, double M12, double M21, double M22, double MDX, double MDY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawTextBoxMatrix(_instanceID, Width, Height, Text,
                    Options, M11, M12, M21, M22, MDX, MDY);
        }

        public int DrawUniscribeText(double XPos, double YPos, string Text, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawUniscribeText(_instanceID, XPos, YPos, Text, Options);
        }

        public int DrawWrappedText(double XPos, double YPos, double Width, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryDrawWrappedText(_instanceID, XPos, YPos, Width, Text);
        }

        public int EditableContentStream()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEditableContentStream(_instanceID);
        }

        public int EmbedFile(string Title, string FileName, string MIMEType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEmbedFile(_instanceID, Title, FileName, MIMEType);
        }

        public int EmbedRelatedFile(string Title, string FileName, string MIMEType,
            string AFRelationship, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEmbedRelatedFile(_instanceID, Title, FileName, MIMEType,
                    AFRelationship, Options);
        }

        public int EmbeddedFileCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEmbeddedFileCount(_instanceID);
        }

        public int EncapsulateContentStream()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncapsulateContentStream(_instanceID);
        }

        public int EncodePermissions(int CanPrint, int CanCopy, int CanChange, int CanAddNotes,
            int CanFillFields, int CanCopyAccess, int CanAssemble, int CanPrintFull)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncodePermissions(_instanceID, CanPrint, CanCopy,
                    CanChange, CanAddNotes, CanFillFields, CanCopyAccess, CanAssemble, CanPrintFull);
        }

        public int Encrypt(string Owner, string User, int Strength, int Permissions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncrypt(_instanceID, Owner, User, Strength, Permissions);
        }

        public int EncryptFile(string InputFileName, string OutputFileName, string Owner,
            string User, int Strength, int Permissions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncryptFile(_instanceID, InputFileName, OutputFileName,
                    Owner, User, Strength, Permissions);
        }

        public int EncryptWithFingerprint(string Fingerprint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncryptWithFingerprint(_instanceID, Fingerprint);
        }

        public int EncryptionAlgorithm()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncryptionAlgorithm(_instanceID);
        }

        public int EncryptionStatus()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncryptionStatus(_instanceID);
        }

        public int EncryptionStrength()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEncryptionStrength(_instanceID);
        }

        public int EndPageUpdate()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEndPageUpdate(_instanceID);
        }

        public int EndSignProcessToFile(int SignProcessID, string OutputFile)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryEndSignProcessToFile(_instanceID, SignProcessID,
                    OutputFile);
        }

        public byte[] EndSignProcessToString(int SignProcessID)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryEndSignProcessToString(_instanceID, SignProcessID);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public byte[] ExtractFilePageContentToString(string InputFileName, string Password, int Page)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryExtractFilePageContentToString(_instanceID,
                    InputFileName, Password, Page);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public string ExtractFilePageText(string InputFileName, string Password, int Page,
            int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryExtractFilePageText(_instanceID, InputFileName,
                    Password, Page, Options));
        }

        public int ExtractFilePageTextBlocks(string InputFileName, string Password, int Page,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractFilePageTextBlocks(_instanceID, InputFileName,
                    Password, Page, Options);
        }

        public int ExtractFilePages(string InputFileName, string Password, string OutputFileName,
            string RangeList)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractFilePages(_instanceID, InputFileName, Password,
                    OutputFileName, RangeList);
        }

        public int ExtractFilePagesEx(string InputFileName, string Password, string OutputFileName,
            string RangeList, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractFilePagesEx(_instanceID, InputFileName, Password,
                    OutputFileName, RangeList, Options);
        }

        public int ExtractPageRanges(string RangeList)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractPageRanges(_instanceID, RangeList);
        }

        public int ExtractPageTextBlocks(int ExtractOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractPageTextBlocks(_instanceID, ExtractOptions);
        }

        public int ExtractPages(int StartPage, int PageCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryExtractPages(_instanceID, StartPage, PageCount);
        }

        public int FileListCount(string ListName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFileListCount(_instanceID, ListName);
        }

        public string FileListItem(string ListName, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryFileListItem(_instanceID, ListName, Index));
        }

        public int FindFonts()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFindFonts(_instanceID);
        }

        public int FindFormFieldByTitle(string Title)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFindFormFieldByTitle(_instanceID, Title);
        }

        public int FindImages()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFindImages(_instanceID);
        }

        public int FitImage(double Left, double Top, double Width, double Height, int HAlign,
            int VAlign, int Rotate)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFitImage(_instanceID, Left, Top, Width, Height, HAlign,
                    VAlign, Rotate);
        }

        public int FitRotatedTextBox(double Left, double Top, double Width, double Height,
            double Angle, string Text, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFitRotatedTextBox(_instanceID, Left, Top, Width, Height,
                    Angle, Text, Options);
        }

        public int FitTextBox(double Left, double Top, double Width, double Height, string Text,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFitTextBox(_instanceID, Left, Top, Width, Height, Text,
                    Options);
        }

        public int FlattenAllFormFields(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFlattenAllFormFields(_instanceID, Options);
        }

        public int FlattenAnnot(int Index, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFlattenAnnot(_instanceID, Index, Options);
        }

        public int FlattenFormField(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFlattenFormField(_instanceID, Index);
        }

        public int FontCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFontCount(_instanceID);
        }

        public string FontFamily()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryFontFamily(_instanceID));
        }

        public int FontHasKerning()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFontHasKerning(_instanceID);
        }

        public string FontName()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryFontName(_instanceID));
        }

        public string FontReference()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryFontReference(_instanceID));
        }

        public int FontSize()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFontSize(_instanceID);
        }

        public int FontType()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFontType(_instanceID);
        }

        public int FormFieldCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFormFieldCount(_instanceID);
        }

        public int FormFieldHasParent(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFormFieldHasParent(_instanceID, Index);
        }

        public int FormFieldJavaScriptAction(int Index, string ActionType, string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFormFieldJavaScriptAction(_instanceID, Index, ActionType,
                    JavaScript);
        }

        public int FormFieldWebLinkAction(int Index, string ActionType, string Link)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryFormFieldWebLinkAction(_instanceID, Index, ActionType,
                    Link);
        }

        public int GetActionDest(int ActionID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetActionDest(_instanceID, ActionID);
        }

        public string GetActionType(int ActionID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetActionType(_instanceID, ActionID));
        }

        public string GetActionURL(int ActionID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetActionURL(_instanceID, ActionID));
        }

        public string GetAnalysisInfo(int AnalysisID, int AnalysisItem)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetAnalysisInfo(_instanceID, AnalysisID, AnalysisItem));
        }

        public int GetAnnotActionID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotActionID(_instanceID, Index);
        }

        public double GetAnnotDblProperty(int Index, int Tag)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotDblProperty(_instanceID, Index, Tag);
        }

        public int GetAnnotDest(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotDest(_instanceID, Index);
        }

        public string GetAnnotEmbeddedFileName(int Index, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetAnnotEmbeddedFileName(_instanceID, Index, Options));
        }

        public int GetAnnotEmbeddedFileToFile(int Index, int Options, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotEmbeddedFileToFile(_instanceID, Index, Options,
                    FileName);
        }

        public byte[] GetAnnotEmbeddedFileToString(int Index, int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetAnnotEmbeddedFileToString(_instanceID, Index,
                    Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetAnnotIntProperty(int Index, int Tag)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotIntProperty(_instanceID, Index, Tag);
        }

        public int GetAnnotQuadCount(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotQuadCount(_instanceID, Index);
        }

        public double GetAnnotQuadPoints(int Index, int QuadNumber, int PointNumber)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotQuadPoints(_instanceID, Index, QuadNumber,
                    PointNumber);
        }

        public int GetAnnotSoundToFile(int Index, int Options, string SoundFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetAnnotSoundToFile(_instanceID, Index, Options,
                    SoundFileName);
        }

        public byte[] GetAnnotSoundToString(int Index, int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetAnnotSoundToString(_instanceID, Index, Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public string GetAnnotStrProperty(int Index, int Tag)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetAnnotStrProperty(_instanceID, Index, Tag));
        }

        public double GetBarcodeWidth(double NominalWidth, string Text, int Barcode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetBarcodeWidth(_instanceID, NominalWidth, Text, Barcode);
        }

        public string GetBaseURL()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetBaseURL(_instanceID));
        }

        public int GetCSDictEPSG(int CSDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetCSDictEPSG(_instanceID, CSDictID);
        }

        public int GetCSDictType(int CSDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetCSDictType(_instanceID, CSDictID);
        }

        public string GetCSDictWKT(int CSDictID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetCSDictWKT(_instanceID, CSDictID));
        }

        public IntPtr GetCanvasDC(int CanvasWidth, int CanvasHeight)
        {
            if (_dll == null) return IntPtr.Zero;
            else
                return _dll.DebenuPDFLibraryGetCanvasDC(_instanceID, CanvasWidth, CanvasHeight);
        }

        public int GetCanvasDCEx(int CanvasWidth, int CanvasHeight, int ReferenceDC)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetCanvasDCEx(_instanceID, CanvasWidth, CanvasHeight,
                    ReferenceDC);
        }

        public string GetCatalogInformation(string Key)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetCatalogInformation(_instanceID, Key));
        }

        public byte[] GetContentStreamToString()
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetContentStreamToString(_instanceID);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public string GetCustomInformation(string Key)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetCustomInformation(_instanceID, Key));
        }

        public string GetCustomKeys(int Location)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetCustomKeys(_instanceID, Location));
        }

        public string GetDefaultPrinterName()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDefaultPrinterName(_instanceID));
        }

        public string GetDestName(int DestID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDestName(_instanceID, DestID));
        }

        public int GetDestPage(int DestID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDestPage(_instanceID, DestID);
        }

        public int GetDestType(int DestID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDestType(_instanceID, DestID);
        }

        public double GetDestValue(int DestID, int ValueKey)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDestValue(_instanceID, DestID, ValueKey);
        }

        public string GetDocJavaScript(string ActionType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDocJavaScript(_instanceID, ActionType));
        }

        public string GetDocumentFileName()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDocumentFileName(_instanceID));
        }

        public int GetDocumentFileSize()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDocumentFileSize(_instanceID);
        }

        public int GetDocumentID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDocumentID(_instanceID, Index);
        }

        public string GetDocumentIdentifier(int Part, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDocumentIdentifier(_instanceID, Part, Options));
        }

        public string GetDocumentMetadata()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDocumentMetadata(_instanceID));
        }

        public int GetDocumentRepaired()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetDocumentRepaired(_instanceID);
        }

        public string GetDocumentResourceList()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetDocumentResourceList(_instanceID));
        }

        public int GetEmbeddedFileContentToFile(int Index, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetEmbeddedFileContentToFile(_instanceID, Index, FileName);
        }

        public byte[] GetEmbeddedFileContentToString(int Index)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetEmbeddedFileContentToString(_instanceID, Index);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetEmbeddedFileID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetEmbeddedFileID(_instanceID, Index);
        }

        public int GetEmbeddedFileIntProperty(int Index, int Tag)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetEmbeddedFileIntProperty(_instanceID, Index, Tag);
        }

        public string GetEmbeddedFileStrProperty(int Index, int Tag)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetEmbeddedFileStrProperty(_instanceID, Index, Tag));
        }

        public string GetEncryptionFingerprint()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetEncryptionFingerprint(_instanceID));
        }

        public string GetFileMetadata(string InputFileName, string Password)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFileMetadata(_instanceID, InputFileName, Password));
        }

        public int GetFirstChildOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFirstChildOutline(_instanceID, OutlineID);
        }

        public int GetFirstOutline()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFirstOutline(_instanceID);
        }

        public byte[] GetFontCharacterCodesToString(string InputText)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetFontCharacterCodesToString(_instanceID,
                    InputText);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetFontEncoding()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontEncoding(_instanceID);
        }

        public int GetFontFlags(int FontFlagItemID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontFlags(_instanceID, FontFlagItemID);
        }

        public int GetFontID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontID(_instanceID, Index);
        }

        public int GetFontIsEmbedded()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontIsEmbedded(_instanceID);
        }

        public int GetFontIsSubsetted()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontIsSubsetted(_instanceID);
        }

        public int GetFontMetrics(int MetricType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontMetrics(_instanceID, MetricType);
        }

        public int GetFontObjectNumber()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFontObjectNumber(_instanceID);
        }

        public int GetFormFieldActionID(int Index, string TriggerEvent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldActionID(_instanceID, Index, TriggerEvent);
        }

        public int GetFormFieldAlignment(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldAlignment(_instanceID, Index);
        }

        public int GetFormFieldAnnotFlags(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldAnnotFlags(_instanceID, Index);
        }

        public double GetFormFieldBackgroundColor(int Index, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBackgroundColor(_instanceID, Index,
                    ColorComponent);
        }

        public int GetFormFieldBackgroundColorType(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBackgroundColorType(_instanceID, Index);
        }

        public double GetFormFieldBorderColor(int Index, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBorderColor(_instanceID, Index, ColorComponent);
        }

        public int GetFormFieldBorderColorType(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBorderColorType(_instanceID, Index);
        }

        public double GetFormFieldBorderProperty(int Index, int PropKey)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBorderProperty(_instanceID, Index, PropKey);
        }

        public int GetFormFieldBorderStyle(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBorderStyle(_instanceID, Index);
        }

        public double GetFormFieldBound(int Index, int Edge)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldBound(_instanceID, Index, Edge);
        }

        public string GetFormFieldCaption(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldCaption(_instanceID, Index));
        }

        public string GetFormFieldCaptionEx(int Index, int StringType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldCaptionEx(_instanceID, Index, StringType));
        }

        public int GetFormFieldCheckStyle(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldCheckStyle(_instanceID, Index);
        }

        public string GetFormFieldChildTitle(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldChildTitle(_instanceID, Index));
        }

        public int GetFormFieldChoiceType(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldChoiceType(_instanceID, Index);
        }

        public double GetFormFieldColor(int Index, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldColor(_instanceID, Index, ColorComponent);
        }

        public int GetFormFieldComb(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldComb(_instanceID, Index);
        }

        public string GetFormFieldDefaultValue(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldDefaultValue(_instanceID, Index));
        }

        public string GetFormFieldDescription(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldDescription(_instanceID, Index));
        }

        public int GetFormFieldFlags(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldFlags(_instanceID, Index);
        }

        public string GetFormFieldFontName(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldFontName(_instanceID, Index));
        }

        public string GetFormFieldJavaScript(int Index, string ActionType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldJavaScript(_instanceID, Index, ActionType));
        }

        public int GetFormFieldKidCount(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldKidCount(_instanceID, Index);
        }

        public int GetFormFieldKidTempIndex(int Index, int SubIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldKidTempIndex(_instanceID, Index, SubIndex);
        }

        public int GetFormFieldMaxLen(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldMaxLen(_instanceID, Index);
        }

        public int GetFormFieldNoExport(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldNoExport(_instanceID, Index);
        }

        public int GetFormFieldPage(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldPage(_instanceID, Index);
        }

        public int GetFormFieldPrintable(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldPrintable(_instanceID, Index);
        }

        public int GetFormFieldReadOnly(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldReadOnly(_instanceID, Index);
        }

        public int GetFormFieldRequired(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldRequired(_instanceID, Index);
        }

        public string GetFormFieldRichTextString(int Index, string Key)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldRichTextString(_instanceID, Index, Key));
        }

        public int GetFormFieldRotation(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldRotation(_instanceID, Index);
        }

        public int GetFormFieldSubCount(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldSubCount(_instanceID, Index);
        }

        public string GetFormFieldSubDisplayName(int Index, int SubIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldSubDisplayName(_instanceID, Index, SubIndex));
        }

        public string GetFormFieldSubName(int Index, int SubIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldSubName(_instanceID, Index, SubIndex));
        }

        public string GetFormFieldSubmitActionString(int Index, string ActionType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldSubmitActionString(_instanceID, Index,
                    ActionType));
        }

        public int GetFormFieldTabOrder(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldTabOrder(_instanceID, Index);
        }

        public int GetFormFieldTabOrderEx(int Index, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldTabOrderEx(_instanceID, Index, Options);
        }

        public int GetFormFieldTextFlags(int Index, int ValueKey)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldTextFlags(_instanceID, Index, ValueKey);
        }

        public double GetFormFieldTextSize(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldTextSize(_instanceID, Index);
        }

        public string GetFormFieldTitle(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldTitle(_instanceID, Index));
        }

        public int GetFormFieldType(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldType(_instanceID, Index);
        }

        public string GetFormFieldValue(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldValue(_instanceID, Index));
        }

        public string GetFormFieldValueByTitle(string Title)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldValueByTitle(_instanceID, Title));
        }

        public int GetFormFieldVisible(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFieldVisible(_instanceID, Index);
        }

        public string GetFormFieldWebLink(int Index, string ActionType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFieldWebLink(_instanceID, Index, ActionType));
        }

        public int GetFormFontCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetFormFontCount(_instanceID);
        }

        public string GetFormFontName(int FontIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetFormFontName(_instanceID, FontIndex));
        }

        public string GetGlobalJavaScript(string PackageName)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetGlobalJavaScript(_instanceID, PackageName));
        }

        public double GetHTMLTextHeight(double Width, string HTMLText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetHTMLTextHeight(_instanceID, Width, HTMLText);
        }

        public int GetHTMLTextLineCount(double Width, string HTMLText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetHTMLTextLineCount(_instanceID, Width, HTMLText);
        }

        public double GetHTMLTextWidth(double MaxWidth, string HTMLText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetHTMLTextWidth(_instanceID, MaxWidth, HTMLText);
        }

        public int GetImageID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImageID(_instanceID, Index);
        }

        public int GetImageListCount(int ImageListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImageListCount(_instanceID, ImageListID);
        }

        public byte[] GetImageListItemDataToString(int ImageListID, int ImageIndex, int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetImageListItemDataToString(_instanceID,
                    ImageListID, ImageIndex, Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public double GetImageListItemDblProperty(int ImageListID, int ImageIndex, int PropertyID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImageListItemDblProperty(_instanceID, ImageListID,
                    ImageIndex, PropertyID);
        }

        public string GetImageListItemFormatDesc(int ImageListID, int ImageIndex, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetImageListItemFormatDesc(_instanceID, ImageListID,
                    ImageIndex, Options));
        }

        public int GetImageListItemIntProperty(int ImageListID, int ImageIndex, int PropertyID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImageListItemIntProperty(_instanceID, ImageListID,
                    ImageIndex, PropertyID);
        }

        public int GetImageMeasureDict()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImageMeasureDict(_instanceID);
        }

        public int GetImagePageCount(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImagePageCount(_instanceID, FileName);
        }

        public int GetImagePageCountFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryGetImagePageCountFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int GetImagePtDataDict()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetImagePtDataDict(_instanceID);
        }

        public string GetInformation(int Key)   
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetInformation(_instanceID, Key));
        }

        public string GetInstalledFontsByCharset(int CharsetIndex, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetInstalledFontsByCharset(_instanceID, CharsetIndex,
                    Options));
        }

        public string GetInstalledFontsByCodePage(int CodePage, int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetInstalledFontsByCodePage(_instanceID, CodePage,
                    Options));
        }

        public int GetKerning(string CharPair)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetKerning(_instanceID, CharPair);
        }

        public string GetLatestPrinterNames()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetLatestPrinterNames(_instanceID));
        }

        public int GetMaxObjectNumber()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMaxObjectNumber(_instanceID);
        }

        public int GetMeasureDictBoundsCount(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictBoundsCount(_instanceID, MeasureDictID);
        }

        public double GetMeasureDictBoundsItem(int MeasureDictID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictBoundsItem(_instanceID, MeasureDictID,
                    ItemIndex);
        }

        public int GetMeasureDictCoordinateSystem(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictCoordinateSystem(_instanceID, MeasureDictID);
        }

        public int GetMeasureDictDCSDict(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictDCSDict(_instanceID, MeasureDictID);
        }

        public int GetMeasureDictGCSDict(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictGCSDict(_instanceID, MeasureDictID);
        }

        public int GetMeasureDictGPTSCount(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictGPTSCount(_instanceID, MeasureDictID);
        }

        public double GetMeasureDictGPTSItem(int MeasureDictID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictGPTSItem(_instanceID, MeasureDictID,
                    ItemIndex);
        }

        public int GetMeasureDictLPTSCount(int MeasureDictID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictLPTSCount(_instanceID, MeasureDictID);
        }

        public double GetMeasureDictLPTSItem(int MeasureDictID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictLPTSItem(_instanceID, MeasureDictID,
                    ItemIndex);
        }

        public int GetMeasureDictPDU(int MeasureDictID, int UnitIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasureDictPDU(_instanceID, MeasureDictID, UnitIndex);
        }

        public int GetMeasurementUnits()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetMeasurementUnits(_instanceID);
        }

        public int GetNamedDestination(string DestName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetNamedDestination(_instanceID, DestName);
        }

        public int GetNeedAppearances()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetNeedAppearances(_instanceID);
        }

        public int GetNextOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetNextOutline(_instanceID, OutlineID);
        }

        public int GetObjectCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetObjectCount(_instanceID);
        }

        public int GetObjectDecodeError(int ObjectNumber)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetObjectDecodeError(_instanceID, ObjectNumber);
        }

        public byte[] GetObjectToString(int ObjectNumber)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetObjectToString(_instanceID, ObjectNumber);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetOpenActionDestination()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOpenActionDestination(_instanceID);
        }

        public string GetOpenActionJavaScript()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOpenActionJavaScript(_instanceID));
        }

        public int GetOptionalContentConfigCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigCount(_instanceID);
        }

        public int GetOptionalContentConfigLocked(int OptionalContentConfigID,
            int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigLocked(_instanceID,
                    OptionalContentConfigID, OptionalContentGroupID);
        }

        public int GetOptionalContentConfigOrderCount(int OptionalContentConfigID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigOrderCount(_instanceID,
                    OptionalContentConfigID);
        }

        public int GetOptionalContentConfigOrderItemID(int OptionalContentConfigID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigOrderItemID(_instanceID,
                    OptionalContentConfigID, ItemIndex);
        }

        public string GetOptionalContentConfigOrderItemLabel(int OptionalContentConfigID,
            int ItemIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOptionalContentConfigOrderItemLabel(_instanceID,
                    OptionalContentConfigID, ItemIndex));
        }

        public int GetOptionalContentConfigOrderItemLevel(int OptionalContentConfigID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigOrderItemLevel(_instanceID,
                    OptionalContentConfigID, ItemIndex);
        }

        public int GetOptionalContentConfigOrderItemType(int OptionalContentConfigID, int ItemIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigOrderItemType(_instanceID,
                    OptionalContentConfigID, ItemIndex);
        }

        public int GetOptionalContentConfigState(int OptionalContentConfigID,
            int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentConfigState(_instanceID,
                    OptionalContentConfigID, OptionalContentGroupID);
        }

        public int GetOptionalContentGroupID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentGroupID(_instanceID, Index);
        }

        public string GetOptionalContentGroupName(int OptionalContentGroupID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOptionalContentGroupName(_instanceID,
                    OptionalContentGroupID));
        }

        public int GetOptionalContentGroupPrintable(int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentGroupPrintable(_instanceID,
                    OptionalContentGroupID);
        }

        public int GetOptionalContentGroupVisible(int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOptionalContentGroupVisible(_instanceID,
                    OptionalContentGroupID);
        }

        public int GetOrigin()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOrigin(_instanceID);
        }

        public int GetOutlineActionID(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineActionID(_instanceID, OutlineID);
        }

        public double GetOutlineColor(int OutlineID, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineColor(_instanceID, OutlineID, ColorComponent);
        }

        public int GetOutlineDest(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineDest(_instanceID, OutlineID);
        }

        public int GetOutlineID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineID(_instanceID, Index);
        }

        public string GetOutlineJavaScript(int OutlineID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOutlineJavaScript(_instanceID, OutlineID));
        }

        public int GetOutlineObjectNumber(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineObjectNumber(_instanceID, OutlineID);
        }

        public string GetOutlineOpenFile(int OutlineID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOutlineOpenFile(_instanceID, OutlineID));
        }

        public int GetOutlinePage(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlinePage(_instanceID, OutlineID);
        }

        public int GetOutlineStyle(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetOutlineStyle(_instanceID, OutlineID);
        }

        public string GetOutlineWebLink(int OutlineID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetOutlineWebLink(_instanceID, OutlineID));
        }

        public double GetPDF417SymbolHeight(string Text, int Options, int FixedColumns,
            int FixedRows, int ErrorLevel, double ModuleSize, double HeightWidthRatio)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPDF417SymbolHeight(_instanceID, Text, Options,
                    FixedColumns, FixedRows, ErrorLevel, ModuleSize, HeightWidthRatio);
        }

        public double GetPDF417SymbolWidth(string Text, int Options, int FixedColumns, int FixedRows,
            int ErrorLevel, double ModuleSize, double HeightWidthRatio)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPDF417SymbolWidth(_instanceID, Text, Options,
                    FixedColumns, FixedRows, ErrorLevel, ModuleSize, HeightWidthRatio);
        }

        public double GetPageBox(int BoxType, int Dimension)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageBox(_instanceID, BoxType, Dimension);
        }

        public string GetPageColorSpaces(int Options)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPageColorSpaces(_instanceID, Options));
        }

        public byte[] GetPageContentToString()
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetPageContentToString(_instanceID);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetPageImageList(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageImageList(_instanceID, Options);
        }

        public string GetPageJavaScript(string ActionType)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPageJavaScript(_instanceID, ActionType));
        }

        public string GetPageLGIDictContent(int DictIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPageLGIDictContent(_instanceID, DictIndex));
        }

        public int GetPageLGIDictCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageLGIDictCount(_instanceID);
        }

        public string GetPageLabel(int Page)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPageLabel(_instanceID, Page));
        }

        public int GetPageLayout()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageLayout(_instanceID);
        }

        public byte[] GetPageMetricsToString(int StartPage, int EndPage, int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetPageMetricsToString(_instanceID, StartPage,
                    EndPage, Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GetPageMode()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageMode(_instanceID);
        }

        public string GetPageText(int ExtractOptions)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPageText(_instanceID, ExtractOptions));
        }

        public double GetPageUserUnit()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageUserUnit(_instanceID);
        }

        public int GetPageViewPortCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageViewPortCount(_instanceID);
        }

        public int GetPageViewPortID(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPageViewPortID(_instanceID, Index);
        }

        public int GetParentOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetParentOutline(_instanceID, OutlineID);
        }

        public int GetPrevOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetPrevOutline(_instanceID, OutlineID);
        }

        public byte[] GetPrintPreviewBitmapToString(string PrinterName, int PreviewPage,
            int PrintOptions, int MaxDimension, int PreviewOptions)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetPrintPreviewBitmapToString(_instanceID,
                    PrinterName, PreviewPage, PrintOptions, MaxDimension, PreviewOptions);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public string GetPrinterBins(string PrinterName)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPrinterBins(_instanceID, PrinterName));
        }

        public byte[] GetPrinterDevModeToString(string PrinterName)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetPrinterDevModeToString(_instanceID, PrinterName);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public string GetPrinterMediaTypes(string PrinterName)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPrinterMediaTypes(_instanceID, PrinterName));
        }

        public string GetPrinterNames()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetPrinterNames(_instanceID));
        }

        public double GetRenderScale()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetRenderScale(_instanceID);
        }

        public double GetScale()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetScale(_instanceID);
        }

        public int GetSignProcessByteRange(int SignProcessID, int ArrayPosition)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetSignProcessByteRange(_instanceID, SignProcessID,
                    ArrayPosition);
        }

        public int GetSignProcessResult(int SignProcessID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetSignProcessResult(_instanceID, SignProcessID);
        }

        public int GetStringListCount(int StringListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetStringListCount(_instanceID, StringListID);
        }

        public string GetStringListItem(int StringListID, int ItemIndex)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetStringListItem(_instanceID, StringListID, ItemIndex));
        }

        public string GetTabOrderMode()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTabOrderMode(_instanceID));
        }

        public double GetTableCellDblProperty(int TableID, int RowNumber, int ColumnNumber, int Tag)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTableCellDblProperty(_instanceID, TableID, RowNumber,
                    ColumnNumber, Tag);
        }

        public int GetTableCellIntProperty(int TableID, int RowNumber, int ColumnNumber, int Tag)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTableCellIntProperty(_instanceID, TableID, RowNumber,
                    ColumnNumber, Tag);
        }

        public string GetTableCellStrProperty(int TableID, int RowNumber, int ColumnNumber, int Tag)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTableCellStrProperty(_instanceID, TableID, RowNumber,
                    ColumnNumber, Tag));
        }

        public int GetTableColumnCount(int TableID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTableColumnCount(_instanceID, TableID);
        }

        public int GetTableLastDrawnRow(int TableID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTableLastDrawnRow(_instanceID, TableID);
        }

        public int GetTableRowCount(int TableID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTableRowCount(_instanceID, TableID);
        }

        public string GetTempPath()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTempPath(_instanceID));
        }

        public double GetTextAscent()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextAscent(_instanceID);
        }

        public string GetTextBlockAsString(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTextBlockAsString(_instanceID, TextBlockListID,
                    Index));
        }

        public double GetTextBlockBound(int TextBlockListID, int Index, int BoundIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockBound(_instanceID, TextBlockListID, Index,
                    BoundIndex);
        }

        public double GetTextBlockCharWidth(int TextBlockListID, int Index, int CharIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockCharWidth(_instanceID, TextBlockListID, Index,
                    CharIndex);
        }

        public double GetTextBlockColor(int TextBlockListID, int Index, int ColorComponent)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockColor(_instanceID, TextBlockListID, Index,
                    ColorComponent);
        }

        public int GetTextBlockColorType(int TextBlockListID, int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockColorType(_instanceID, TextBlockListID, Index);
        }

        public int GetTextBlockCount(int TextBlockListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockCount(_instanceID, TextBlockListID);
        }

        public string GetTextBlockFontName(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTextBlockFontName(_instanceID, TextBlockListID,
                    Index));
        }

        public double GetTextBlockFontSize(int TextBlockListID, int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBlockFontSize(_instanceID, TextBlockListID, Index);
        }

        public string GetTextBlockText(int TextBlockListID, int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetTextBlockText(_instanceID, TextBlockListID, Index));
        }

        public double GetTextBound(int Edge)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextBound(_instanceID, Edge);
        }

        public double GetTextDescent()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextDescent(_instanceID);
        }

        public double GetTextHeight()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextHeight(_instanceID);
        }

        public double GetTextSize()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextSize(_instanceID);
        }

        public double GetTextWidth(string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetTextWidth(_instanceID, Text);
        }

        public string GetUnicodeCharactersFromEncoding(int Encoding)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetUnicodeCharactersFromEncoding(_instanceID, Encoding));
        }

        public double GetViewPortBBox(int ViewPortID, int Dimension)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetViewPortBBox(_instanceID, ViewPortID, Dimension);
        }

        public int GetViewPortMeasureDict(int ViewPortID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetViewPortMeasureDict(_instanceID, ViewPortID);
        }

        public string GetViewPortName(int ViewPortID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetViewPortName(_instanceID, ViewPortID));
        }

        public int GetViewPortPtDataDict(int ViewPortID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetViewPortPtDataDict(_instanceID, ViewPortID);
        }

        public int GetViewerPreferences(int Option)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetViewerPreferences(_instanceID, Option);
        }

        public string GetWrappedText(double Width, string Delimiter, string Text)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetWrappedText(_instanceID, Width, Delimiter, Text));
        }

        public string GetWrappedTextBreakString(double Width, string Delimiter, string Text)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetWrappedTextBreakString(_instanceID, Width, Delimiter,
                    Text));
        }

        public double GetWrappedTextHeight(double Width, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetWrappedTextHeight(_instanceID, Width, Text);
        }

        public int GetWrappedTextLineCount(double Width, string Text)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetWrappedTextLineCount(_instanceID, Width, Text);
        }

        public int GetXFAFormFieldCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGetXFAFormFieldCount(_instanceID);
        }

        public string GetXFAFormFieldName(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetXFAFormFieldName(_instanceID, Index));
        }

        public string GetXFAFormFieldNames(string Delimiter)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetXFAFormFieldNames(_instanceID, Delimiter));
        }

        public string GetXFAFormFieldValue(string XFAFieldName)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGetXFAFormFieldValue(_instanceID, XFAFieldName));
        }

        public byte[] GetXFAToString(int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryGetXFAToString(_instanceID, Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int GlobalJavaScriptCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryGlobalJavaScriptCount(_instanceID);
        }

        public string GlobalJavaScriptPackageName(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryGlobalJavaScriptPackageName(_instanceID, Index));
        }

        public int HasFontResources()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryHasFontResources(_instanceID);
        }

        public int HasPageBox(int BoxType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryHasPageBox(_instanceID, BoxType);
        }

        public int HidePage()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryHidePage(_instanceID);
        }

        public int ImageCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageCount(_instanceID);
        }

        public int ImageFillColor()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageFillColor(_instanceID);
        }

        public int ImageHeight()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageHeight(_instanceID);
        }

        public int ImageHorizontalResolution()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageHorizontalResolution(_instanceID);
        }

        public int ImageResolutionUnits()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageResolutionUnits(_instanceID);
        }

        public int ImageType()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageType(_instanceID);
        }

        public int ImageVerticalResolution()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageVerticalResolution(_instanceID);
        }

        public int ImageWidth()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImageWidth(_instanceID);
        }

        public int ImportEMFFromFile(string FileName, int FontOptions, int GeneralOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryImportEMFFromFile(_instanceID, FileName, FontOptions,
                    GeneralOptions);
        }

        public int InsertPages(int StartPage, int PageCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryInsertPages(_instanceID, StartPage, PageCount);
        }

        public int InsertTableColumns(int TableID, int Position, int NewColumnCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryInsertTableColumns(_instanceID, TableID, Position,
                    NewColumnCount);
        }

        public int InsertTableRows(int TableID, int Position, int NewRowCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryInsertTableRows(_instanceID, TableID, Position,
                    NewRowCount);
        }

        public int IsAnnotFormField(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryIsAnnotFormField(_instanceID, Index);
        }

        public int IsLinearized()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryIsLinearized(_instanceID);
        }

        public int IsTaggedPDF()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryIsTaggedPDF(_instanceID);
        }

        public int LastErrorCode()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryLastErrorCode(_instanceID);
        }

        public string LastRenderError()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryLastRenderError(_instanceID));
        }

        public string LibraryVersion()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryLibraryVersion(_instanceID));
        }

        public string LibraryVersionEx()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryLibraryVersionEx(_instanceID));
        }

        public string LicenseInfo()
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryLicenseInfo(_instanceID));
        }

        public int LinearizeFile(string InputFileName, string Password, string OutputFileName,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryLinearizeFile(_instanceID, InputFileName, Password,
                    OutputFileName, Options);
        }

        public int LoadFromCanvasDC(double DPI, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryLoadFromCanvasDC(_instanceID, DPI, Options);
        }

        public int LoadFromFile(string FileName, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryLoadFromFile(_instanceID, FileName, Password);
        }

        public int LoadFromString(byte[] Source, string Password)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryLoadFromString(_instanceID, bufferID, Password);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int LoadState()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryLoadState(_instanceID);
        }

        public int MergeDocument(int DocumentID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMergeDocument(_instanceID, DocumentID);
        }

        public int MergeFileList(string ListName, string OutputFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMergeFileList(_instanceID, ListName, OutputFileName);
        }

        public int MergeFileListFast(string ListName, string OutputFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMergeFileListFast(_instanceID, ListName, OutputFileName);
        }

        public int MergeFiles(string FirstFileName, string SecondFileName, string OutputFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMergeFiles(_instanceID, FirstFileName, SecondFileName,
                    OutputFileName);
        }

        public int MergeTableCells(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMergeTableCells(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn);
        }

        public int MoveContentStream(int FromPosition, int ToPosition)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMoveContentStream(_instanceID, FromPosition, ToPosition);
        }

        public int MoveOutlineAfter(int OutlineID, int SiblingID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMoveOutlineAfter(_instanceID, OutlineID, SiblingID);
        }

        public int MoveOutlineBefore(int OutlineID, int SiblingID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMoveOutlineBefore(_instanceID, OutlineID, SiblingID);
        }

        public int MovePage(int NewPosition)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMovePage(_instanceID, NewPosition);
        }

        public int MovePath(double NewX, double NewY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMovePath(_instanceID, NewX, NewY);
        }

        public int MultiplyScale(double MultScaleBy)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryMultiplyScale(_instanceID, MultScaleBy);
        }

        public int NewCMYKAxialShader(string ShaderName, double StartX, double StartY,
            double StartCyan, double StartMagenta, double StartYellow, double StartBlack,
            double EndX, double EndY, double EndCyan, double EndMagenta, double EndYellow,
            double EndBlack, int Extend)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewCMYKAxialShader(_instanceID, ShaderName, StartX, StartY,
                    StartCyan, StartMagenta, StartYellow, StartBlack, EndX, EndY, EndCyan,
                    EndMagenta, EndYellow, EndBlack, Extend);
        }

        public int NewChildFormField(int Index, string Title, int FieldType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewChildFormField(_instanceID, Index, Title, FieldType);
        }

        public int NewContentStream()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewContentStream(_instanceID);
        }

        public string NewCustomPrinter(string OriginalPrinterName)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryNewCustomPrinter(_instanceID, OriginalPrinterName));
        }

        public int NewDestination(int DestPage, int Zoom, int DestType, double Left, double Top,
            double Right, double Bottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewDestination(_instanceID, DestPage, Zoom, DestType, Left,
                    Top, Right, Bottom);
        }

        public int NewDocument()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewDocument(_instanceID);
        }

        public int NewFormField(string Title, int FieldType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewFormField(_instanceID, Title, FieldType);
        }

        public int NewInternalPrinterObject(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewInternalPrinterObject(_instanceID, Options);
        }

        public int NewNamedDestination(string DestName, int DestID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewNamedDestination(_instanceID, DestName, DestID);
        }

        public int NewOptionalContentGroup(string GroupName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewOptionalContentGroup(_instanceID, GroupName);
        }

        public int NewOutline(int Parent, string Title, int DestPage, double DestPosition)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewOutline(_instanceID, Parent, Title, DestPage,
                    DestPosition);
        }

        public int NewPage()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewPage(_instanceID);
        }

        public int NewPageFromCanvasDC(double DPI, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewPageFromCanvasDC(_instanceID, DPI, Options);
        }

        public int NewPages(int PageCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewPages(_instanceID, PageCount);
        }

        public int NewPostScriptXObject(string PS)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewPostScriptXObject(_instanceID, PS);
        }

        public int NewRGBAxialShader(string ShaderName, double StartX, double StartY,
            double StartRed, double StartGreen, double StartBlue, double EndX, double EndY,
            double EndRed, double EndGreen, double EndBlue, int Extend)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewRGBAxialShader(_instanceID, ShaderName, StartX, StartY,
                    StartRed, StartGreen, StartBlue, EndX, EndY, EndRed, EndGreen, EndBlue, Extend);
        }

        public int NewSignProcessFromFile(string InputFile, string Password)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewSignProcessFromFile(_instanceID, InputFile, Password);
        }

        public int NewSignProcessFromString(byte[] Source, string Password)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibraryNewSignProcessFromString(_instanceID, bufferID,
                    Password);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int NewStaticOutline(int Parent, string Title)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewStaticOutline(_instanceID, Parent, Title);
        }

        public int NewTilingPatternFromCapturedPage(string PatternName, int CaptureID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNewTilingPatternFromCapturedPage(_instanceID, PatternName,
                    CaptureID);
        }

        public int NoEmbedFontListAdd(string FontName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNoEmbedFontListAdd(_instanceID, FontName);
        }

        public int NoEmbedFontListCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNoEmbedFontListCount(_instanceID);
        }

        public string NoEmbedFontListGet(int Index)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryNoEmbedFontListGet(_instanceID, Index));
        }

        public int NoEmbedFontListRemoveAll()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNoEmbedFontListRemoveAll(_instanceID);
        }

        public int NoEmbedFontListRemoveIndex(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNoEmbedFontListRemoveIndex(_instanceID, Index);
        }

        public int NoEmbedFontListRemoveName(string FontName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNoEmbedFontListRemoveName(_instanceID, FontName);
        }

        public int NormalizePage(int NormalizeOptions)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryNormalizePage(_instanceID, NormalizeOptions);
        }

        public int OpenOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryOpenOutline(_instanceID, OutlineID);
        }

        public int OptionalContentGroupCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryOptionalContentGroupCount(_instanceID);
        }

        public int OutlineCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryOutlineCount(_instanceID);
        }

        public string OutlineTitle(int OutlineID)
        {
            if (_dll == null) return "";
            else
                return SR(_dll.DebenuPDFLibraryOutlineTitle(_instanceID, OutlineID));
        }

        public int PDFiumPrintDocument(string PrinterName, int StartPage, int EndPage, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPDFiumPrintDocument(_instanceID, PrinterName, StartPage,
                    EndPage, Options);
        }

        public int PageCount()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageCount(_instanceID);
        }

        public int PageHasFontResources(int PageNumber)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageHasFontResources(_instanceID, PageNumber);
        }

        public double PageHeight()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageHeight(_instanceID);
        }

        public int PageJavaScriptAction(string ActionType, string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageJavaScriptAction(_instanceID, ActionType, JavaScript);
        }

        public int PageRotation()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageRotation(_instanceID);
        }

        public double PageWidth()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPageWidth(_instanceID);
        }

        public int PrintDocument(string PrinterName, int StartPage, int EndPage, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintDocument(_instanceID, PrinterName, StartPage, EndPage,
                    Options);
        }

        public int PrintDocumentToFile(string PrinterName, int StartPage, int EndPage, int Options,
            string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintDocumentToFile(_instanceID, PrinterName, StartPage,
                    EndPage, Options, FileName);
        }

        public int PrintMode(int Mode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintMode(_instanceID, Mode);
        }

        public int PrintOptions(int PageScaling, int AutoRotateCenter, string Title)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintOptions(_instanceID, PageScaling, AutoRotateCenter,
                    Title);
        }

        public int PrintPages(string PrinterName, string PageRanges, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintPages(_instanceID, PrinterName, PageRanges, Options);
        }

        public int PrintPagesToFile(string PrinterName, string PageRanges, int Options,
            string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryPrintPagesToFile(_instanceID, PrinterName, PageRanges,
                    Options, FileName);
        }

        public int ReduceSize(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReduceSize(_instanceID, Options);
        }

        public int ReleaseImage(int ImageID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReleaseImage(_instanceID, ImageID);
        }

        public int ReleaseImageList(int ImageListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReleaseImageList(_instanceID, ImageListID);
        }

        public int ReleaseSignProcess(int SignProcessID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReleaseSignProcess(_instanceID, SignProcessID);
        }

        public int ReleaseStringList(int StringListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReleaseStringList(_instanceID, StringListID);
        }

        public int ReleaseTextBlocks(int TextBlockListID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReleaseTextBlocks(_instanceID, TextBlockListID);
        }

        public int RemoveAppearanceStream(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveAppearanceStream(_instanceID, Index);
        }

        public int RemoveCustomInformation(string Key)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveCustomInformation(_instanceID, Key);
        }

        public int RemoveDocument(int DocumentID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveDocument(_instanceID, DocumentID);
        }

        public int RemoveEmbeddedFile(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveEmbeddedFile(_instanceID, Index);
        }

        public int RemoveFormFieldBackgroundColor(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveFormFieldBackgroundColor(_instanceID, Index);
        }

        public int RemoveFormFieldBorderColor(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveFormFieldBorderColor(_instanceID, Index);
        }

        public int RemoveFormFieldChoiceSub(int Index, string SubName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveFormFieldChoiceSub(_instanceID, Index, SubName);
        }

        public int RemoveGlobalJavaScript(string PackageName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveGlobalJavaScript(_instanceID, PackageName);
        }

        public int RemoveOpenAction()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveOpenAction(_instanceID);
        }

        public int RemoveOutline(int OutlineID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveOutline(_instanceID, OutlineID);
        }

        public int RemovePageBox(int BoxType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemovePageBox(_instanceID, BoxType);
        }

        public int RemoveSharedContentStreams()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveSharedContentStreams(_instanceID);
        }

        public int RemoveStyle(string StyleName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveStyle(_instanceID, StyleName);
        }

        public int RemoveUsageRights()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveUsageRights(_instanceID);
        }

        public int RemoveXFAEntries(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRemoveXFAEntries(_instanceID, Options);
        }

        public int RenderAsMultipageTIFFToFile(double DPI, string PageRanges, int ImageOptions,
            int OutputOptions, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRenderAsMultipageTIFFToFile(_instanceID, DPI, PageRanges,
                    ImageOptions, OutputOptions, FileName);
        }

        public int RenderDocumentToFile(double DPI, int StartPage, int EndPage, int Options,
            string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRenderDocumentToFile(_instanceID, DPI, StartPage, EndPage,
                    Options, FileName);
        }

        public int RenderPageToDC(double DPI, int Page, IntPtr DC)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRenderPageToDC(_instanceID, DPI, Page, DC);
        }

        public int RenderPageToFile(double DPI, int Page, int Options, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRenderPageToFile(_instanceID, DPI, Page, Options, FileName);
        }

        public byte[] RenderPageToString(double DPI, int Page, int Options)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryRenderPageToString(_instanceID, DPI, Page, Options);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int ReplaceFonts(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReplaceFonts(_instanceID, Options);
        }

        public int ReplaceImage(int OriginalImageID, int NewImageID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReplaceImage(_instanceID, OriginalImageID, NewImageID);
        }

        public int ReplaceTag(string Tag, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReplaceTag(_instanceID, Tag, NewValue);
        }

        public int RequestPrinterStatus(int StatusCommand)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRequestPrinterStatus(_instanceID, StatusCommand);
        }

        public int RetrieveCustomDataToFile(string Key, string FileName, int Location)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRetrieveCustomDataToFile(_instanceID, Key, FileName,
                    Location);
        }

        public byte[] RetrieveCustomDataToString(string Key, int Location)
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibraryRetrieveCustomDataToString(_instanceID, Key,
                    Location);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int ReverseImage(int Reset)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryReverseImage(_instanceID, Reset);
        }

        public int RotatePage(int PageRotation)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryRotatePage(_instanceID, PageRotation);
        }

        public int SaveFontToFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveFontToFile(_instanceID, FileName);
        }

        public int SaveImageListItemDataToFile(int ImageListID, int ImageIndex, int Options,
            string ImageFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveImageListItemDataToFile(_instanceID, ImageListID,
                    ImageIndex, Options, ImageFileName);
        }

        public int SaveImageToFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveImageToFile(_instanceID, FileName);
        }

        public byte[] SaveImageToString()
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibrarySaveImageToString(_instanceID);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int SaveState()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveState(_instanceID);
        }

        public int SaveStyle(string StyleName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveStyle(_instanceID, StyleName);
        }

        public int SaveToFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySaveToFile(_instanceID, FileName);
        }

        public byte[] SaveToString()
        {
            if (_dll == null) return new byte[0];
            else
            {
                IntPtr data = _dll.DebenuPDFLibrarySaveToString(_instanceID);
                int size = _dll.DebenuPDFLibraryAnsiStringResultLength(_instanceID);
                byte[] result = new byte[size];
                Marshal.Copy(data, result, 0, size);
                return result;
            }
        }

        public int SecurityInfo(int SecurityItem)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySecurityInfo(_instanceID, SecurityItem);
        }

        public int SelectContentStream(int NewIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectContentStream(_instanceID, NewIndex);
        }

        public int SelectDocument(int DocumentID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectDocument(_instanceID, DocumentID);
        }

        public int SelectFont(int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectFont(_instanceID, FontID);
        }

        public int SelectImage(int ImageID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectImage(_instanceID, ImageID);
        }

        public int SelectPage(int PageNumber)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectPage(_instanceID, PageNumber);
        }

        public int SelectRenderer(int RendererID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectRenderer(_instanceID, RendererID);
        }

        public int SelectedDocument()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectedDocument(_instanceID);
        }

        public int SelectedFont()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectedFont(_instanceID);
        }

        public int SelectedImage()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectedImage(_instanceID);
        }

        public int SelectedPage()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectedPage(_instanceID);
        }

        public int SelectedRenderer()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySelectedRenderer(_instanceID);
        }

        public int SetActionURL(int ActionID, string NewURL)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetActionURL(_instanceID, ActionID, NewURL);
        }

        public int SetAnnotBorderColor(int Index, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotBorderColor(_instanceID, Index, Red, Green, Blue);
        }

        public int SetAnnotBorderStyle(int Index, double Width, int Style, double DashOn,
            double DashOff)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotBorderStyle(_instanceID, Index, Width, Style,
                    DashOn, DashOff);
        }

        public int SetAnnotContents(int Index, string NewContents)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotContents(_instanceID, Index, NewContents);
        }

        public int SetAnnotDblProperty(int Index, int Tag, double NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotDblProperty(_instanceID, Index, Tag, NewValue);
        }

        public int SetAnnotIntProperty(int Index, int Tag, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotIntProperty(_instanceID, Index, Tag, NewValue);
        }

        public int SetAnnotOptional(int Index, int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotOptional(_instanceID, Index,
                    OptionalContentGroupID);
        }

        public int SetAnnotQuadPoints(int Index, int QuadNumber, double X1, double Y1, double X2,
            double Y2, double X3, double Y3, double X4, double Y4)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotQuadPoints(_instanceID, Index, QuadNumber, X1, Y1,
                    X2, Y2, X3, Y3, X4, Y4);
        }

        public int SetAnnotRect(int Index, double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotRect(_instanceID, Index, Left, Top, Width, Height);
        }

        public int SetAnnotStrProperty(int Index, int Tag, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnnotStrProperty(_instanceID, Index, Tag, NewValue);
        }

        public int SetAnsiMode(int NewAnsiMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetAnsiMode(_instanceID, NewAnsiMode);
        }

        public int SetAppendInputFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetAppendInputFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetBaseURL(string NewBaseURL)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetBaseURL(_instanceID, NewBaseURL);
        }

        public int SetBlendMode(int BlendMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetBlendMode(_instanceID, BlendMode);
        }

        public int SetBreakString(string NewBreakString)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetBreakString(_instanceID, NewBreakString);
        }

        public int SetCSDictEPSG(int CSDictID, int NewEPSG)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCSDictEPSG(_instanceID, CSDictID, NewEPSG);
        }

        public int SetCSDictType(int CSDictID, int NewDictType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCSDictType(_instanceID, CSDictID, NewDictType);
        }

        public int SetCSDictWKT(int CSDictID, string NewWKT)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCSDictWKT(_instanceID, CSDictID, NewWKT);
        }

        public int SetCairoFileName(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCairoFileName(_instanceID, FileName);
        }

        public int SetCapturedPageOptional(int CaptureID, int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCapturedPageOptional(_instanceID, CaptureID,
                    OptionalContentGroupID);
        }

        public int SetCapturedPageTransparencyGroup(int CaptureID, int CS, int Isolate, int Knockout)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCapturedPageTransparencyGroup(_instanceID, CaptureID,
                    CS, Isolate, Knockout);
        }

        public int SetCatalogInformation(string Key, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCatalogInformation(_instanceID, Key, NewValue);
        }

        public int SetCharWidth(int CharCode, int NewWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCharWidth(_instanceID, CharCode, NewWidth);
        }

        public int SetClippingPath()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetClippingPath(_instanceID);
        }

        public int SetClippingPathEvenOdd()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetClippingPathEvenOdd(_instanceID);
        }

        public int SetCompatibility(int CompatibilityItem, int CompatibilityMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCompatibility(_instanceID, CompatibilityItem,
                    CompatibilityMode);
        }

        public int SetContentStreamFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetContentStreamFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetContentStreamOptional(int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetContentStreamOptional(_instanceID,
                    OptionalContentGroupID);
        }

        public int SetCropBox(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCropBox(_instanceID, Left, Top, Width, Height);
        }

        public int SetCustomInformation(string Key, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCustomInformation(_instanceID, Key, NewValue);
        }

        public int SetCustomLineDash(string DashPattern, double DashPhase)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetCustomLineDash(_instanceID, DashPattern, DashPhase);
        }

        public int SetDPLRFileName(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetDPLRFileName(_instanceID, FileName);
        }

        public int SetDecodeMode(int NewDecodeMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetDecodeMode(_instanceID, NewDecodeMode);
        }

        public int SetDestProperties(int DestID, int Zoom, int DestType, double Left, double Top,
            double Right, double Bottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetDestProperties(_instanceID, DestID, Zoom, DestType,
                    Left, Top, Right, Bottom);
        }

        public int SetDestValue(int DestID, int ValueKey, double NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetDestValue(_instanceID, DestID, ValueKey, NewValue);
        }

        public int SetDocumentMetadata(string XMP)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetDocumentMetadata(_instanceID, XMP);
        }

        public int SetEmbeddedFileStrProperty(int Index, int Tag, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetEmbeddedFileStrProperty(_instanceID, Index, Tag,
                    NewValue);
        }

        public int SetFillColor(double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFillColor(_instanceID, Red, Green, Blue);
        }

        public int SetFillColorCMYK(double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFillColorCMYK(_instanceID, C, M, Y, K);
        }

        public int SetFillColorSep(string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFillColorSep(_instanceID, ColorName, Tint);
        }

        public int SetFillShader(string ShaderName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFillShader(_instanceID, ShaderName);
        }

        public int SetFillTilingPattern(string PatternName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFillTilingPattern(_instanceID, PatternName);
        }

        public int SetFindImagesMode(int NewFindImagesMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFindImagesMode(_instanceID, NewFindImagesMode);
        }

        public int SetFontEncoding(int Encoding)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFontEncoding(_instanceID, Encoding);
        }

        public int SetFontFlags(int Fixed, int Serif, int Symbolic, int Script, int Italic,
            int AllCap, int SmallCap, int ForceBold)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFontFlags(_instanceID, Fixed, Serif, Symbolic, Script,
                    Italic, AllCap, SmallCap, ForceBold);
        }

        public int SetFormFieldAlignment(int Index, int Alignment)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldAlignment(_instanceID, Index, Alignment);
        }

        public int SetFormFieldAnnotFlags(int Index, int NewFlags)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldAnnotFlags(_instanceID, Index, NewFlags);
        }

        public int SetFormFieldAppearanceFromString(int Index, byte[] Source, int FontID,
            string FontReference)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetFormFieldAppearanceFromString(_instanceID, Index,
                    bufferID, FontID, FontReference);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetFormFieldBackgroundColor(int Index, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBackgroundColor(_instanceID, Index, Red, Green,
                    Blue);
        }

        public int SetFormFieldBackgroundColorCMYK(int Index, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBackgroundColorCMYK(_instanceID, Index, C, M,
                    Y, K);
        }

        public int SetFormFieldBackgroundColorGray(int Index, double Gray)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBackgroundColorGray(_instanceID, Index, Gray);
        }

        public int SetFormFieldBackgroundColorSep(int Index, string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBackgroundColorSep(_instanceID, Index,
                    ColorName, Tint);
        }

        public int SetFormFieldBorderColor(int Index, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBorderColor(_instanceID, Index, Red, Green,
                    Blue);
        }

        public int SetFormFieldBorderColorCMYK(int Index, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBorderColorCMYK(_instanceID, Index, C, M, Y, K);
        }

        public int SetFormFieldBorderColorGray(int Index, double Gray)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBorderColorGray(_instanceID, Index, Gray);
        }

        public int SetFormFieldBorderColorSep(int Index, string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBorderColorSep(_instanceID, Index, ColorName,
                    Tint);
        }

        public int SetFormFieldBorderStyle(int Index, double Width, int Style, double DashOn,
            double DashOff)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBorderStyle(_instanceID, Index, Width, Style,
                    DashOn, DashOff);
        }

        public int SetFormFieldBounds(int Index, double Left, double Top, double Width,
            double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldBounds(_instanceID, Index, Left, Top, Width,
                    Height);
        }

        public int SetFormFieldCalcOrder(int Index, int Order)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldCalcOrder(_instanceID, Index, Order);
        }

        public int SetFormFieldCaption(int Index, string NewCaption)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldCaption(_instanceID, Index, NewCaption);
        }

        public int SetFormFieldCheckStyle(int Index, int CheckStyle, int Position)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldCheckStyle(_instanceID, Index, CheckStyle,
                    Position);
        }

        public int SetFormFieldCheckboxColor(int Index, int SetType, double Red, double Green,
            double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldCheckboxColor(_instanceID, Index, SetType, Red,
                    Green, Blue);
        }

        public int SetFormFieldChildTitle(int Index, string NewTitle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldChildTitle(_instanceID, Index, NewTitle);
        }

        public int SetFormFieldChoiceSub(int Index, int SubIndex, string SubName, string DisplayName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldChoiceSub(_instanceID, Index, SubIndex,
                    SubName, DisplayName);
        }

        public int SetFormFieldChoiceType(int Index, int ChoiceType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldChoiceType(_instanceID, Index, ChoiceType);
        }

        public int SetFormFieldColor(int Index, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldColor(_instanceID, Index, Red, Green, Blue);
        }

        public int SetFormFieldColorCMYK(int Index, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldColorCMYK(_instanceID, Index, C, M, Y, K);
        }

        public int SetFormFieldColorSep(int Index, string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldColorSep(_instanceID, Index, ColorName, Tint);
        }

        public int SetFormFieldComb(int Index, int Comb)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldComb(_instanceID, Index, Comb);
        }

        public int SetFormFieldCustomDict(int Index, string Key, string NewValue, int StorageType,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldCustomDict(_instanceID, Index, Key, NewValue,
                    StorageType, Options);
        }

        public int SetFormFieldDefaultValue(int Index, string NewDefaultValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldDefaultValue(_instanceID, Index,
                    NewDefaultValue);
        }

        public int SetFormFieldDefaultValueEx(int Index, string NewDefaultValue, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldDefaultValueEx(_instanceID, Index,
                    NewDefaultValue, Options);
        }

        public int SetFormFieldDescription(int Index, string NewDescription)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldDescription(_instanceID, Index, NewDescription);
        }

        public int SetFormFieldFlags(int Index, int NewFlags)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldFlags(_instanceID, Index, NewFlags);
        }

        public int SetFormFieldFont(int Index, int FontIndex)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldFont(_instanceID, Index, FontIndex);
        }

        public int SetFormFieldFormatMode(int NewFormatMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldFormatMode(_instanceID, NewFormatMode);
        }

        public int SetFormFieldHighlightMode(int Index, int NewMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldHighlightMode(_instanceID, Index, NewMode);
        }

        public int SetFormFieldIcon(int Index, int IconType, int CaptureID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldIcon(_instanceID, Index, IconType, CaptureID);
        }

        public int SetFormFieldIconStyle(int Index, int Placement, int Scale, int ScaleType,
            int HorizontalShift, int VerticalShift)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldIconStyle(_instanceID, Index, Placement, Scale,
                    ScaleType, HorizontalShift, VerticalShift);
        }

        public int SetFormFieldLockAction(int Index, int LockAction, string FieldList,
            string Delimiter)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldLockAction(_instanceID, Index, LockAction,
                    FieldList, Delimiter);
        }

        public int SetFormFieldMaxLen(int Index, int NewMaxLen)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldMaxLen(_instanceID, Index, NewMaxLen);
        }

        public int SetFormFieldMetadata(int Index, string XMP)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldMetadata(_instanceID, Index, XMP);
        }

        public int SetFormFieldNoExport(int Index, int NoExport)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldNoExport(_instanceID, Index, NoExport);
        }

        public int SetFormFieldOptional(int Index, int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldOptional(_instanceID, Index,
                    OptionalContentGroupID);
        }

        public int SetFormFieldPage(int Index, int NewPage)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldPage(_instanceID, Index, NewPage);
        }

        public int SetFormFieldPrintable(int Index, int Printable)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldPrintable(_instanceID, Index, Printable);
        }

        public int SetFormFieldReadOnly(int Index, int ReadOnly)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldReadOnly(_instanceID, Index, ReadOnly);
        }

        public int SetFormFieldRequired(int Index, int Required)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldRequired(_instanceID, Index, Required);
        }

        public int SetFormFieldResetAction(int Index, string ActionType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldResetAction(_instanceID, Index, ActionType);
        }

        public int SetFormFieldRichTextString(int Index, string Key, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldRichTextString(_instanceID, Index, Key,
                    NewValue);
        }

        public int SetFormFieldRotation(int Index, int Angle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldRotation(_instanceID, Index, Angle);
        }

        public int SetFormFieldSignatureImage(int Index, int ImageID, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldSignatureImage(_instanceID, Index, ImageID,
                    Options);
        }

        public int SetFormFieldStandardFont(int Index, int StandardFontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldStandardFont(_instanceID, Index,
                    StandardFontID);
        }

        public int SetFormFieldSubmitAction(int Index, string ActionType, string Link)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldSubmitAction(_instanceID, Index, ActionType,
                    Link);
        }

        public int SetFormFieldSubmitActionEx(int Index, string ActionType, string Link, int Flags)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldSubmitActionEx(_instanceID, Index, ActionType,
                    Link, Flags);
        }

        public int SetFormFieldTabOrder(int Index, int Order)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldTabOrder(_instanceID, Index, Order);
        }

        public int SetFormFieldTextFlags(int Index, int Multiline, int Password, int FileSelect,
            int DoNotSpellCheck, int DoNotScroll)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldTextFlags(_instanceID, Index, Multiline,
                    Password, FileSelect, DoNotSpellCheck, DoNotScroll);
        }

        public int SetFormFieldTextSize(int Index, double NewTextSize)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldTextSize(_instanceID, Index, NewTextSize);
        }

        public int SetFormFieldTitle(int Index, string NewTitle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldTitle(_instanceID, Index, NewTitle);
        }

        public int SetFormFieldValue(int Index, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldValue(_instanceID, Index, NewValue);
        }

        public int SetFormFieldValueByTitle(string Title, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldValueByTitle(_instanceID, Title, NewValue);
        }

        public int SetFormFieldValueEx(int Index, string NewValue, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldValueEx(_instanceID, Index, NewValue, Options);
        }

        public int SetFormFieldVisible(int Index, int Visible)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetFormFieldVisible(_instanceID, Index, Visible);
        }

        public int SetGDIPlusFileName(string DLLFileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetGDIPlusFileName(_instanceID, DLLFileName);
        }

        public int SetGDIPlusOptions(int OptionID, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetGDIPlusOptions(_instanceID, OptionID, NewValue);
        }

        public int SetHTMLBoldFont(string FontSet, int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetHTMLBoldFont(_instanceID, FontSet, FontID);
        }

        public int SetHTMLBoldItalicFont(string FontSet, int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetHTMLBoldItalicFont(_instanceID, FontSet, FontID);
        }

        public int SetHTMLItalicFont(string FontSet, int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetHTMLItalicFont(_instanceID, FontSet, FontID);
        }

        public int SetHTMLNormalFont(string FontSet, int FontID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetHTMLNormalFont(_instanceID, FontSet, FontID);
        }

        public int SetHeaderCommentsFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetHeaderCommentsFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetImageAsMask(int MaskType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageAsMask(_instanceID, MaskType);
        }

        public int SetImageMask(double FromRed, double FromGreen, double FromBlue, double ToRed,
            double ToGreen, double ToBlue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageMask(_instanceID, FromRed, FromGreen, FromBlue,
                    ToRed, ToGreen, ToBlue);
        }

        public int SetImageMaskCMYK(double FromC, double FromM, double FromY, double FromK,
            double ToC, double ToM, double ToY, double ToK)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageMaskCMYK(_instanceID, FromC, FromM, FromY, FromK,
                    ToC, ToM, ToY, ToK);
        }

        public int SetImageMaskFromImage(int ImageID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageMaskFromImage(_instanceID, ImageID);
        }

        public int SetImageOptional(int OptionalContentGroupID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageOptional(_instanceID, OptionalContentGroupID);
        }

        public int SetImageResolution(int Horizontal, int Vertical, int Units)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetImageResolution(_instanceID, Horizontal, Vertical,
                    Units);
        }

        public int SetInformation(int Key, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetInformation(_instanceID, Key, NewValue);
        }

        public int SetJPEGQuality(int Quality)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetJPEGQuality(_instanceID, Quality);
        }

        public int SetJavaScriptMode(int JSMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetJavaScriptMode(_instanceID, JSMode);
        }

        public int SetKerning(string CharPair, int Adjustment)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetKerning(_instanceID, CharPair, Adjustment);
        }

        public int SetLineCap(int LineCap)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineCap(_instanceID, LineCap);
        }

        public int SetLineColor(double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineColor(_instanceID, Red, Green, Blue);
        }

        public int SetLineColorCMYK(double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineColorCMYK(_instanceID, C, M, Y, K);
        }

        public int SetLineColorSep(string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineColorSep(_instanceID, ColorName, Tint);
        }

        public int SetLineDash(double DashOn, double DashOff)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineDash(_instanceID, DashOn, DashOff);
        }

        public int SetLineDashEx(string DashValues)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineDashEx(_instanceID, DashValues);
        }

        public int SetLineJoin(int LineJoin)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineJoin(_instanceID, LineJoin);
        }

        public int SetLineShader(string ShaderName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineShader(_instanceID, ShaderName);
        }

        public int SetLineWidth(double LineWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetLineWidth(_instanceID, LineWidth);
        }

        public int SetMarkupAnnotStyle(int Index, double Red, double Green, double Blue,
            double Transparency)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMarkupAnnotStyle(_instanceID, Index, Red, Green, Blue,
                    Transparency);
        }

        public int SetMeasureDictBoundsCount(int MeasureDictID, int NewCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictBoundsCount(_instanceID, MeasureDictID,
                    NewCount);
        }

        public int SetMeasureDictBoundsItem(int MeasureDictID, int ItemIndex, double NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictBoundsItem(_instanceID, MeasureDictID,
                    ItemIndex, NewValue);
        }

        public int SetMeasureDictCoordinateSystem(int MeasureDictID, int CoordinateSystemID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictCoordinateSystem(_instanceID, MeasureDictID,
                    CoordinateSystemID);
        }

        public int SetMeasureDictGPTSCount(int MeasureDictID, int NewCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictGPTSCount(_instanceID, MeasureDictID,
                    NewCount);
        }

        public int SetMeasureDictGPTSItem(int MeasureDictID, int ItemIndex, double NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictGPTSItem(_instanceID, MeasureDictID,
                    ItemIndex, NewValue);
        }

        public int SetMeasureDictLPTSCount(int MeasureDictID, int NewCount)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictLPTSCount(_instanceID, MeasureDictID,
                    NewCount);
        }

        public int SetMeasureDictLPTSItem(int MeasureDictID, int ItemIndex, double NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictLPTSItem(_instanceID, MeasureDictID,
                    ItemIndex, NewValue);
        }

        public int SetMeasureDictPDU(int MeasureDictID, int LinearUnit, int AreaUnit,
            int AngularUnit)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasureDictPDU(_instanceID, MeasureDictID, LinearUnit,
                    AreaUnit, AngularUnit);
        }

        public int SetMeasurementUnits(int MeasurementUnits)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMeasurementUnits(_instanceID, MeasurementUnits);
        }

        public int SetMetafileMode(int Mode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetMetafileMode(_instanceID, Mode);
        }

        public int SetNeedAppearances(int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetNeedAppearances(_instanceID, NewValue);
        }

        public int SetObjectAsStreamFromString(int ObjectNumber, byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetObjectAsStreamFromString(_instanceID,
                    ObjectNumber, bufferID, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetObjectFromString(int ObjectNumber, byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetObjectFromString(_instanceID, ObjectNumber,
                    bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetOpenActionDestination(int OpenPage, int Zoom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOpenActionDestination(_instanceID, OpenPage, Zoom);
        }

        public int SetOpenActionDestinationFull(int OpenPage, int Zoom, int DestType, double Left,
            double Top, double Right, double Bottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOpenActionDestinationFull(_instanceID, OpenPage, Zoom,
                    DestType, Left, Top, Right, Bottom);
        }

        public int SetOpenActionJavaScript(string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOpenActionJavaScript(_instanceID, JavaScript);
        }

        public int SetOpenActionMenu(string MenuItem)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOpenActionMenu(_instanceID, MenuItem);
        }

        public int SetOptionalContentConfigLocked(int OptionalContentConfigID,
            int OptionalContentGroupID, int NewLocked)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOptionalContentConfigLocked(_instanceID,
                    OptionalContentConfigID, OptionalContentGroupID, NewLocked);
        }

        public int SetOptionalContentConfigState(int OptionalContentConfigID,
            int OptionalContentGroupID, int NewState)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOptionalContentConfigState(_instanceID,
                    OptionalContentConfigID, OptionalContentGroupID, NewState);
        }

        public int SetOptionalContentGroupName(int OptionalContentGroupID, string NewGroupName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOptionalContentGroupName(_instanceID,
                    OptionalContentGroupID, NewGroupName);
        }

        public int SetOptionalContentGroupPrintable(int OptionalContentGroupID, int Printable)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOptionalContentGroupPrintable(_instanceID,
                    OptionalContentGroupID, Printable);
        }

        public int SetOptionalContentGroupVisible(int OptionalContentGroupID, int Visible)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOptionalContentGroupVisible(_instanceID,
                    OptionalContentGroupID, Visible);
        }

        public int SetOrigin(int Origin)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOrigin(_instanceID, Origin);
        }

        public int SetOutlineColor(int OutlineID, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineColor(_instanceID, OutlineID, Red, Green, Blue);
        }

        public int SetOutlineDestination(int OutlineID, int DestPage, double DestPosition)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineDestination(_instanceID, OutlineID, DestPage,
                    DestPosition);
        }

        public int SetOutlineDestinationFull(int OutlineID, int DestPage, int Zoom, int DestType,
            double Left, double Top, double Right, double Bottom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineDestinationFull(_instanceID, OutlineID, DestPage,
                    Zoom, DestType, Left, Top, Right, Bottom);
        }

        public int SetOutlineDestinationZoom(int OutlineID, int DestPage, double DestPosition,
            int Zoom)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineDestinationZoom(_instanceID, OutlineID, DestPage,
                    DestPosition, Zoom);
        }

        public int SetOutlineJavaScript(int OutlineID, string JavaScript)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineJavaScript(_instanceID, OutlineID, JavaScript);
        }

        public int SetOutlineNamedDestination(int OutlineID, string DestName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineNamedDestination(_instanceID, OutlineID,
                    DestName);
        }

        public int SetOutlineOpenFile(int OutlineID, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineOpenFile(_instanceID, OutlineID, FileName);
        }

        public int SetOutlineRemoteDestination(int OutlineID, string FileName, int OpenPage,
            int Zoom, int DestType, double PntLeft, double PntTop, double PntRight, double PntBottom,
            int NewWindow)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineRemoteDestination(_instanceID, OutlineID,
                    FileName, OpenPage, Zoom, DestType, PntLeft, PntTop, PntRight, PntBottom,
                    NewWindow);
        }

        public int SetOutlineStyle(int OutlineID, int SetItalic, int SetBold)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineStyle(_instanceID, OutlineID, SetItalic, SetBold);
        }

        public int SetOutlineTitle(int OutlineID, string NewTitle)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineTitle(_instanceID, OutlineID, NewTitle);
        }

        public int SetOutlineWebLink(int OutlineID, string Link)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOutlineWebLink(_instanceID, OutlineID, Link);
        }

        public int SetOverprint(int StrokingOverprint, int OtherOverprint, int OverprintMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetOverprint(_instanceID, StrokingOverprint,
                    OtherOverprint, OverprintMode);
        }

        public int SetPDFAMode(int NewMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPDFAMode(_instanceID, NewMode);
        }

        public int SetPDFiumFileName(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPDFiumFileName(_instanceID, FileName);
        }

        public int SetPDFiumRenderFlags(int NewRenderFlags)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPDFiumRenderFlags(_instanceID, NewRenderFlags);
        }

        public int SetPNGTransparencyColor(int RedByte, int GreenByte, int BlueByte)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPNGTransparencyColor(_instanceID, RedByte, GreenByte,
                    BlueByte);
        }

        public int SetPageActionMenu(string MenuItem)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageActionMenu(_instanceID, MenuItem);
        }

        public int SetPageBox(int BoxType, double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageBox(_instanceID, BoxType, Left, Top, Width, Height);
        }

        public int SetPageContentFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetPageContentFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetPageDimensions(double NewPageWidth, double NewPageHeight)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageDimensions(_instanceID, NewPageWidth, NewPageHeight);
        }

        public int SetPageLayout(int NewPageLayout)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageLayout(_instanceID, NewPageLayout);
        }

        public int SetPageMetadata(string XMP)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageMetadata(_instanceID, XMP);
        }

        public int SetPageMode(int NewPageMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageMode(_instanceID, NewPageMode);
        }

        public int SetPageSize(string PaperName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageSize(_instanceID, PaperName);
        }

        public int SetPageThumbnail()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageThumbnail(_instanceID);
        }

        public int SetPageTransparencyGroup(int CS, int Isolate, int Knockout)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageTransparencyGroup(_instanceID, CS, Isolate,
                    Knockout);
        }

        public int SetPageUserUnit(double UserUnit)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPageUserUnit(_instanceID, UserUnit);
        }

        public int SetPrecision(int NewPrecision)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetPrecision(_instanceID, NewPrecision);
        }

        public int SetPrinterDevModeFromString(byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetPrinterDevModeFromString(_instanceID, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetRenderArea(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderArea(_instanceID, Left, Top, Width, Height);
        }

        public int SetRenderCropType(int NewCropType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderCropType(_instanceID, NewCropType);
        }

        public int SetRenderDCErasePage(int NewErasePage)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderDCErasePage(_instanceID, NewErasePage);
        }

        public int SetRenderDCOffset(int NewOffsetX, int NewOffsetY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderDCOffset(_instanceID, NewOffsetX, NewOffsetY);
        }

        public int SetRenderOptions(int OptionID, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderOptions(_instanceID, OptionID, NewValue);
        }

        public int SetRenderScale(double NewScale)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetRenderScale(_instanceID, NewScale);
        }

        public int SetScale(double NewScale)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetScale(_instanceID, NewScale);
        }

        public int SetSignProcessAppearanceFromString(int SignProcessID, string LayerName,
            byte[] Source)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetSignProcessAppearanceFromString(_instanceID,
                    SignProcessID, LayerName, bufferID);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetSignProcessCertFromStore(int SignProcessID, string CertHash, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessCertFromStore(_instanceID, SignProcessID,
                    CertHash, Options);
        }

        public int SetSignProcessCustomDict(int SignProcessID, string Key, string NewValue,
            int StorageType)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessCustomDict(_instanceID, SignProcessID, Key,
                    NewValue, StorageType);
        }

        public int SetSignProcessCustomSubFilter(int SignProcessID, string SubFilterStr)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessCustomSubFilter(_instanceID, SignProcessID,
                    SubFilterStr);
        }

        public int SetSignProcessField(int SignProcessID, string SignatureFieldName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessField(_instanceID, SignProcessID,
                    SignatureFieldName);
        }

        public int SetSignProcessFieldBounds(int SignProcessID, double Left, double Top,
            double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessFieldBounds(_instanceID, SignProcessID, Left,
                    Top, Width, Height);
        }

        public int SetSignProcessFieldImageFromFile(int SignProcessID, string ImageFileName,
            int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessFieldImageFromFile(_instanceID,
                    SignProcessID, ImageFileName, Options);
        }

        public int SetSignProcessFieldImageFromString(int SignProcessID, byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetSignProcessFieldImageFromString(_instanceID,
                    SignProcessID, bufferID, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetSignProcessFieldLocked(int SignProcessID, int LockFieldAfterSign)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessFieldLocked(_instanceID, SignProcessID,
                    LockFieldAfterSign);
        }

        public int SetSignProcessFieldMetadata(int SignProcessID, string XMP)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessFieldMetadata(_instanceID, SignProcessID,
                    XMP);
        }

        public int SetSignProcessFieldPage(int SignProcessID, int SignaturePage)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessFieldPage(_instanceID, SignProcessID,
                    SignaturePage);
        }

        public int SetSignProcessImageLayer(int SignProcessID, string LayerName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessImageLayer(_instanceID, SignProcessID,
                    LayerName);
        }

        public int SetSignProcessInfo(int SignProcessID, string Reason, string Location,
            string ContactInfo)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessInfo(_instanceID, SignProcessID, Reason,
                    Location, ContactInfo);
        }

        public int SetSignProcessKeyset(int SignProcessID, int KeysetID)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessKeyset(_instanceID, SignProcessID, KeysetID);
        }

        public int SetSignProcessOpenSSLFileName(int SignProcessID, string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessOpenSSLFileName(_instanceID, SignProcessID,
                    FileName);
        }

        public int SetSignProcessOptions(int SignProcessID, int OptionID, int OptionValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessOptions(_instanceID, SignProcessID, OptionID,
                    OptionValue);
        }

        public int SetSignProcessPFXFromFile(int SignProcessID, string PFXFileName,
            string PFXPassword)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessPFXFromFile(_instanceID, SignProcessID,
                    PFXFileName, PFXPassword);
        }

        public int SetSignProcessPFXFromString(int SignProcessID, byte[] Source, string PFXPassword)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetSignProcessPFXFromString(_instanceID,
                    SignProcessID, bufferID, PFXPassword);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetSignProcessPassthrough(int SignProcessID, int SignatureLength)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessPassthrough(_instanceID, SignProcessID,
                    SignatureLength);
        }

        public int SetSignProcessSubFilter(int SignProcessID, int SubFilter)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessSubFilter(_instanceID, SignProcessID,
                    SubFilter);
        }

        public int SetSignProcessTimestampURL(int SignProcessID, string TimestampURL, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetSignProcessTimestampURL(_instanceID, SignProcessID,
                    TimestampURL, Options);
        }

        public int SetTabOrderMode(string Mode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTabOrderMode(_instanceID, Mode);
        }

        public int SetTableBorderColor(int TableID, int BorderIndex, double Red, double Green,
            double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableBorderColor(_instanceID, TableID, BorderIndex, Red,
                    Green, Blue);
        }

        public int SetTableBorderColorCMYK(int TableID, int BorderIndex, double C, double M,
            double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableBorderColorCMYK(_instanceID, TableID, BorderIndex,
                    C, M, Y, K);
        }

        public int SetTableBorderWidth(int TableID, int BorderIndex, double NewWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableBorderWidth(_instanceID, TableID, BorderIndex,
                    NewWidth);
        }

        public int SetTableCellAlignment(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, int NewCellAlignment)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellAlignment(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, NewCellAlignment);
        }

        public int SetTableCellBackgroundColor(int TableID, int FirstRow, int FirstColumn,
            int LastRow, int LastColumn, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellBackgroundColor(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, Red, Green, Blue);
        }

        public int SetTableCellBackgroundColorCMYK(int TableID, int FirstRow, int FirstColumn,
            int LastRow, int LastColumn, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellBackgroundColorCMYK(_instanceID, TableID,
                    FirstRow, FirstColumn, LastRow, LastColumn, C, M, Y, K);
        }

        public int SetTableCellBorderColor(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, int BorderIndex, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellBorderColor(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, BorderIndex, Red, Green, Blue);
        }

        public int SetTableCellBorderColorCMYK(int TableID, int FirstRow, int FirstColumn,
            int LastRow, int LastColumn, int BorderIndex, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellBorderColorCMYK(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, BorderIndex, C, M, Y, K);
        }

        public int SetTableCellBorderWidth(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, int BorderIndex, double NewWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellBorderWidth(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, BorderIndex, NewWidth);
        }

        public int SetTableCellContent(int TableID, int RowNumber, int ColumnNumber, string HTMLText)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellContent(_instanceID, TableID, RowNumber,
                    ColumnNumber, HTMLText);
        }

        public int SetTableCellPadding(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, int BorderIndex, double NewPadding)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellPadding(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, BorderIndex, NewPadding);
        }

        public int SetTableCellTextColor(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellTextColor(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, Red, Green, Blue);
        }

        public int SetTableCellTextColorCMYK(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellTextColorCMYK(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, C, M, Y, K);
        }

        public int SetTableCellTextSize(int TableID, int FirstRow, int FirstColumn, int LastRow,
            int LastColumn, double NewTextSize)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableCellTextSize(_instanceID, TableID, FirstRow,
                    FirstColumn, LastRow, LastColumn, NewTextSize);
        }

        public int SetTableColumnWidth(int TableID, int FirstColumn, int LastColumn, double NewWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableColumnWidth(_instanceID, TableID, FirstColumn,
                    LastColumn, NewWidth);
        }

        public int SetTableRowHeight(int TableID, int FirstRow, int LastRow, double NewHeight)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableRowHeight(_instanceID, TableID, FirstRow, LastRow,
                    NewHeight);
        }

        public int SetTableThinBorders(int TableID, int ThinBorders, double Red, double Green,
            double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableThinBorders(_instanceID, TableID, ThinBorders, Red,
                    Green, Blue);
        }

        public int SetTableThinBordersCMYK(int TableID, int ThinBorders, double C, double M,
            double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTableThinBordersCMYK(_instanceID, TableID, ThinBorders,
                    C, M, Y, K);
        }

        public int SetTempFile(string FileName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTempFile(_instanceID, FileName);
        }

        public int SetTempPath(string NewPath)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTempPath(_instanceID, NewPath);
        }

        public int SetTextAlign(int TextAlign)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextAlign(_instanceID, TextAlign);
        }

        public int SetTextCharSpacing(double CharSpacing)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextCharSpacing(_instanceID, CharSpacing);
        }

        public int SetTextColor(double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextColor(_instanceID, Red, Green, Blue);
        }

        public int SetTextColorCMYK(double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextColorCMYK(_instanceID, C, M, Y, K);
        }

        public int SetTextColorSep(string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextColorSep(_instanceID, ColorName, Tint);
        }

        public int SetTextExtractionArea(double Left, double Top, double Width, double Height)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextExtractionArea(_instanceID, Left, Top, Width,
                    Height);
        }

        public int SetTextExtractionOptions(int OptionID, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextExtractionOptions(_instanceID, OptionID, NewValue);
        }

        public int SetTextExtractionScaling(int Options, double Horizontal, double Vertical)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextExtractionScaling(_instanceID, Options, Horizontal,
                    Vertical);
        }

        public int SetTextExtractionWordGap(double NewWordGap)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextExtractionWordGap(_instanceID, NewWordGap);
        }

        public int SetTextHighlight(int Highlight)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextHighlight(_instanceID, Highlight);
        }

        public int SetTextHighlightColor(double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextHighlightColor(_instanceID, Red, Green, Blue);
        }

        public int SetTextHighlightColorCMYK(double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextHighlightColorCMYK(_instanceID, C, M, Y, K);
        }

        public int SetTextHighlightColorSep(string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextHighlightColorSep(_instanceID, ColorName, Tint);
        }

        public int SetTextMode(int TextMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextMode(_instanceID, TextMode);
        }

        public int SetTextRise(double Rise)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextRise(_instanceID, Rise);
        }

        public int SetTextScaling(double ScalePercentage)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextScaling(_instanceID, ScalePercentage);
        }

        public int SetTextShader(string ShaderName)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextShader(_instanceID, ShaderName);
        }

        public int SetTextSize(double TextSize)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextSize(_instanceID, TextSize);
        }

        public int SetTextSpacing(double Spacing)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextSpacing(_instanceID, Spacing);
        }

        public int SetTextUnderline(int Underline)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderline(_instanceID, Underline);
        }

        public int SetTextUnderlineColor(double Red, double Green, double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineColor(_instanceID, Red, Green, Blue);
        }

        public int SetTextUnderlineColorCMYK(double C, double M, double Y, double K)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineColorCMYK(_instanceID, C, M, Y, K);
        }

        public int SetTextUnderlineColorSep(string ColorName, double Tint)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineColorSep(_instanceID, ColorName, Tint);
        }

        public int SetTextUnderlineCustomDash(string DashPattern, double DashPhase)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineCustomDash(_instanceID, DashPattern,
                    DashPhase);
        }

        public int SetTextUnderlineDash(double DashOn, double DashOff)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineDash(_instanceID, DashOn, DashOff);
        }

        public int SetTextUnderlineDistance(double UnderlineDistance)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineDistance(_instanceID, UnderlineDistance);
        }

        public int SetTextUnderlineWidth(double UnderlineWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextUnderlineWidth(_instanceID, UnderlineWidth);
        }

        public int SetTextWordSpacing(double WordSpacing)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTextWordSpacing(_instanceID, WordSpacing);
        }

        public int SetTransparency(int Transparency)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetTransparency(_instanceID, Transparency);
        }

        public int SetUpdateMode(int NewUpdateMode)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetUpdateMode(_instanceID, NewUpdateMode);
        }

        public int SetViewerPreferences(int Option, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetViewerPreferences(_instanceID, Option, NewValue);
        }

        public int SetXFAFormFieldAccess(string XFAFieldName, int NewAccess)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetXFAFormFieldAccess(_instanceID, XFAFieldName, NewAccess);
        }

        public int SetXFAFormFieldBorderColor(string XFAFieldName, double Red, double Green,
            double Blue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetXFAFormFieldBorderColor(_instanceID, XFAFieldName, Red,
                    Green, Blue);
        }

        public int SetXFAFormFieldBorderPresence(string XFAFieldName, int NewPresence)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetXFAFormFieldBorderPresence(_instanceID, XFAFieldName,
                    NewPresence);
        }

        public int SetXFAFormFieldBorderWidth(string XFAFieldName, double BorderWidth)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetXFAFormFieldBorderWidth(_instanceID, XFAFieldName,
                    BorderWidth);
        }

        public int SetXFAFormFieldValue(string XFAFieldName, string NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetXFAFormFieldValue(_instanceID, XFAFieldName, NewValue);
        }

        public int SetXFAFromString(byte[] Source, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(Source, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, Source.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), Source.Length);
                int result = _dll.DebenuPDFLibrarySetXFAFromString(_instanceID, bufferID, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int SetupCustomPrinter(string CustomPrinterName, int Setting, int NewValue)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySetupCustomPrinter(_instanceID, CustomPrinterName, Setting,
                    NewValue);
        }

        public int SignFile(string InputFileName, string OpenPassword, string SignatureFieldName,
            string OutputFileName, string PFXFileName, string PFXPassword, string Reason,
            string Location, string ContactInfo)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySignFile(_instanceID, InputFileName, OpenPassword,
                    SignatureFieldName, OutputFileName, PFXFileName, PFXPassword, Reason, Location,
                    ContactInfo);
        }

        public int SplitPageText(int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibrarySplitPageText(_instanceID, Options);
        }

        public int StartPath(double StartX, double StartY)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryStartPath(_instanceID, StartX, StartY);
        }

        public int StoreCustomDataFromFile(string Key, string FileName, int Location, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryStoreCustomDataFromFile(_instanceID, Key, FileName,
                    Location, Options);
        }

        public int StoreCustomDataFromString(string Key, byte[] NewValue, int Location, int Options)
        {
            if (_dll == null) return 0;
            else
            {
                GCHandle gch = GCHandle.Alloc(NewValue, GCHandleType.Pinned);
                IntPtr bufferID = _dll.DebenuPDFLibraryCreateBuffer(_instanceID, NewValue.Length);
                _dll.DebenuPDFLibraryAddToBuffer(_instanceID, bufferID, gch.AddrOfPinnedObject(), NewValue.Length);
                int result = _dll.DebenuPDFLibraryStoreCustomDataFromString(_instanceID, Key, bufferID,
                    Location, Options);
                _dll.DebenuPDFLibraryReleaseBuffer(_instanceID, bufferID);
                gch.Free();
                return result;
            }
        }

        public int StringResultLength()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryStringResultLength(_instanceID);
        }

        public int TestTempPath()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryTestTempPath(_instanceID);
        }

        public int TransformFile(string InputFileName, string Password, string OutputFileName,
            int TransformType, int Options)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryTransformFile(_instanceID, InputFileName, Password,
                    OutputFileName, TransformType, Options);
        }

        public int UnlockKey(string LicenseKey)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUnlockKey(_instanceID, LicenseKey);
        }

        public int Unlocked()
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUnlocked(_instanceID);
        }

        public int UpdateAndFlattenFormField(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUpdateAndFlattenFormField(_instanceID, Index);
        }

        public int UpdateAppearanceStream(int Index)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUpdateAppearanceStream(_instanceID, Index);
        }

        public int UpdateTrueTypeSubsettedFont(string SubsetChars)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUpdateTrueTypeSubsettedFont(_instanceID, SubsetChars);
        }

        public int UseKerning(int Kern)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUseKerning(_instanceID, Kern);
        }

        public int UseUnsafeContentStreams(int SafetyLevel)
        {
            if (_dll == null) return 0;
            else
                return _dll.DebenuPDFLibraryUseUnsafeContentStreams(_instanceID, SafetyLevel);
        }
    }
}
