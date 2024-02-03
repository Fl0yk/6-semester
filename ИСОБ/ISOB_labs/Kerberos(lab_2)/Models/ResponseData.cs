using System.Text;
using System.Text.Json;

namespace Kerberos_lab_2_.Models
{
    public class ResponseData<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess {  get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
        }
    }
}
