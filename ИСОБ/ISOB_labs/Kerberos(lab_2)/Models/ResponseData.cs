namespace Kerberos_lab_2_.Models
{
    public class ResponseData<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess {  get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
