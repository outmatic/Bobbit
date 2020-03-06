namespace Bobbit.Services.Model
{
    public class Message
    {
        public string Body { get; set; }
        public string Type { get; set; }
        public ulong DeliveryTag { get; set; }
    }
}
