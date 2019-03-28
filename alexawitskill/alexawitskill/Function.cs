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

namespace alexawitskill
{
    public class Function
    {
        private SkillResponse response = null;
        private ILambdaContext context = null;
        const string LOCALENAME = "locale";
        const string USA_Locale = "en-US";
        static Random rand = new Random();
        private static List<FactData> resources = null;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
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

                    var facts = GetFacts();
                    ProcessLaunchRequest(facts, response.Response);
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
                        var facts = GetFacts();

                        if (IsDialogIntentRequest(input))
                        {
                            if (!IsDialogSequenceComplete(input))
                            { // delegate to Alexa until dialog is complete
                                CreateDelegateResponse();
                                return response;
                            }
                        }

                        if (!ProcessDialogRequest(facts, input, response))
                        {
                            response.Response.OutputSpeech = ProcessIntentRequest(facts, input);
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
        /// <param name="factdata"></param>
        /// <param name="response"></param>
        /// <returns>void</returns>
        private void ProcessLaunchRequest(FactData factdata, ResponseBody response)
        {
            if (factdata != null)
            {
                IOutputSpeech innerResponse = new SsmlOutputSpeech();
                (innerResponse as SsmlOutputSpeech).Ssml = SsmlDecorate(factdata.LaunchMessage);
                response.OutputSpeech = innerResponse;
                IOutputSpeech prompt = new PlainTextOutputSpeech();
                (prompt as PlainTextOutputSpeech).Text = factdata.LaunchMessageReprompt;
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
        private bool ProcessDialogRequest(FactData data, SkillRequest input, SkillResponse response)
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
        private IOutputSpeech ProcessIntentRequest(FactData factdata, SkillRequest input)
        {
            var intentRequest = input.Request;
            IOutputSpeech innerResponse = new PlainTextOutputSpeech();

            switch (intentRequest.Intent.Name)
            {
                case "GetNewFactIntent":
                    innerResponse = new SsmlOutputSpeech();
                    (innerResponse as SsmlOutputSpeech).Ssml = GetNewFact(factdata, true);
                    break;
                case AlexaConstants.CancelIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = factdata.StopMessage;
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.StopIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = factdata.StopMessage;
                    response.Response.ShouldEndSession = true;
                    break;

                case AlexaConstants.HelpIntent:
                    (innerResponse as PlainTextOutputSpeech).Text = factdata.HelpMessage;
                    break;

                default:
                    (innerResponse as PlainTextOutputSpeech).Text = factdata.HelpReprompt;
                    break;
            }
            if (innerResponse.Type == AlexaConstants.SSMLSpeech)
            {
                BuildCard(factdata.SkillName, (innerResponse as SsmlOutputSpeech).Ssml);
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
        ///  Get a new random fact from the fact list.
        /// </summary>
        /// <param name="factdata"></param>
        /// <param name="withPreface"></param>
        /// <returns>string newfact</returns>
        private string GetNewFact(FactData factdata, bool withPreface)
        {
            string preface = string.Empty;
            if (factdata == null)
            {
                return string.Empty;
            }

            if (withPreface)
            {
                preface = factdata.GetFactMessage;
            }

            return preface + factdata.Facts[rand.Next(factdata.Facts.Count)] + factdata.AskMessage;
        }

        /// <summary>
        /// Get the facts from the list
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private FactData GetFacts()
        {
            if (resources == null)
            {
                resources = FactData.LoadFacts();
            }

            foreach (FactData factdata in resources)
            {
                return factdata;
            }
            return null;
        }




        // <summary>
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
