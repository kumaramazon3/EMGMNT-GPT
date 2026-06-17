using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using System.Net;
using static System.Net.WebRequestMethods;

namespace Maruwa_Emgmt.DAL
{
    public class da_FtpService : IFtpService
    {
        private readonly ILogger<da_FtpService> _logger;
        private readonly IConfiguration _configuration;
        public da_FtpService(ILogger<da_FtpService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  // Ensure logger is not null
            _configuration = configuration;
        }

        private readonly string _ftpServer;
        private readonly int _ftpPort;
        private readonly string _ftpUsername;
        private readonly string _ftpPassword;
        private readonly string _ftpBasePath;

        public da_FtpService(IConfiguration configuration)
        {
            _ftpServer = configuration["FTPSettings:Server"]!;
            _ftpPort = int.Parse(configuration["FTPSettings:Port"] ?? "21");
            _ftpUsername = configuration["FTPSettings:Username"]!;
            _ftpPassword = configuration["FTPSettings:Password"]!;
            _ftpBasePath = configuration["FTPSettings:BasePath"]!;
        }

        public async Task<string?> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            string fileName = $"{DateTime.Now:ddMMyyyyHHmmssfff}{Path.GetExtension(file.FileName)}";
            string ftpUrl = $"ftp://{_ftpServer}:{_ftpPort}{_ftpBasePath}/{fileName}";// ftp://server:port/basepath/filename
            //string ftpUrl = $"ftp://{_ftpServer}{_ftpBasePath}/{fileName}";// ftp://server/basepath/filename  (NO PORT)

            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;
            try
            {
                using var requestStream = await request.GetRequestStreamAsync();
                await file.CopyToAsync(requestStream);

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                return ftpUrl; // Save this in DB
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching house Code: {ex.Message}");
                return null;
            }
        }
    }
    public class da_ProjectLocationService: IProjectLocationService
    {
        private readonly ILogger<da_FtpService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public da_ProjectLocationService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(IFormFile file,string empcode, string filePrefix)
        {
            try
            {
                string wwwRootPath = _env.WebRootPath;// wwwroot path
                string folderPath = Path.Combine(wwwRootPath, "EmployeeDocuments", empcode);// EmployeeDocuments/EMP001

                if (!Directory.Exists(folderPath))// Create directory if not exists
                    Directory.CreateDirectory(folderPath);
                
                string fileExt = Path.GetExtension(file.FileName);
                string fileName = $"{filePrefix}_{DateTime.Now:yyyyMMddHHmmss}{fileExt}";// Unique file name
                string fullPath = Path.Combine(folderPath, fileName);
                
                using (var stream = new FileStream(fullPath, FileMode.Create))// Save file
                {
                    await file.CopyToAsync(stream);
                }

                string fileUrl = $"/EmployeeDocuments/{empcode}/{fileName}"; // Public URL (stored in DB)
                return fileUrl;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
