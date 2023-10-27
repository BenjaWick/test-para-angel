using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace YouTubeUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string apiKey = "AIzaSyAzW0XoMhKqXtTtNtzP0fgLdZUIU_OI5zA";
            
            string credentialsPath = "client_secrets.json";

            UserCredential credential;

            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    System.Threading.CancellationToken.None,
                    new FileDataStore("YouTubeUploader")).Result;
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                HttpClientInitializer = credential,
                ApplicationName = "SVideosApi"
            });

            
            string[] videoFilePaths = new string[]
            {
                "/home/benja/Documentos/Resultantes/benja.mp4",
                "/home/benja/Documentos/Resultantes/benj.mp4"
                // Agrega más rutas de archivos de video según sea necesario
            };

            foreach (var videoFilePath in videoFilePaths)
            {
                var video = new Google.Apis.YouTube.v3.Data.Video();
                video.Snippet = new Google.Apis.YouTube.v3.Data.VideoSnippet();
                video.Snippet.Title = "Título del video";
                video.Snippet.Description = "Descripción del video";
                video.Snippet.Tags = new string[] { "etiqueta1", "etiqueta2" };
                video.Snippet.CategoryId = "22"; // ID de la categoría de YouTube (opcional)
                video.Status = new Google.Apis.YouTube.v3.Data.VideoStatus();
                video.Status.PrivacyStatus = "public"; // Privacidad del video ("public", "private", "unlisted")

                using (var fileStream = new FileStream(videoFilePath, FileMode.Open))
                {
                    var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                    videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
                    videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

                    videosInsertRequest.Upload();
                }

                Console.WriteLine($"Video '{Path.GetFileName(videoFilePath)}' subido a YouTube.");
            }

            Console.WriteLine("Presiona cualquier tecla para salir.");
            Console.ReadKey();
        }

        private static void VideosInsertRequest_ProgressChanged(IUploadProgress progress)
        {
            Console.WriteLine($"Progreso de subida a YouTube: {progress.BytesSent} bytes enviados.");
        }

        private static void VideosInsertRequest_ResponseReceived(Google.Apis.YouTube.v3.Data.Video video)
        {
            Console.WriteLine($"El video '{video.Snippet.Title}' se ha subido correctamente a YouTube. ID: {video.Id}");
        }
    }
}