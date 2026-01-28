using Microsoft.ML.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.TokenService
{
    internal class CalculateTokenService(string model) : ICalculateTokenService
    {
        
        private readonly Tokenizer _tokenizer = TiktokenTokenizer.CreateForModel(model);
        public string Model { get; } = model;

        public int CountTokens(string text)
        {
            return _tokenizer.CountTokens(text);
        }
    }
}
