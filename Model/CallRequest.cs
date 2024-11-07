namespace TwilioVoipBackend.Model
{
    public class CallRequest
    {
        public string ToPhoneNumber { get; set; }
        public string VoiceUrl { get; set; } // URL to Twilio XML or your custom TwiML Bin
    }
}
