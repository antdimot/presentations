using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Devday
{
    public class Function
    {
        
        public async Task<SkillResponse> FunctionHandler( SkillRequest input, ILambdaContext context )
        {
            try
            {
                if( input.Request is LaunchRequest )
                {
                    context.Logger.LogLine( "LaunchRequest detected." );

                    var speech = new SsmlOutputSpeech();
                    speech.Ssml = $"<speak>Benvenuto al Dev Day.</speak>";

                    var repromptMessage = new PlainTextOutputSpeech();
                    repromptMessage.Text = "Cosa vuoi sapere?";

                    var repromptBody = new Reprompt();
                    repromptBody.OutputSpeech = repromptMessage;

                    var finalResponse = ResponseBuilder.Ask( speech, repromptBody );

                    return finalResponse;              
                } else {
                    var intentRequest = input.Request as IntentRequest;

                    context.Logger.LogLine( $"{intentRequest.Intent.Name} detected." );

                    var sb = new StringBuilder();

                    switch ( intentRequest.Intent.Name )
                    {   
                        case "nexteventsIntent":
                            var devdayEvents = await new DataManager().GetAllAsync<DevdayEvent>( new List<ScanCondition> {
                                new ScanCondition( "Date", ScanOperator.GreaterThanOrEqual, DateTime.Now )
                            } );

                            if( devdayEvents.Count == 0 ) sb.Append( "Al momento non sono previsti nuovi eventi." );
                            else {
                                sb.Append( "I prossimi eventi sono: " );

                                foreach (var item in devdayEvents )
                                {
                                    var itaDate = item.Date.ToString( "dddd, d MMMM yyyy", new CultureInfo("it-IT") );
                                    var itaHour = item.Date.ToString( "H:mm", new CultureInfo("it-IT") );
                                    
                                    sb.Append( $"{item.Title} per {itaDate} alle {itaHour}, " );
                                }
                            }
                            break;
                        default:
                            sb.Append( "Mi dispiace, non ho capito." );
                            break;
                    }

                    var speech = new SsmlOutputSpeech { 
                        Ssml = $"<speak>{sb.ToString()}</speak>"
                    };
                
                    return ResponseBuilder.Tell( speech );
                }
            }
            catch( System.Exception ex )
            {
                context.Logger.LogLine( ex.Message );

                return ResponseBuilder.Tell( new SsmlOutputSpeech {
                    Ssml = @"<speak>Si è verificato un problema, riprova più tardi.</speak>"
                } );
            }           
        }
    }
}
