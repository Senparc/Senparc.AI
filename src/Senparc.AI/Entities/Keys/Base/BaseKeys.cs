using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.Entities.Keys
{
    /// <summary>
    /// Keys base class
    /// </summary>
    [Serializable]
    public class BaseKeys
    {
        /// <summary>
        /// model name configuration
        /// </summary>
        public ModelName ModelName { get; set; } = new ModelName();
    }

    /// <summary>
    /// model name configuration
    /// </summary>
    [Serializable]
    public class ModelName
    {
        /// <summary>
        /// text completion model name
        /// </summary>
        public string TextCompletion { get; set; }

        /// <summary>
        /// Chat model name
        /// </summary>
        public string Chat { get; set; }

        /// <summary>
        /// Embedding model name
        /// </summary>
        public string Embedding { get; set; }

        /// <summary>
        /// Embedding dimensions
        /// </summary>
        public int? EmbeddingDimensions { get; set; }

        /// <summary>
        /// text-to-image model name
        /// </summary>
        public string TextToImage { get; set; }

        /// <summary>
        /// image-to-text model name
        /// </summary>
        public string ImageToText { get; set; }

        /// <summary>
        /// text-to-speech model name
        /// </summary>
        public string TextToSpeech { get; set; }

        /// <summary>
        /// speech-to-text model name(Whisper)
        /// </summary>
        public string SpeechToText { get; set; }
    }
}
