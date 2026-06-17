namespace Maruwa_Emgmt.Models
{
    public class Operations<T>
    {
        public bool success { get; set; } 
        public T Data { get; set; } 
        public string ErrorMessage { get; set; } 
        public string Message { get; set; }
    }
}
