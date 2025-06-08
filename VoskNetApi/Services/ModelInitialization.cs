using System;
using System.Collections.Generic;
using System.IO;
using Vosk;

namespace VoskNetApi.Services;

public static class ModelInitialization
{
        
    // private static Model _textModel = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "dll/TextModel/ja"));
    private static SpkModel _speakerModel = new(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "dll/SpeakerModel"));
    // public static Model TextModel => _textModel;
    public static SpkModel SpeakerModel => _speakerModel;
    public static Dictionary<string,Model> Models = [];
    // public static Dictionary<string,SpkModel> SpkModels = [];

    public static Model GetModel(string lang)
    {
        if (Models.ContainsKey(lang))
        {
            return Models[lang];
        }

        var model = new Model(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "dll/TextModel/"+lang));
        Models.Add(lang, model);
        return model;
    }

    // public static SpkModel getSpkModel(string lang)
    // {
    //     if (SpkModels.ContainsKey(lang))
    //     {
    //         return SpkModels[lang];
    //     }
    //     var spkModel = new SpkModel(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "dll/SpkModel/"+lang));
    //     SpkModels.Add(lang, spkModel);
    //     return spkModel;
    // }
}