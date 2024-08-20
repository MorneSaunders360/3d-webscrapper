
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Google.Protobuf.WellKnownTypes;
using Minio.DataModel.Args;
using Minio.DataModel;
using System;
using System.IO;
using System.Net;
using System.Security.Policy;
using Minio;
using Newtonsoft.Json.Linq;


namespace WebScrapperFunctionApp.Services
{
    public class DimensionsAndVolumeResponse
    {
        public string Url { get; set; }
        public double Volume { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Length { get; set; }
    }
    public class STLProcessorService
    {
        public static async Task<List<DimensionsAndVolumeResponse>> ProcessSTLFiles(List<string> stlUrls)
        {
            var tasks = new List<Task<DimensionsAndVolumeResponse>>();
            foreach (var url in stlUrls)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (var memoryStream = LoadSTLIntoMemory(url))
                    {
                        if (memoryStream == null)
                        {
                            SentrySdk.CaptureMessage($"Failed to load STL file from {url}", SentryLevel.Warning);
                            return null;
                        }

                        var result = await ProcessSTLFile(memoryStream, url);
                        return new DimensionsAndVolumeResponse
                        {
                            Url = url,
                            Volume = result.totalVolume,
                            Width = result.width,
                            Height = result.height,
                            Length = result.length
                        };
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);
            return results.Where(result => result != null).ToList();
        }


        private static MemoryStream LoadSTLIntoMemory(string urlOrPath)
        {
            try
            {
                var memoryStream = new MemoryStream();
                using (var webClient = new WebClient())
                {

                    byte[] data = webClient.DownloadData(urlOrPath);
                    memoryStream.Write(data, 0, data.Length);
                    memoryStream.Position = 0;
                }
                return memoryStream;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return null;
            }
        }




        private async static Task<(double totalVolume, double width, double height, double length)> ProcessSTLFile(MemoryStream stream, string url)
        {
            var scene = new Scene();

            try
            {
                scene.Open(stream);
            }
            catch (Exception ex)
            {
                if (url.ToLower().Contains(".stl"))
                {
                    try
                    {
                        stream.Position = 0; // Reset stream position before retrying
                        scene.Open(stream, FileFormat.STLBinary);
                    }
                    catch
                    {
                        return (100, 0, 0, 0);
                    }
                }
                else if (url.ToLower().Contains(".3mf"))
                {
                    try
                    {
                        stream.Position = 0; // Reset stream position before retrying
                        scene.Open(stream, FileFormat.Microsoft3MF);
                    }
                    catch
                    {
                        return (100, 0, 0, 0);
                    }
                }
                else
                {
                    return (100, 0, 0, 0);
                }
            }

            double totalVolume = 0;
            double width = 0, height = 0, length = 0;

            foreach (var node in scene.RootNode.ChildNodes)
            {
                if (node.Entity is Mesh mesh)
                {
                    mesh = TriangulateMesh(mesh);
                    CalculateMeshDimensions(mesh, out double meshWidth, out double meshHeight, out double meshLength);

                    width = Math.Max(width, meshWidth);
                    height = Math.Max(height, meshHeight);
                    length = Math.Max(length, meshLength);
                }
            }
            width = Math.Round(width, 0);
            height = Math.Round(height, 0);
            length = Math.Round(length, 0);

            //var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Post, "https://3d-print-stl-estimation.p.rapidapi.com/slice_and_extract?rotate_y=0&rotate_x=0&config_file=config.ini");
            //request.Headers.Add("accept", "application/json");
            //request.Headers.Add("x-rapidapi-key", "8a7f4a7b88msh51365cffca73f48p11e251jsncd292873f5d5");

            //// Reset the stream position before sending it to the API
            //stream.Position = 0;

            //var content = new MultipartFormDataContent();
            //content.Add(new StreamContent(stream), "stl_file", "test.stl"); // Ensure "stl_file" matches the expected parameter name
            //request.Content = content;

            //var response = await client.SendAsync(request);
            //if (response.IsSuccessStatusCode)
            //{
            //    var jsonDownloadPackLink = JObject.Parse(await response.Content.ReadAsStringAsync());
            //    var total_filament_used_g = jsonDownloadPackLink["total_filament_used_g"].ToString();
            //    totalVolume = Convert.ToDouble(total_filament_used_g);
            //}
            //else
            //{
            //    var errorResponse = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine($"Error: {errorResponse}");
            //}

            // If totalVolume is still 0, calculate a rough estimate
            totalVolume = Math.Ceiling(((length * width * height) / 1000) * 0.50);

            // Ensure a minimum volume if no valid volume was calculated
            if (totalVolume == 0)
            {
                totalVolume = 100;
            }

            return (totalVolume, width, height, length);
        }


        private static Mesh TriangulateMesh(Mesh mesh)
        {
            return PolygonModifier.Triangulate(mesh);
        }


        private static void CalculateMeshDimensions(Mesh mesh, out double width, out double height, out double length)
        {
            var controlPoints = mesh.ControlPoints;

            // Initialize min and max points
            Vector3 minPoint = new Vector3(double.MaxValue, double.MaxValue, double.MaxValue);
            Vector3 maxPoint = new Vector3(double.MinValue, double.MinValue, double.MinValue);

            // Find the bounding box of the mesh
            foreach (var point in controlPoints)
            {
                if (point.X < minPoint.X) minPoint.X = point.X;
                if (point.Y < minPoint.Y) minPoint.Y = point.Y;
                if (point.Z < minPoint.Z) minPoint.Z = point.Z;
                if (point.X > maxPoint.X) maxPoint.X = point.X;
                if (point.Y > maxPoint.Y) maxPoint.Y = point.Y;
                if (point.Z > maxPoint.Z) maxPoint.Z = point.Z;
            }

            // Calculate dimensions
            width = maxPoint.X - minPoint.X;
            height = maxPoint.Y - minPoint.Y;
            length = maxPoint.Z - minPoint.Z;
        }

    }


}
