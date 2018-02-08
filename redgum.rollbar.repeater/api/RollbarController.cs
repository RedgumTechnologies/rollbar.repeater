using Newtonsoft.Json;
using redgum.rollbar.repeater.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace redgum.rollbar.repeater.api
{
    [Route("api/1")]
    public class RollbarController : ApiController
    {
        public Rollbar.RollbarConfig Get()
        {
            return RollbarConfigProvider.GetRollbarConfig();
        }

        [HttpPost()]
        public async Task<Rollbar.RollbarResponse> Post([FromBody] dynamic payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            //unfortunately Newtonsoft makes dynamics into JTokens rather than real dynamics
            dynamic dynamicPayload = payload.ToObject<ExpandoObject>();

            //Post to the real Rollbar, or to whatever Rollbar endpoint is configured in the rollbar.repeater's web.config (only configured if we need to override the default)
            return await PostToRollbar(dynamicPayload);
        }

        [HttpPost()]
        [Route("api/1/{applicationIdentifier}")] //seem to need the route defined
        public async Task<Rollbar.RollbarResponse> Post([FromUri] string applicationIdentifier, [FromBody] dynamic payload)
        {
            //I don't think we'll ever get routed here if applicationIdentifier is null or empty, but lets check anyway
            if (string.IsNullOrEmpty(applicationIdentifier)) throw new ArgumentNullException(nameof(applicationIdentifier));
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            //look up the access token for that applicationIdentifier
            var accessToken = RollbarRepeaterAccessTokenSettingsProvider.GetSetting(applicationIdentifier);
            if (string.IsNullOrEmpty(accessToken)) throw new KeyNotFoundException($"Application: {applicationIdentifier} not found in configuration.");

            //unfortunately Newtonsoft makes dynamics into JTokens rather than real dynamics
            dynamic dynamicPayload = payload.ToObject<ExpandoObject>();
            //set the access_token to the one we've looked up out of our config
            dynamicPayload.access_token = accessToken;

            //Post to the real Rollbar, or to whatever Rollbar endpoint is configured in the rollbar.repeater's web.config (only configured if we need to override the default)
            return await PostToRollbar(dynamicPayload);
        }

        private async Task<Rollbar.RollbarResponse> PostToRollbar(dynamic payload)
        {
            var rollbarConfig = RollbarConfigProvider.GetRollbarConfig();
            var rollbarClient = new Services.RollbarClient(rollbarConfig);

            //Post to the real Rollbar, or to whatever Rollbar endpoint is configured in the rollbar.repeater's web.config (only configured if we need to override the default)
            return await rollbarClient.PostAsJsonAsync(payload, scrubFields: null);
        }
    }
}
