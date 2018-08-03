﻿using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using Microsoft.Psi.Speech;
using NU.Kiosk.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NU.Kqml
{
    public class KioskInputTextPreProcessor : ConsumerProducer<IStreamingSpeechRecognitionResult, string>
    {
        public static bool isUsingIsAccepting = false;
        private bool isAccepting = true;
        private SystemSpeechRecognizer recognizer;
        private int ReloadMessageIDCurrent = 0;
        private static Regex quote_s = new Regex("[ ][']s");

        public KioskInputTextPreProcessor(Pipeline pipeline, SystemSpeechRecognizer rec)
            : base(pipeline)
        {
            this.recognizer = rec;
        }
        
        protected override void Receive(IStreamingSpeechRecognitionResult result, Envelope e)
        {
            string message = quote_s.Replace(result.Text, "'s");
            double confidence = result.Confidence.Value;
            Console.WriteLine($"[KioskInputTextPreProcessor] Received {message} with confidence {confidence}; ");// isAccepting {isAccepting}.");
            if ((!isUsingIsAccepting || isUsingIsAccepting && isAccepting) && confidence > 0.3)
            {
                //isAccepting = false;
                switch (message)
                {
                    case "":
                    case "Hi":
                    case "Hello":
                    case "Greetings":
                    case "Good morning":
                    case "Sup":
                    case "okay":
                    case "hm":
                    case "um":
                    case "ah":
                    case "cool":
                    case "huh?":
                    case "wow!":
                    case "Huck you":
                    case "bye":
                    case "bye bye":
                        Console.WriteLine($"[KioskInputTextPreProcessor] Discarding message: {message}");
                        if (isUsingIsAccepting)
                        {
                            isAccepting = true;
                        } else
                        {
                            this.Out.Post(null, DateTime.Now);
                        }
                        break;
                    case "Reload grammars":
                        Console.WriteLine($"[KioskInputTextPreProcessor] Reloading grammar.");
                        {
                            var gw = new GrammarWriter();
                            gw.ReadFileAndConvert();
                            string updatedGrammar = gw.GetResultString();

                            DateTime post_time = new DateTime();

                            Message<System.Collections.Generic.IEnumerable<String>> updateRequest =
                                new Message<System.Collections.Generic.IEnumerable<String>>(
                                    new String[] {
                                    updatedGrammar
                                    }, post_time, post_time, 9876, ReloadMessageIDCurrent++);
                            recognizer.SetGrammars(updateRequest);
                            gw.WriteToFile();
                            if (isUsingIsAccepting)
                            {
                                isAccepting = true;
                            }
                            else
                            {
                                this.Out.Post(null, DateTime.Now);
                            }
                            break;
                        }
                    default:
                        this.Out.Post(message, DateTime.Now);
                        Console.WriteLine($"[KioskInputTextPreProcessor] Starting timer.");
                        if (isUsingIsAccepting)
                        {
                            restartAcceptingInMs(10000); 
                        } else
                        {
                            // do nothing
                        }
                        break;
                }
            }
        }

        async Task delay(int ms)
        {
            await Task.Delay(ms);
        }

        private async void restartAcceptingInMs(int ms)
        {
            await delay(ms);
            if (this.isAccepting == false)
            {
                setAccepting();
                Console.WriteLine($"[KioskInputTextPreProcessor] Timer stopped; once again accepting.");
            }
            else
            {
                Console.WriteLine($"[KioskInputTextPreProcessor] Timer stopped; already accepting.");
            }
        }

        public void setAccepting()
        {
            this.isAccepting = true;
        }
    }
}
