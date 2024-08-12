using Minio.DataModel.Args;
using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Minio.Exceptions;
using System.IO.Compression;
using WebScrapperFunctionApp.Dto;

namespace WebScrapperFunctionApp
{
    internal class BlobSerivce
    {
        public static async Task<List<Dto.File>> UploadZipContent(string filename,string zipFileUrl, string bucketName)
        {
            var uploadedFiles = new List<Dto.File>();

            try
            {
                IMinioClient minio = new MinioClient()
                                        .WithEndpoint("s3storage.threedprintingservices.com")
                                        .WithCredentials("5LW1eSOLVXSWCabAyKEj", "SdRrRqCXObDnbOtArqKofkiTdPzmum4zF5mYvzna")
                                        .WithSSL()
                                        .Build();

                // Download the ZIP file into a memory stream
                var client = new HttpClient();
                using var zipStream = await client.GetStreamAsync(zipFileUrl);

                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue; // Skip directories

                    // Generate unique filename with GUID
                    var uniqueFileName = $"{filename}_{Guid.NewGuid()}_{Guid.NewGuid()}{Path.GetExtension(entry.Name)}";

                    using var entryStream = entry.Open();
                    var memoryStream = new MemoryStream();
                    await entryStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(uniqueFileName)
                        .WithStreamData(memoryStream)
                        .WithObjectSize(memoryStream.Length)
                        .WithContentType("application/octet-stream");

                    await minio.PutObjectAsync(putObjectArgs);

                    string fileUrl = $"https://s3storage.threedprintingservices.com/{bucketName}/{uniqueFileName}";

                    // Create and add File object to the list
                    uploadedFiles.Add(new Dto.File
                    {
                        name = uniqueFileName,
                        url = fileUrl,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return uploadedFiles;
        }
        public static async Task<string> UploadFile(string urlOrPath,string filename,string bucketName)
        {
            try
            {
                IMinioClient minio = new MinioClient()
                                        .WithEndpoint("s3storage.threedprintingservices.com")
                                        .WithCredentials("5LW1eSOLVXSWCabAyKEj", "SdRrRqCXObDnbOtArqKofkiTdPzmum4zF5mYvzna")
                                        .WithSSL()
                                        .Build();
            
                var memoryStream = new MemoryStream();
                using (var webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(urlOrPath);
                    memoryStream.Write(data, 0, data.Length);
                    memoryStream.Position = 0;
                }
                using (var stream = memoryStream)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(filename)
                        .WithStreamData(stream)
                        .WithObjectSize(stream.Length)
                        .WithContentType("application/octet-stream");

                    await minio.PutObjectAsync(putObjectArgs);
                }

                string firstFileUrl = $"https://s3storage.threedprintingservices.com/{bucketName}/{filename}";
                return firstFileUrl;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
