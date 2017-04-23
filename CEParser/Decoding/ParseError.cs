using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEParser.Decoding
{
    public sealed class ParseError
    {
        public string Details { get; set; }
        public Entity Entity { get; set; }
        public string Error { get; set; }
        public int Position { get; set; }
        public int SeverityScore { get; set; }

        public ParseError(int position, string error, string details, int score) : this(position, error, details, score, null)
        {
        }

        public ParseError(int position, string error, string details, int score, Entity entity)
        {
            Position = position;
            Error = error;
            Details = details;
            SeverityScore = score;
            Entity = entity;
        }

        public void CalculateSeverityScore()
        {
            if (SeverityScore > 0 || Entity == null)
                return;

            SeverityScore = Entity.GetChildrenCount() + 1;
        }
    }
}