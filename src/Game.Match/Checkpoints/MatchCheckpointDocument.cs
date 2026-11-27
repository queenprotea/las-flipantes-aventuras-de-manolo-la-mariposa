using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Game.Match
{
    internal static class MatchCheckpointDocument
    {
        private const string WrongDocumentMessage = "The saved match state is not a match checkpoint.";

        private static readonly DataContractSerializer _checkpointSerializer = new DataContractSerializer(typeof(MatchCheckpoint));
        private static readonly XmlWriterSettings _writerSettings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
        };
        private static readonly XmlReaderSettings _readerSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        internal static string Write(MatchCheckpoint checkpoint)
        {
            var documentText = new StringBuilder();
            using (var documentWriter = XmlWriter.Create(documentText, _writerSettings))
            {
                _checkpointSerializer.WriteObject(documentWriter, checkpoint);
            }

            string stateDocument = documentText.ToString();

            return stateDocument;
        }

        internal static MatchCheckpoint Read(string stateDocument)
        {
            using var documentText = new StringReader(stateDocument);
            using var documentReader = XmlReader.Create(documentText, _readerSettings);
            MatchCheckpoint checkpoint = _checkpointSerializer.ReadObject(documentReader) as MatchCheckpoint ?? throw new SerializationException(WrongDocumentMessage);

            return checkpoint;
        }
    }
}
