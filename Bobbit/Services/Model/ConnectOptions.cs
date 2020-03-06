namespace Bobbit.Services.Model
{
    public class ConnectOptions
    {
        public string Host { get; set; }
        public int HttpApiPort { get; set; }
        public int AmqpPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
