using Minio.DataModel.Args;
using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Minio.Exceptions;

namespace WebScrapperFunctionApp
{
    internal class BlobSerivce
    {
        public static async Task<string> UploadFile(string urlOrPath,string filename,string bucketName)
        {
            try
            {
                IMinioClient minio = new MinioClient()
                                        .WithEndpoint("s3.houselabs.co.za")
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

                string firstFileUrl = $"https://s3.houselabs.co.za/{bucketName}/{filename}";
                return firstFileUrl;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
