using System.Text;
using consumer.DTOs;
using Newtonsoft.Json;

namespace consumer.Helpers;

public class ConsumerHelper
{
  public static PaymentMessageServiceDTO? GetPaymentMessage(byte[] body)
  {
    try
    {
      PaymentMessageServiceDTO? message = JsonConvert.DeserializeObject<PaymentMessageServiceDTO>(Encoding.UTF8.GetString(body));
      return message;
    } 
    catch 
    {
      return null;
    }
  }
}