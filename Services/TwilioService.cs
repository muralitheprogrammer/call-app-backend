using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TwilioVoipBackend.Services
{
    public class TwilioService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _twilioNumber;

        public TwilioService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _twilioNumber = configuration["Twilio:TwilioNumber"];
            TwilioClient.Init(_accountSid, _authToken);
        }

        //public string GenerateToken()
        //{
        //    // Create an access token for Twilio
        //    var token = new Token(_accountSid, _authToken, "YOUR_TWILIO_CHAT_SERVICE_SID", "YOUR_TWILIO_GRANT");
        //    return token.ToJwt();
        //}

        public CallResource MakeCall(string toPhoneNumber, string voiceUrl)
        {
            var to = new PhoneNumber(toPhoneNumber);
            var from = new PhoneNumber(_twilioNumber);
            return CallResource.Create(
                to,
                from,
                url: new Uri(voiceUrl)
            );
        }

        // Implement more methods for handling calls, if necessary
    }
}
