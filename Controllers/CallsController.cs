using Microsoft.AspNetCore.Mvc;
using TwilioVoipBackend.Model;
using TwilioVoipBackend.Services;
using Twilio.Jwt.AccessToken;
using Twilio.TwiML;
using Twilio.AspNet.Core;
using Microsoft.AspNetCore.SignalR;
using TwilioVoipBackend.Hubs;
using System.Collections.Concurrent;


namespace TwilioVoipBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallsController : ControllerBase
    {
        private readonly TwilioService _twilioService;
        private readonly IHubContext<CallHub> _hubContext;

        private static ConcurrentDictionary<string, bool> CallStatus = new ConcurrentDictionary<string, bool>();

        public CallsController(TwilioService twilioService, IHubContext<CallHub> hubContext)
        {
            _twilioService = twilioService;
            _hubContext = hubContext;
        }

        [HttpPost("get-token")]
        public IActionResult GetToken()
        {
            try
            {
                // Twilio Account SID and Auth Token
                var accountSid = "AC2fb2b05efa485fb5ce8a92b6ff686600";
                var authToken = "76f16dcc9424e833aeec39e4c37fd3b4";
                var twilioApiKey = "SK1f60198dfe926de26e1d5288a8956514";
                var twilioApiSecret = "0tMMKLHpYwnGdmBdDmKJBMl0YPFYPjsS";
                var identity = "mURALI"; // Change to a unique identity for the user

                // Create an Access Token
                var grant = new VoiceGrant
                {
                    IncomingAllow = true,
                    OutgoingApplicationSid = "AC2fb2b05efa485fb5ce8a92b6ff686600" // Your TwiML App SID
                };

                var token = new Token(
                    accountSid,
                    twilioApiKey,
                    twilioApiSecret,
                    identity,
                    grants: new HashSet<IGrant> { grant }
                );
                return Ok(new { token = token.ToJwt() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpPost("make-call")]
        public IActionResult MakeCall([FromBody] CallRequest request)
        {
            try
            {
                var call = _twilioService.MakeCall(request.ToPhoneNumber, request.VoiceUrl);
                return Ok(new { CallSid = call.Sid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("handle-incoming")]
        public IActionResult HandleIncoming()
        {
            var response = new VoiceResponse();
            response.Say("Thank you for calling! Please wait while we connect you.");

            // Simulate a call ID or use a unique identifier from the incoming request
            string callId = "sampleCallId"; // This should be replaced with a unique call identifier

            // Notify the front-end about the incoming call
            _hubContext.Clients.All.SendAsync("IncomingCall", new { From = "CallerNumber" });

            // Wait and check if the call has been accepted
            if (CallStatus.TryGetValue(callId, out bool isAccepted) && isAccepted)
            {
                response.Say("Thank you for calling! Have a great day.");
            }
            else
            {
                response.Say("Call declined!");
            }

            return new TwiMLResult(response);
        }

        [HttpPost("accept-call")]
        public IActionResult AcceptCall([FromBody] string callId)
        {
            // Mark the call as accepted in the in-memory store
            CallStatus[callId] = true;

            var response = new VoiceResponse();
            response.Say("Call accepted.");
            return Ok(new { Message = "Call accepted" });
        }
    }
}
