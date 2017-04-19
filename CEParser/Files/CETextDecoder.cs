using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CEParser.Tokenization;

namespace CEParser.Files
{
    internal sealed class CETextDecoder : CEDecoder
    {
        private StreamReader stream;
        internal bool LineFeed { get; private set; }

        /// <summary>
        /// Creates a new file structure.
        /// </summary>
        /// <param name="path">Path to the file</param>
        public CETextDecoder(StreamReader stream) : base()
        {
            this.stream = stream;
            DetectLineFeed();
        }

        public CETextDecoder(MemoryStream stream) : base()
        {
            this.stream = new StreamReader(stream, System.Text.Encoding.GetEncoding(1250), true);
            DetectLineFeed();
        }

        public CETextDecoder(string path) : this(GetStreamReader(path))
        {
        }

        /// <summary>
        /// Creates a new parentless, zero-depth container out of a file.
        /// </summary>
        public override void Parse()
        {
            stream.BaseStream.Seek(0, SeekOrigin.Begin);
            int eolLength = LineFeed ? 2 : 1;

            // Initialize parsing
            char[] lBuf = new char[65536];
            char[] rBuf = new char[65536];
            int lIndex = 0;
            int rIndex = 0;
            ParsePhase phase = ParsePhase.Looking;

            while (!stream.EndOfStream)
            {
                if (lIndex >= 65535) lIndex = 0;
                if (rIndex >= 65535) rIndex = 0;
                if (stream.BaseStream.Position % 100000 == 0) OnFileParseProgress(stream.BaseStream.Position / (float)stream.BaseStream.Length);
                char buffer = (char)stream.Read();

                if (buffer == '\r') continue;

                switch (phase)
                {
                    case ParsePhase.Looking:
                        if (buffer == '"') { phase = ParsePhase.RecordingQuotedLHS; }
                        else if (buffer == '#') { phase = ParsePhase.SkippingComments; }
                        else if (IsLegalLHSCharacter(buffer)) { lBuf[lIndex++] = buffer; phase = ParsePhase.RecordingQuotelessLHS; }
                        else if (buffer == '=') { AddError((int)stream.BaseStream.Position, "Parsing error", "Assignment operator without left-hand side found.", 100); }
                        else if (buffer == '{')
                        {
                            AddContainer("");
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '}')
                        {
                            CloseContainer();
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        break;

                    case ParsePhase.RecordingQuotedLHS:
                        if (buffer == '"') { phase = ParsePhase.LookingAfterRecordedQuotedLHS; }
                        else { lBuf[lIndex++] = buffer; }
                        break;

                    case ParsePhase.RecordingQuotelessLHS:
                        if (buffer == '\t' || buffer == ' ') { phase = ParsePhase.LookingAfterRecordedQuotelessLHS; }
                        else if (buffer == '=') { phase = ParsePhase.LookingForRHS; }
                        else if (buffer == '\n')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '#')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.SkippingComments;
                        }
                        else if (buffer == '{')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            AddContainer("");
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '}')
                        {
                            CloseContainer();
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '"') { AddError((int)stream.BaseStream.Position, "Parsing error", "Unexpected quote found.", 100); }
                        else { lBuf[lIndex++] = buffer; }
                        break;

                    case ParsePhase.LookingAfterRecordedQuotelessLHS:
                        if (buffer == '=') { phase = ParsePhase.LookingForRHS; continue; }
                        else if (buffer == '\n')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '#')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.SkippingComments;
                        }
                        else if (buffer == '"')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.RecordingQuotedLHS;
                        }
                        else if (IsLegalLHSCharacter(buffer))
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            lIndex = 0; rIndex = 0;
                            lBuf[lIndex++] = buffer;
                            phase = ParsePhase.RecordingQuotelessLHS;
                        }
                        else if (buffer == '{')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            AddContainer("");
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '}')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), false);
                            CloseContainer();
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        break;

                    case ParsePhase.LookingAfterRecordedQuotedLHS:
                        if (buffer == '=') { phase = ParsePhase.LookingForRHS; continue; }
                        else if (buffer == '\n')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '#')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.SkippingComments;
                        }
                        else if (buffer == '"')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.RecordingQuotedLHS;
                        }
                        else if (IsLegalLHSCharacter(buffer))
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            lIndex = 0; rIndex = 0;
                            lBuf[lIndex++] = buffer;
                            phase = ParsePhase.RecordingQuotelessLHS;
                        }
                        else if (buffer == '{')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            AddContainer("");
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '}')
                        {
                            AddEntry(new string(lBuf, 0, lIndex), true);
                            CloseContainer();
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        break;

                    case ParsePhase.LookingForRHS:
                        if (buffer == '"') { phase = ParsePhase.RecordingQuotedRHS; }
                        else if (IsLegalRHSCharacter(buffer)) { rBuf[rIndex++] = buffer; phase = ParsePhase.RecordingQuotelessRHS; }
                        else if (buffer == '{')
                        {
                            AddContainer(new string(lBuf, 0, lIndex));
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '\n')
                        {
                            // We must allow a situation where there's an assignment operator, an endline and then opening brace
                            break;
                        }
                        else if (buffer == '#') { AddError((int)stream.BaseStream.Position, "Parsing error", "Comments appear after an assignment operator.", 100); }
                        else if (buffer == '=') { AddError((int)stream.BaseStream.Position, "Parsing error", "Double assignment operator.", 100); }
                        else if (buffer == '}') { AddError((int)stream.BaseStream.Position, "Parsing error", "Closing brace appears after an assignment operator.", 100); }
                        break;

                    case ParsePhase.RecordingQuotedRHS:
                        if (buffer == '"')
                        {
                            AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), true);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else { rBuf[rIndex++] = buffer; }
                        break;

                    case ParsePhase.RecordingQuotelessRHS:
                        if (buffer == ' ' || buffer == '\t')
                        {
                            AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '\n')
                        {
                            AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '"')
                        {
                            AddError((int)stream.BaseStream.Position, "Parsing error", "Unexpected quote found.", 100);
                        }
                        else if (buffer == '{')
                        {
                            AddError((int)stream.BaseStream.Position, "Parsing error", "Unexpected opening brace found.", 100);
                        }
                        else if (buffer == '}')
                        {
                            AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), false);
                            CloseContainer();
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.Looking;
                        }
                        else if (buffer == '#')
                        {
                            AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), false);
                            lIndex = 0; rIndex = 0;
                            phase = ParsePhase.SkippingComments;
                        }
                        else if (buffer == '=')
                        {
                            AddError((int)stream.BaseStream.Position, "Parsing error", "Double assignment operator.", 100);
                        }
                        else { rBuf[rIndex++] = buffer; }
                        break;

                    case ParsePhase.SkippingComments:
                        if (buffer == '\n') { phase = ParsePhase.Looking; }
                        break;
                }
            }

            // Finish parsing and exit
            switch (phase)
            {
                case ParsePhase.RecordingQuotedLHS: AddError((int)stream.BaseStream.Position, "Parsing error", "No closing quote at the end of the file.", 100); break;
                case ParsePhase.LookingAfterRecordedQuotedLHS:
                    AddEntry(new string(lBuf, 0, lIndex), true);
                    break;

                case ParsePhase.RecordingQuotelessLHS:
                case ParsePhase.LookingAfterRecordedQuotelessLHS:
                    AddEntry(new string(lBuf, 0, lIndex), false);
                    break;

                case ParsePhase.LookingForRHS: AddError((int)stream.BaseStream.Position, "Parsing error", "Assignment without right-hand side found at the end of the file.", 100); break;
                case ParsePhase.RecordingQuotedRHS: AddError((int)stream.BaseStream.Position, "Parsing error", "No closing quote at the end of the file.", 100); break;
                case ParsePhase.RecordingQuotelessRHS:
                    AddAttribute(new string(lBuf, 0, lIndex), new string(rBuf, 0, rIndex), false);
                    break;
            }

            OnFileParseProgress(1);
        }

        /// <summary>
        /// Returns true if a character can appear legally as a part of a left-hand side of an attribute or a container or
        /// to form an entry, otherwise false.
        /// </summary>
        /// <param name="data">A character to check</param>
        /// <returns>True if a character can be a legal left-hand side character, otherwise false.</returns>
        internal static bool IsLegalLHSCharacter(char data)
        {
            return Char.IsLetterOrDigit(data) || data == '_' || data == '-' || data == '+';
        }

        /// <summary>
        /// Returns true if a character can appear legally as a part of a right-hand side of an attribute, otherwise false.
        /// </summary>
        /// <param name="data">A character to check</param>
        /// <returns>True if a character can be a legal right-hand side character, otherwise false.</returns>
        internal static bool IsLegalRHSCharacter(char data)
        {
            return IsLegalLHSCharacter(data);
        }

        private void AddAttribute(string name, string value, bool quoted)
        {
            var n = new Tokenization.Attribute(hierarchy.Peek(), name, value, quoted);
        }

        private void AddContainer(string name)
        {
            Node n = new Node(hierarchy.Peek(), name);
            hierarchy.Push(n);
        }

        private void AddEntry(string value, bool quoted)
        {
            Entry n = new Entry(hierarchy.Peek(), value, quoted);
        }

        private void CloseContainer()
        {
            if (hierarchy.Count > 1)
            {
                var n = hierarchy.Pop();
                n.PrepareCache();
            }
        }

        private void DetectLineFeed()
        {
            try
            {
                char[] header = new char[1024];

                stream.BaseStream.Seek(0, SeekOrigin.Begin);
                stream.ReadBlock(header, 0, header.Length);
                LineFeed = false;
                for (int i = 0; i < header.Length; i++)
                {
                    if (header[i] == '\r') { LineFeed = true; continue; }
                }
            }
            catch (Exception)
            {
                throw new IOException("Error initializing text file.");
            }
        }
    }
}