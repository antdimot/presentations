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
                var responseBuilder = new StringBuilder();

                var intentRequest = input.Request as IntentRequest;

                if( !(intentRequest is null) )
                {
                    context.Logger.LogLine( $"{intentRequest.Intent.Name} recognized." );

                    var conditions = new List<ScanCondition>{
                        new ScanCondition( "Date", ScanOperator.GreaterThanOrEqual, DateTime.Now )
                    };

                    switch ( intentRequest.Intent.Name )
                    {   
                        case "nexteventsIntent":
                        break;
                        case "nexteventByCityIntent":
                            var citySlot = intentRequest.Intent.Slots["city"];

                            context.Logger.LogLine( $"city = {citySlot.Value}" );

                            conditions.Add( new ScanCondition( "City", ScanOperator.Equal, citySlot.Value ) );
                        break;
                        default:
                            break;
                    }

                    var devdayEvents = await new DataManager()
                                        .GetAllAsync<DevdayEvent>( conditions );

                    if( devdayEvents.Count == 0 ) responseBuilder.Append( "Al momento non sono previsti nuovi eventi." );
                    else {
                        responseBuilder.Append( "I prossimi eventi sono: " );

                        foreach (var item in devdayEvents )
                        {
                            var itaDate = item.Date.ToString( "dddd, d MMMM yyyy", new CultureInfo("it-IT") );
                            var itaHour = item.Date.ToString( "H:mm", new CultureInfo("it-IT") );
                            
                            responseBuilder.Append( $"{item.Title} per {itaDate} alle {itaHour}, " );
                        }
                    }
                }
                else {
                    responseBuilder.Append( "Mi dispiace, non ho capito." );
                }

                var speech = new SsmlOutputSpeech { 
                    Ssml = $"<speak>{responseBuilder.ToString()}</speak>"
                };
                
                return ResponseBuilder.Tell( speech );
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
