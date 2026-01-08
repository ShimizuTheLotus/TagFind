using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace TagFind.Classes.Extensions
{
    public static class FileExtensions
    {
        public static bool IsValidFilePath(this string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath);
        }

        public static bool IsImageFile(this string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg", ".webp" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return imageExtensions.Contains(fileExtension);
        }

        public static bool IsVideoFile(this string filePath)
        {
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return videoExtensions.Contains(fileExtension);
        }

        public static bool IsAudioFile(this string filePath)
        {
            string[] audioExtensions = { ".mp3", ".wav", ".aac", ".flac", ".ogg", ".wma", ".m4a" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return audioExtensions.Contains(fileExtension);
        }

        public static bool IsDocumentFile(this string filePath)
        {
            string[] documentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".rtf", ".csv", ".md", ".log", ".xml", ".json" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return documentExtensions.Contains(fileExtension);
        }

        public static bool IsArchiveFile(this string filePath)
        {
            string[] archiveExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            return archiveExtensions.Contains(fileExtension);
        }

        public static async void OpenReferencedFileByDefaultProgram(this string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var storageFile = await StorageFile.GetFileFromPathAsync(filePath);
            var options = new LauncherOptions
            {
                DisplayApplicationPicker = false
            };

            await Launcher.LaunchFileAsync(storageFile, options);
        }

        /*
         PSEUDOCODE / PLAN (detailed):
         
         - Provide an async, streaming text extractor that yields text chunks/lines so very large files don't need to be loaded fully into memory.
         - Expose a streaming API: IAsyncEnumerable<string> GetDocumentFileTextAsync(filePath, Encoding?, CancellationToken)
         - Keep the existing synchronous method name GetDocumentFileText but have it return the async enumerable as object (non-breaking minimal change).
         - Behavior by extension:
           - For plain text-like files (.txt, .csv, .log, .md, .json, .xml): open a FileStream with FileOptions.SequentialScan and FileOptions.Asynchronous and StreamReader; yield lines as they are read (await ReadLineAsync).
           - For .docx: open as zip, find "word/document.xml", create an XmlReader with Async = true and stream through; accumulate text nodes (<w:t>) into a paragraph buffer; when encountering paragraph end (<w:p>) or break (<w:br/>) yield the paragraph as one item; respect cancellation token.
           - For .rtf: treat as text and stream lines (RTF is textual). This does not convert RTF to plain text; it returns raw RTF lines. Converting to plain text reliably requires external libs or UI components.
           - For .pdf, .doc (binary), .xls/.xlsx/.pptx: these require third-party libraries for robust extraction; throw NotSupportedException with a helpful message for unsupported formats.
         - Ensure methods validate file existence and throw meaningful exceptions.
         - Mark method as async-stream so callers can iterate without loading entire content into memory.
         - Use minimal dependencies (System.IO.Compression and System.Xml) available in .NET 8.
        */

        // Keep original method name but return the async enumerable (as object) so callers who accept an object can still get the stream.
        public static object GetDocumentFileText(this string filePath)
        {
            return GetDocumentFileTextAsync(filePath);
        }

        // Async streaming extractor - callers should iterate asynchronously to avoid loading entire file into memory.
        public static async IAsyncEnumerable<string> GetDocumentFileTextAsync(this string filePath, Encoding? encoding = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                yield break;
            }

            if (!File.Exists(filePath))
            {
                yield break;
            }   

            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            // Text-like files: stream line by line
            string[] textLike = { ".txt", ".csv", ".log", ".md", ".json", ".xml", ".rtf"};
            if (textLike.Contains(ext))
            {
                encoding ??= Encoding.UTF8;
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                using var reader = new StreamReader(fs, encoding);
                while (!reader.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line is null) break;
                    yield return line;
                }
                yield break;
            }

            // DOCX: open as zip and stream document.xml
            if (ext == ".docx")
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan);
                using var zip = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false);
                var entry = zip.GetEntry("word/document.xml") ?? zip.Entries.FirstOrDefault(e => e.FullName.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                    yield break;

                using var entryStream = entry.Open();
                var settings = new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Ignore, IgnoreComments = true, IgnoreProcessingInstructions = true };
                using var xr = XmlReader.Create(entryStream, settings);

                var paragraphBuilder = new StringBuilder();
                while (await xr.ReadAsync().ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        var name = xr.LocalName;

                        // Text node in WordprocessingML
                        if (string.Equals(name, "t", StringComparison.OrdinalIgnoreCase))
                        {
                            // Read element content (text) asynchronously
                            var txt = await xr.ReadElementContentAsStringAsync().ConfigureAwait(false);
                            paragraphBuilder.Append(txt);
                        }
                        else if (string.Equals(name, "tab", StringComparison.OrdinalIgnoreCase))
                        {
                            paragraphBuilder.Append('\t');
                        }
                        else if (string.Equals(name, "br", StringComparison.OrdinalIgnoreCase))
                        {
                            // line break inside paragraph
                            paragraphBuilder.AppendLine();
                        }
                        // other elements ignored; paragraph boundary handled below on end element
                    }
                    else if (xr.NodeType == XmlNodeType.EndElement && string.Equals(xr.LocalName, "p", StringComparison.OrdinalIgnoreCase))
                    {
                        // End of paragraph, yield accumulated paragraph text (if any)
                        var para = paragraphBuilder.ToString();
                        paragraphBuilder.Clear();
                        yield return para;
                    }
                }

                // If leftover text exists, yield it
                var remaining = paragraphBuilder.ToString();
                if (!string.IsNullOrEmpty(remaining))
                    yield return remaining;

                yield break;
            }

            // Unsupported/complex formats - require external libraries for reliable extraction
            //switch (ext)
            //{
            //    case ".pdf":
            //        //NotSupportedException("PDF text extraction is not supported by this implementation. Use a PDF library like 'PdfPig' or 'iText7' to extract text.");
            //    case ".doc":
            //        //NotSupportedException("Legacy .doc (binary Word) extraction is not supported. Use external libraries (e.g., Microsoft.Office.Interop.Word or third-party) to extract text.");
            //    case ".xls":
            //    case ".xlsx":
            //    case ".ppt":
            //    case ".pptx":
            //        //NotSupportedException("Office binary/packaged formats (xls/xlsx/ppt/pptx) require external libraries to extract text reliably.");
            //    default:
            //        //NotSupportedException($"File extension '{ext}' is not supported for text extraction.");
            //}
        }

        public enum AccessType
        {
            Read,
            Write,
            ReadWrite,
            Delete,
            Execute
        }

        public static bool HasFileAccess(this string filePath, AccessType accessType)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return HasDirectoryAccess(Path.GetDirectoryName(filePath), accessType);
                }

                // Get safety info
                var fileInfo = new FileInfo(filePath);
                var fileSecurity = fileInfo.GetAccessControl();

                // Get current user
                var currentUser = WindowsIdentity.GetCurrent();
                var currentUserSid = currentUser.User;

                // Check access rules
                var accessRules = fileSecurity.GetAccessRules(
                    includeExplicit: true,
                    includeInherited: true,
                    targetType: typeof(SecurityIdentifier));

                FileSystemRights requiredRights = GetRequiredRights(accessType);
                bool hasAllow = false;
                bool hasDeny = false;

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    var sid = rule.IdentityReference as SecurityIdentifier;
                    if (sid == null) continue;

                    if (IsUserInSD(currentUser, sid))
                    {
                        if ((rule.FileSystemRights & requiredRights) == requiredRights)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                            {
                                hasAllow = true;
                            }
                            else if (rule.AccessControlType == AccessControlType.Deny)
                            {
                                hasDeny = true;
                            }
                        }
                    }
                }

                if (hasDeny) return false;
                return hasAllow;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool HasDirectoryAccess(this string? directoryPath, AccessType accessType)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return false;

                var dirInfo = new DirectoryInfo(directoryPath);
                var dirSecurity = dirInfo.GetAccessControl();
                var currentUser = WindowsIdentity.GetCurrent();

                FileSystemRights requiredRights = GetRequiredRights(accessType);

                var accessRules = dirSecurity.GetAccessRules(
                    includeExplicit: true,
                    includeInherited: true,
                    targetType: typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    var sid = rule.IdentityReference as SecurityIdentifier;
                    if (sid == null) continue;

                    if (IsUserInSD(currentUser, sid))
                    {
                        if ((rule.FileSystemRights & requiredRights) == requiredRights)
                        {
                            return rule.AccessControlType == AccessControlType.Allow;
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static FileSystemRights GetRequiredRights(AccessType accessType)
        {
            return accessType switch
            {
                AccessType.Read => FileSystemRights.ReadData | FileSystemRights.ReadAttributes,
                AccessType.Write => FileSystemRights.WriteData | FileSystemRights.WriteAttributes,
                AccessType.ReadWrite => FileSystemRights.ReadData | FileSystemRights.WriteData,
                AccessType.Delete => FileSystemRights.Delete,
                AccessType.Execute => FileSystemRights.ExecuteFile,
                _ => FileSystemRights.ReadData
            };
        }

        private static bool IsUserInSD(WindowsIdentity user, SecurityIdentifier sid)
        {
            if (user == null) return false;
            // Check user SID
            if (sid == user.User)
                return true;

            // User in group
            foreach (var group in user.Groups ?? [])
            {
                if (sid == group)
                    return true;
            }

            // Is built-in group
            if (sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) &&
                new WindowsPrincipal(user).IsInRole(WindowsBuiltInRole.Administrator))
                return true;

            return false;
        }



        public static async Task<BitmapImage> GetThumbnail(this string filePath, int width, int height)
        {
            try
            {
                if (filePath.IsDocumentFile())
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/DataItemThumbnail/TextDataItem.png", UriKind.Absolute));
                }
                if (filePath.IsImageFile())
                {
                    return await GetImageThumbnail(filePath, width, height);
                }
                // Is other object
                return new BitmapImage(new Uri("ms-appx:///Assets/DataItemThumbnail/ObjectDataItem.png", UriKind.Absolute));
            }
            catch
            {
                return await EmptyBitmapImageAsync();
            }
        }

        public static async Task<BitmapImage> GetImageThumbnail(this string filePath, int width, int height)
        {
            try
            {
                if (!filePath.IsValidFilePath() || !File.Exists(filePath))
                {
                    return new BitmapImage(new Uri("ms-appx:///Assets/DataItemThumbnail/ImageDataItem.png", UriKind.Absolute));
                }
                // For WPF version core.
                //var assembly = Assembly.GetExecutingAssembly();
                //var resourceNames = assembly.GetManifestResourceNames();

                //BitmapImage myBitmapImage = new BitmapImage();

                //myBitmapImage.UriSource = new Uri(filePath);

                //myBitmapImage.DecodePixelWidth = 200;
                //return myBitmapImage;

                using (var fs = File.OpenRead(filePath))
                {
                    IRandomAccessStream ras = fs.AsRandomAccessStream();
                    var bitmapImage = new BitmapImage();
                    if (width > 0) bitmapImage.DecodePixelWidth = width;
                    if (height > 0) bitmapImage.DecodePixelHeight = height;
                    await bitmapImage.SetSourceAsync(ras);
                    return bitmapImage;
                }
            }
            catch (Exception)
            {
                return await EmptyBitmapImageAsync();
            }
        }

        private static async Task<BitmapImage> EmptyBitmapImageAsync()
        {
            const int width = 100;
            const int height = 100;
            var pixelData = new byte[width * height * 4];

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = 0;
                pixelData[i + 1] = 0;
                pixelData[i + 2] = 0;
                pixelData[i + 3] = 255;
            }

            var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
                pixelData.AsBuffer(),
                BitmapPixelFormat.Bgra8,
                width,
                height
            );

            var bitmapImage = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();

                stream.Seek(0);
                await bitmapImage.SetSourceAsync(stream);
            }

            return bitmapImage;
        }

    }
}
