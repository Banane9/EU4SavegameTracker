using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CEParser.Decoding;

namespace CEParser.Decoding
{
    internal sealed class CEBinaryDecoder : CEDecoder
    {
        private readonly Stream stream;
        private readonly string token;
        public Ironmelt Decoder { get; }

        /// <summary>
        /// Creates a new file structure.
        /// </summary>
        public CEBinaryDecoder(Stream stream, Game game)
            : base(game)
        {
            this.stream = stream;
            token = game.Extension;
            Decoder = new Ironmelt(token, game.BinaryTokens, game.Encoding) { EnforceDateDatatype = true };
        }

        public override void Parse()
        {
            ParsePhase phase = ParsePhase.Looking;

            int sl = (int)stream.Length;
            DecodeResult lhs = new DecodeResult();

            try
            {
                stream.Position = 0;
                while (stream.Position < sl)
                {
                    if (stream.Position % 100000 == 0) OnFileParseProgress(stream.Position / (float)sl);

                    byte b = (byte)stream.ReadByte();
                    Decoder.Decode(b, hierarchy);
                    if (Decoder.Result.Token == null) continue;

                    switch (phase)
                    {
                        case ParsePhase.Looking:
                            switch (Decoder.Result.Token)
                            {
                                case "=": AddError((int)stream.Position, "Parsing error", "Unexpected equals block found, without left-hand side.", 100); break;
                                case "{": AddContainer(); break;
                                case "}": CloseContainer(); break;
                                default: lhs = Decoder.Result; phase = Decoder.Result.Quoted ? ParsePhase.LookingAfterRecordedQuotedLHS : ParsePhase.LookingAfterRecordedQuotelessLHS; break;
                            }
                            break;

                        case ParsePhase.LookingAfterRecordedQuotedLHS:
                            switch (Decoder.Result.Token)
                            {
                                case "=": phase = ParsePhase.LookingForRHS; break;
                                case "{": AddEntry(lhs); lhs = new DecodeResult(); AddContainer(); phase = ParsePhase.Looking; break;
                                case "}": AddEntry(lhs); lhs = new DecodeResult(); CloseContainer(); phase = ParsePhase.Looking; break;
                                default: AddEntry(lhs); lhs = Decoder.Result; phase = Decoder.Result.Quoted ? ParsePhase.LookingAfterRecordedQuotedLHS : ParsePhase.LookingAfterRecordedQuotelessLHS; break;
                            }
                            break;

                        case ParsePhase.LookingAfterRecordedQuotelessLHS:
                            switch (Decoder.Result.Token)
                            {
                                case "=": phase = ParsePhase.LookingForRHS; break;
                                case "{": AddEntry(lhs); lhs = new DecodeResult(); AddContainer(); phase = ParsePhase.Looking; break;
                                case "}": AddEntry(lhs); lhs = new DecodeResult(); CloseContainer(); phase = ParsePhase.Looking; break;
                                default: AddEntry(lhs); lhs = Decoder.Result; phase = Decoder.Result.Quoted ? ParsePhase.LookingAfterRecordedQuotedLHS : ParsePhase.LookingAfterRecordedQuotelessLHS; break;
                            }
                            break;

                        case ParsePhase.LookingForRHS:
                            switch (Decoder.Result.Token)
                            {
                                case "=": AddError((int)stream.Position, "Parsing error", "Duplicated equals block found, while looking for right-hand side.", 100); break;
                                case "{": AddContainer(lhs); lhs = new DecodeResult(); phase = ParsePhase.Looking; break;
                                case "}": AddError((int)stream.Position, "Parsing error", "Closing brace found, while looking for right-hand side.", 100); break;
                                default: AddAttribute(lhs, Decoder.Result); lhs = new DecodeResult(); phase = ParsePhase.Looking; break;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddError((int)stream.Position, "Unrecognized parsing error", ex.Message + "\n\n" + ex.StackTrace, 1000);
            }

            // Calculate severity
            foreach (var e in Errors)
            {
                e.CalculateSeverityScore();
            }

            OnFileParseProgress(1);
        }

        private void AddAttribute(DecodeResult lhs, DecodeResult rhs)
        {
            var n = new Decoding.Attribute(hierarchy.Peek(), lhs.Token, rhs.Token, rhs.Quoted);
            if (lhs.Unknown != null)
                AddError((int)stream.Position, "Unknown token", lhs.Unknown, 0, n);
            if (rhs.Unknown != null)
                AddError((int)stream.Position, "Unknown token", rhs.Unknown, 0, n);
        }

        private void AddContainer()
        {
            Node n = new Node(hierarchy.Peek(), "");
            hierarchy.Push(n);
        }

        private void AddContainer(DecodeResult name)
        {
            Node n = new Node(hierarchy.Peek(), name.Token);
            if (name.Unknown != null)
                AddError((int)stream.Position, "Unknown token", name.Unknown, 0, n);
            hierarchy.Push(n);
        }

        private void AddEntry(DecodeResult value)
        {
            Entry n = new Entry(hierarchy.Peek(), value.Token, value.Quoted);
            if (value.Unknown != null)
                AddError((int)stream.Position, "Unknown token", value.Unknown, 0, n);
        }

        private void CloseContainer()
        {
            if (hierarchy.Count > 1)
            {
                var n = hierarchy.Pop();
                n.PrepareCache();
            }
        }
    }
}