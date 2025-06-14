﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Vosk;
using VoskNetApi.Models;

namespace VoskNetApi.Services;

public class TextRecognizeService : ITextRecognizeService
{
    private readonly SpkModel _spkModel;
    private readonly IAudioConvertService _audioConvertService;

    public TextRecognizeService(IAudioConvertService audioConvertService)
    {
        _audioConvertService = audioConvertService;
        _spkModel = ModelInitialization.SpeakerModel;

        Vosk.Vosk.GpuInit();
        Vosk.Vosk.GpuThreadInit();
        Vosk.Vosk.SetLogLevel(-1);
    }

    public TextRecognized Recognize(Stream stream, string filename,string lang)
    {
        var convertedStream = _audioConvertService.ConvertToWavStreamForRecognize(stream, filename);

        var recognizedChunks = RecognizeChunks(convertedStream,lang);

        var results = recognizedChunks.SelectMany(ch => ch?.Result ?? new List<Result>()).ToList();
        var text = string.Join(" ", recognizedChunks.Select(ch => ch.Text).ToList());

        return new TextRecognized()
        {
            Result = results,
            Text = text,
            Str = GetSubSentences(results)
        };
    }

    public string GetSubRip(List<Result> results)
    {
        try
        {
            var index = 1;
            return string.Join("\r\n", SplitByChunks(results, 5).Select(chank =>
                $"{index++}\r\n{TimeSpan.FromMilliseconds(chank.First().Start * 1000):hh\\:mm\\:ss\\,fff} --> {TimeSpan.FromMilliseconds(chank.Last().End * 1000):hh\\:mm\\:ss\\,fff}\r\n{string.Join(" ", chank.Select(ch => ch.Word))}\r\n")
            );
        }
        catch (Exception exp)
        {
            return string.Empty;
        }

    }

    public string GetSubSentences(List<Result> results)
    {
        try
        {
            string output = string.Empty;
            var index = 1;
            if(results.Count == 0)
            {
                return output;
            }
            else if(results.Count >= 1)
            {
                output = $"{index} {TimeSpan.FromMilliseconds(results[0].Start * 1000):hh\\:mm\\:ss\\,fff} {results[0].Word}";

                for(int i = 1; i < results.Count; i++)
                {                       
                    if (results[i].Start - results[i - 1].End < 0.4)
                    {
                        output += results[i].Word;
                    }
                    else
                    {
                        index++;
                        string newline = $"{index} {TimeSpan.FromMilliseconds(results[i].Start * 1000):hh\\:mm\\:ss\\,fff} {results[i].Word}";
                        output += "\r\n" + newline;
                    }
                }
            }

            return output;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static IEnumerable<IEnumerable<T>> SplitByChunks<T>(IEnumerable<T> source, int chunkSize)
    {
        if (chunkSize < 1)
            throw new ArgumentException("Chunk size must be greater than zero.");

        IEnumerator<T> enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return GetChunk(enumerator, chunkSize);
        }
    }

    private static IEnumerable<T> GetChunk<T>(IEnumerator<T> enumerator, int chunkSize)
    {
        int count = 0;
        do
        {
            yield return enumerator.Current;
        } while (++count < chunkSize && enumerator.MoveNext());
    }

    public List<VoskTextRecognized> RecognizeChunks(Stream stream,string lang)
    {
        var recognizedResults = new List<VoskTextRecognized>();

        using var rec = new VoskRecognizer(ModelInitialization.GetModel(lang), 16000.0f);
        rec.SetSpkModel(_spkModel);

        rec.SetMaxAlternatives(0);
        rec.SetWords(true);

        stream.Position = 0;
        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (!rec.AcceptWaveform(buffer, bytesRead)) continue;
            var result = rec.Result();
            var recognizedResultChunk = JsonSerializer.Deserialize<VoskTextRecognized>(result);
            recognizedResults.Add(recognizedResultChunk);
        }

        recognizedResults.Add(JsonSerializer.Deserialize<VoskTextRecognized>(rec.FinalResult()));

        return recognizedResults;
    }

}