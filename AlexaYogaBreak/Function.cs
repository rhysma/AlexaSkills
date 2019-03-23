using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using AlexaAPI;
using AlexaAPI.Request;
using AlexaAPI.Response;
using System.IO;
using System.Text.RegularExpressions;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaYogaBreak
{
    public class Function
    {
        private SkillResponse response = null;
        private ILambdaContext context = null;
        const string LOCALENAME = "locale";
        const string USA_Locale = "en-US";


        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="input"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext ctx)
        {
            context = ctx;
            try
            {
                response = new SkillResponse();
                response.Response = new ResponseBody();
                response.Response.ShouldEndSession = false;
                response.Version = AlexaConstants.AlexaVersion;

                if (input.Request.Type.Equals(AlexaConstants.LaunchRequest))
                {
                    string locale = input.Request.Locale;
                    if (string.IsNullOrEmpty(locale))
                    {
                        locale = USA_Locale;
                    }

                    var session = NewSession(locale);
                    ProcessLaunchRequest(session, response.Response);
                    response.SessionAttributes = new Dictionary<string, object>() { { LOCALENAME, locale } };
                }
                else
                {
                    if (input.Request.Type.Equals(AlexaConstants.IntentRequest))
                    {
                        string locale = string.Empty;
                        Dictionary<string, object> dictionary = input.Session.Attributes;
                        if (dictionary != null)
                        {
                            if (dictionary.ContainsKey(LOCALENAME))
                            {
                                locale = (string)dictionary[LOCALENAME];
                            }
                        }

                        if (string.IsNullOrEmpty(locale))
                        {
                            locale = input.Request.Locale;
                        }

                        if (string.IsNullOrEmpty(locale))
                        {
                            locale = USA_Locale;
                        }

                        response.SessionAttributes = new Dictionary<string, object>() { { LOCALENAME, locale } };
                        var session = NewSession(locale);

                        if (IsDialogIntentRequest(input))
                        {
                            if (!IsDialogSequenceComplete(input))
                            { // delegate to Alexa until dialog is complete
                                CreateDelegateResponse();
                                return response;
                            }
                        }

                        if (!ProcessDialogRequest(session, input, response))
                        {
                            response.Response.OutputSpeech = ProcessIntentRequest(session, input);
                        }
                    }
                }
                Log(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Log($"error :" + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Process and respond to the LaunchRequest with launch
        /// and reprompt message
        /// </summary>
        /// <param name="data"></param>
        /// <param name="response"></param>
        /// <returns>void</returns>
        private void ProcessLaunchRequest(YogaBreak data, ResponseBody response)
        {
            if (data != null)
            {
                IOutputSpeech innerResponse = new SsmlOutputSpeech();
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate(data.LaunchMessage);
                response.OutputSpeech = innerResponse;
                IOutputSpeech prompt = new PlainTextOutputSpeech();
                (prompt as PlainTextOutputSpeech).Text = data.LaunchMessageReprompt;
                response.Reprompt = new Reprompt()
                {
                    OutputSpeech = prompt
                };
            }
        }

        /// <summary>
        /// Check if its IsDialogIntentRequest, e.g. part of a Dialog sequence
        /// </summary>
        /// <param name="input"></param>
        /// <returns>bool true if a dialog</returns>
        private bool IsDialogIntentRequest(SkillRequest input)
        {
            if (string.IsNullOrEmpty(input.Request.DialogState))
                return false;
            return true;
        }

        /// <summary>
        /// Check if its Dialog sequence is complete
        /// </summary>
        /// <param name="input"></param>
        /// <returns>bool true if dialog complete set</returns>
        private bool IsDialogSequenceComplete(SkillRequest input)
        {
            if (input.Request.DialogState.Equals(AlexaConstants.DialogStarted)
               || input.Request.DialogState.Equals(AlexaConstants.DialogInProgress))
            {
                return false;
            }
            else
            {
                if (input.Request.DialogState.Equals(AlexaConstants.DialogCompleted))
                {
                    return true;
                }
            }
            return false;
        }

        // <summary>
        ///  Process intents that are dialog based and may not have a speech
        ///  response. Speech responses cannot be returned with a delegate response
        /// </summary>
        /// <param name="data"></param>
        /// <param name="input"></param>
        /// <param name="response"></param>
        /// <returns>bool true if processed</returns>
        private bool ProcessDialogRequest(YogaBreak data, SkillRequest input, SkillResponse response)
        {
            var intentRequest = input.Request;
            string speech_message = string.Empty;
            bool processed = false;

            return processed;
        }

        /// <summary>
        ///  prepare text for Ssml display
        /// </summary>
        /// <param name="speech"></param>
        /// <returns>string</returns>
        private string SsmlDecorate(string speech)
        {
            return "<speak>" + speech + "</speak>";
        }

 
        /// <summary>
        /// Process all not dialog based Intents
        /// </summary>
        /// <param name="factdata"></param>
        /// <param name="input"></param>
        /// <returns>IOutputSpeech innerResponse</returns>
        private IOutputSpeech ProcessIntentRequest(YogaBreak data, SkillRequest input)
        {
            var intentRequest = input.Request;
            IOutputSpeech innerResponse = new PlainTextOutputSpeech();

            switch (intentRequest.Intent.Name)
            {
                case "StartYogaBreakIntent":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = StartYogaSession(data, true);
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.CancelIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = data.StopMessage;
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.StopIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = data.StopMessage;
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.HelpIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = data.HelpMessage;
                    break;

                default:
                    (innerResponse as PlainTextOutputSpeech).Text = data.HelpReprompt;
                    break;
            }
            if (innerResponse.Type == AlexaConstants.SSMLSpeech)
            {
                BuildCard(data.SkillName, (innerResponse as SsmlOutputSpeech).Ssml);
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate((innerResponse as SsmlOutputSpeech).Ssml);
            }
            return innerResponse;
        }

        /// <summary>
        /// Build a simple card, setting its title and content field 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns>void</returns>
        private void BuildCard(string title, string output)
        {
            if (!string.IsNullOrEmpty(output))
            {
                output = Regex.Replace(output, @"<.*?>", "");
                response.Response.Card = new SimpleCard()
                {
                    Title = title,
                    Content = output,
                };
            }
        }



        /// <summary>
        ///  create a delegate response, we delegate all the dialog requests
        ///  except "Complete"
        /// </summary>
        /// <returns>void</returns>
        private void CreateDelegateResponse()
        {
            DialogDirective dld = new DialogDirective()
            {
                Type = AlexaConstants.DialogDelegate
            };
            response.Response.Directives.Add(dld);
        }

        /// <summary>
        ///  create a invalid slot value message        
        /// </summary>
        /// <param name="data"></param>
        /// <returns>string invalid planet name or empty string</returns>
        private string InvalidSlotMessage(YogaBreak data, string departKey, string arriveKey)
        {
            string output = String.Empty;

            return output;
        }

        /// <summary>
        ///  Runs the yoga break session
        /// </summary>
        /// <param name="data"></param>
        /// <param name="withPreface"></param>
        /// <returns>string newfact</returns>
        private string StartYogaSession(YogaBreak data, bool withPreface)
        {
            string preface = string.Empty;
            if (data == null)
            {
                return string.Empty;
            }

            if (withPreface)
            {
                preface = data.StartSessionMessage;
            }

            return preface + @"<break time=""3s""/>" + data.FirstPose + data.SecondPose + data.ThirdPose + data.FourthPose + data.FifthPose + data.SixthPose + data.SeventhPose + data.EighthPose + data.NinthPose + data.StopMessage;
        }

        /// <summary>
        /// Get the fatcs and questions for the specified locale
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>factdata for the locale</returns>
        private YogaBreak NewSession(string locale)
        {

            if (string.IsNullOrEmpty(locale))
            {
                locale = USA_Locale;
            }

            return YogaBreak.NewSession();


        }

        /// <summary>
        /// logger interface
        /// </summary>
        /// <param name="text"></param>
        /// <returns>void</returns>
        private void Log(string text)
        {
            if (context != null)
            {
                context.Logger.LogLine(text);
            }
        }
    }
}
