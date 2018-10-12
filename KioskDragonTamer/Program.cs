﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NU.Kiosk.Speech
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Audio;
    using Microsoft.Psi.Components;
    using Microsoft.Psi.Data;
    using Microsoft.Psi.Speech;
    using Microsoft.Psi.Visualization.Client;
    using System.Speech.Recognition;
    using System.Threading.Tasks;
    using NDesk.Options;

    public static class Program
    {
        public static bool isDebug = false;
        public static bool isTraining = false;

        public enum ExecutionMode
        {
            Production_Run, Debug_Run, Train_Pairs, Train_Word_List
        }

        public static void Main(string[] args)
        {
            string phrase_audio_file_directory_path = @"Resources\Audio";
            string phrase_audio_file_list = @"text_audio_list.txt";
            string training_word_list_path = @"Resources\words_to_train.txt";
            ExecutionMode mode = ExecutionMode.Production_Run;
            bool show_help = false;

            var p = new OptionSet() {
                { "d|debug", "Stand-alone debug mode. All speeches are consumed locally; recognized speech will be echoed back.",
                   v => {
                        isDebug = true;
                        mode = ExecutionMode.Debug_Run;
                   }
                },
                { "p|train_pa", "Train Dragon with Phrase-Audio pairs with default pair list. ",
                   v => {
                       mode = v != null ? ExecutionMode.Train_Pairs : mode;
                       isTraining = true;
                   }
                },
                { "pa_dir=", "Train Dragon with Phrase-Audio pairs from the following directory. Default: Resources\\Audio\\",
                   v => {
                       phrase_audio_file_directory_path = v;
                       mode = ExecutionMode.Train_Pairs;
                       isTraining = true;
                   }
                },
                { "pa_list=", "Train Dragon with Phrase-Audio pairs specified in this file. Default: text_audio_list.txt, assuming pa_dir=Resources\\Audio\\ if unspecified.",
                   v => {
                        phrase_audio_file_list = v;
                        mode = ExecutionMode.Train_Pairs;
                        isTraining = true;
                   }
                },
                { "w|train_wl", "Interactively train Dragon the pronunciations of words from the default word list: Resources\\words_to_train.txt ",
                   v => {
                       mode = v != null ? ExecutionMode.Train_Word_List : mode;
                       isTraining = true;
                   }
                },
                { "wl=", "Provide a path to the word list and interactively train Dragon their pronunciations.",
                   v => {
                        training_word_list_path = v;
                        mode = ExecutionMode.Train_Word_List;
                        isTraining = true;
                   }
                },
                { "h|help", "Show this message and exit.",
                   v => {
                       show_help = v != null;
                   }
                },
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `program.exe --help' for more information.");
                return;
            }

            if (show_help)
            {
                Console.WriteLine("Usage: KioskDragonTamer.exe [OPTIONS]");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            switch (mode)
            {
                case ExecutionMode.Production_Run:
                    goto case ExecutionMode.Debug_Run;
                case ExecutionMode.Debug_Run:
                    Run();
                    break;
                case ExecutionMode.Train_Pairs:
                    TrainPhraseAudio(phrase_audio_file_directory_path, phrase_audio_file_list);
                    break;
                case ExecutionMode.Train_Word_List:
                    TrainWordList(training_word_list_path);
                    break;
                default:
                    break;
            }           
        }

        public static void Run()
        {
            var recognizer = new DragonRecognizer();
            var speechSynth = new DragonSpeechSynthesizer(recognizer);

            recognizer.Initialize();
            Console.WriteLine("[KioskDragonTamer.Run] Recognizer initialized.");
            speechSynth.Initialize();
            Console.WriteLine("[KioskDragonTamer.Run] Synthesizer initialized.");

            Console.WriteLine("[KioskDragonTamer.Run] Press any key to exit...");
            Console.ReadKey(true);
            // must go in this order
            speechSynth.Dispose();
            recognizer.Dispose();
        }

        public static void TrainPhraseAudio(string training_file_directory_path, string training_file_list)
        {
            var recognizer = new DragonRecognizer();
            recognizer.Initialize();
            recognizer.trainPhraseAudio(training_file_directory_path, training_file_list);
            Console.WriteLine("[KioskDragonTamer.Run] Press any key to exit...");
            Console.ReadKey(true);
            recognizer.Dispose();
        }

        public static void TrainWordList(string training_file_path)
        {
            var recognizer = new DragonRecognizer();
            recognizer.Initialize();
            recognizer.trainWordsFromWordList(training_file_path);
            Console.WriteLine("[KioskDragonTamer.Run] Press any key to exit...");
            Console.ReadKey(true);
            recognizer.Dispose();
        }

        public static void speechTester()
        {
            Console.WriteLine("[KioskDragonTamer.Run] Enter 'exit' to exit...");
            string input;
            var recognizer = new DragonRecognizer();
            DragonSpeechSynthesizer speechSynth = new DragonSpeechSynthesizer(recognizer);
            while ((input = Console.ReadLine()).ToLower() != "exit")
            {
                if (input.Trim().Length > 0)
                {
                    Console.WriteLine($"[KioskDragonTamer.Run] '{input}'");
                    speechSynth.Speak(input);
                }
                else
                {
                    Console.WriteLine($"[KioskDragonTamer.Run] Come again?");
                }
            }
        }

    }
}
