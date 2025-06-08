using System.IO;
using VoskNetApi.Models;

namespace VoskNetApi.Services;

public interface ITextRecognizeService
{
    TextRecognized Recognize(Stream stream, string filename,string lang);
}