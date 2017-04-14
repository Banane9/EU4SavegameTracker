using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

using System.Text;
using System.Reflection;

namespace CEParser
{
    public enum DataType
    {
        Unspecified, Integer, Float, Boolean, String, Float5, Date, Variable
    }

    public enum DecodePhase
    {
        Looking, LookingForValue, RecordingString
    }

    public struct DecodeResult
    {
        public bool Quoted;
        public string Token;
        public string Unknown;

        public DecodeResult(string token, bool quoted, string unknown)
        {
            Token = token;
            Quoted = quoted;
            Unknown = unknown;
        }
    }

    public class Ironmelt
    {
        private static Dictionary<int, string> dateLookup = new Dictionary<int, string>();
        private static Dictionary<int, string> float5Lookup = new Dictionary<int, string>();
        private static Dictionary<int, string> floatLookup = new Dictionary<int, string>();
        private static Dictionary<int, string> intLookup = new Dictionary<int, string>();
        private byte[] buffer = new byte[1024];
        private CultureInfo CI = CultureInfo.InvariantCulture;
        private Encoding encoding;
        private int enforcedOffset = 0;
        private string game;
        private int index = 0;
        private BinaryToken lastToken;
        private DecodePhase phase = DecodePhase.RecordingString;
        private bool quoted = false;
        private Stack<bool?> quotedInheritance = new Stack<bool?>();
        private int strlen = 0;
        private BinaryTokens tokens;
        private DataType type = DataType.Unspecified;
        private Stack<DataType> typeInheritance = new Stack<DataType>();

        #region Settings

        public bool EnforceDateDatatype { get; set; }

        #endregion Settings

        private int val;

        public DecodeResult Result
        {
            get; private set;
        }

        internal Ironmelt(string gameToken, BinaryTokens tokens, Encoding encoding)
        {
            game = gameToken;
            strlen = gameToken.Length + 3;
            this.tokens = tokens;
            this.encoding = encoding;
        }

        public void Decode(byte b, Stack<Node> hierarchy)
        {
            string output = null;
            string unknown = null;
            buffer[index++] = b;

            switch (phase)
            {
                case DecodePhase.Looking:
                    // Check for a special code first
                    if (index < 2)
                    {
                        break;
                    }
                    if (GetSpecialCode(buffer[0], buffer[1], ref type, ref output))
                    {
                        if (output == null) phase = DecodePhase.LookingForValue; //we can be sure that a value follows
                        index = 0;
                        break;
                    }
                    // No special code, then a token maybe?
                    else
                    {
                        var token = tokens.Get(buffer[0], buffer[1]); // from little-endian code
                        // We found a token
                        if (token != null)
                        {
                            index = 0;
                            lastToken = token;
                            type = token.DataType;
                            quoted = token.Quoted;
                            output = token.Text;
                            if (type == DataType.Variable) InterpretVariable(token.Text, hierarchy);
                            break;
                        }
                        else if (enforcedOffset > 0)
                        {
                            enforcedOffset--;
                            Result = new DecodeResult();
                            if (enforcedOffset == 0)
                            {
                                output = GetValue(ref strlen);
                                index = 0;
                                phase = DecodePhase.Looking;
                                Result = new DecodeResult(output, quoted, unknown);
                            }
                            return;
                        }
                        // Treat the token as unrecognized
                        else
                        {
                            phase = DecodePhase.Looking;
                            unknown = GetHexString(buffer[0], buffer[1]);
                            output = "UNKNOWN_" + unknown;
                            index = 0;
                        }
                    }
                    break;

                case DecodePhase.LookingForValue:
                    output = GetValue(ref strlen);
                    if (strlen > 0)
                    {
                        index = 0;
                        phase = DecodePhase.RecordingString;
                    }
                    else if (strlen < 0)
                    {
                        index = 0;
                        strlen = 0;
                        output = "";
                        phase = DecodePhase.Looking;
                    }
                    else if (output != null)
                    {
                        index = 0;
                        phase = DecodePhase.Looking;
                    }
                    break;

                case DecodePhase.RecordingString:
                    strlen--;
                    if (strlen < 1)
                    {
                        if (buffer[index - 1] == 10) index--;
                        output = encoding.GetString(buffer, 0, index);
                        index = 0;
                        phase = DecodePhase.Looking;
                    }

                    break;
            }

            // Handle inheritance
            if (output == "{" && lastToken != null)
            {
                typeInheritance.Push(lastToken.InheritType ? lastToken.DataType : DataType.Unspecified);
                if (lastToken.InheritQuoted)
                    quotedInheritance.Push(lastToken.Quoted);
                else
                    quotedInheritance.Push(null);

                type = DataType.Unspecified;
                quoted = lastToken.Quoted;
            }

            if (output == "}")
            {
                if (typeInheritance.Count > 0) typeInheritance.Pop();
                if (quotedInheritance.Count > 0) quotedInheritance.Pop();
                type = DataType.Unspecified;
                quoted = false;
            }

            Result = new DecodeResult(output, quoted, unknown);
        }

        private static string DecodeDate(int input)
        {
            int[] monthLength = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            int hour = input % 24;
            int year = -5000 + input / 24 / 365;
            int day = 1 + input / 24 % 365;
            int month = 1;

            for (int i = 0; i < monthLength.Length; i++)
            {
                if (day > monthLength[i])
                {
                    day -= monthLength[i];
                    month++;
                }
                else
                {
                    break;
                }
            }

            return year + "." + month + "." + day;
        }

        private static bool? GetInheritedQuoted(Stack<bool?> stack)
        {
            return stack.Count == 0 ? null : stack.Peek();
        }

        private static DataType GetInheritedType(Stack<DataType> stack)
        {
            return stack.Count == 0 ? DataType.Unspecified : stack.Peek();
        }

        private string GetHexString(byte b1, byte b2)
        {
            return b1.ToString("X").PadLeft(2, '0') + b2.ToString("X").PadLeft(2, '0');
        }

        private string GetParent(Stack<Node> hierarchy, int i)
        {
            try
            {
                return hierarchy.ElementAt(i).GetName();
            }
            catch
            {
                return "";
            }
        }

        private bool GetSpecialCode(byte b1, byte b2, ref DataType type, ref string s)
        {
            if (EnforceDateDatatype && type == DataType.Date)
            {
                if (b1 == 12 && b2 == 0 && (!EnforceDateDatatype || type == DataType.Date)) { return true; } //0x0C00
                if (b1 == 20 && b2 == 0 && (!EnforceDateDatatype || type == DataType.Date)) { return true; } //0x1400
            }

            if (b1 == 1 && b2 == 0) { s = "="; return true; } //0x0100
            else if (b1 == 3 && b2 == 0) { s = "{"; return true; } //0x0300
            else if (b1 == 4 && b2 == 0) { s = "}"; return true; } //0x0400

            if (b1 == 12 && b2 == 0) type = DataType.Integer; //0x0C00
            else if (b1 == 13 && b2 == 0) type = DataType.Float; //0x0D00
            else if (b1 == 14 && b2 == 0) type = DataType.Boolean; //0x0E00
            else if (b1 == 15 && b2 == 0) type = DataType.String; //0x0F00
            else if (b1 == 20 && b2 == 0) type = DataType.Integer; //0x1400
            else if (b1 == 23 && b2 == 0) type = DataType.String; //0x1700
            else if (b1 == 103 && b2 == 1) type = DataType.Float5; //0x6701
            else if (b1 == 144 && b2 == 1) type = DataType.Float5; //0x9001
            else if (b1 == 231 && b2 == 1) type = DataType.Float5; //0xE701
            else return false;
            return true;
        }

        private string GetValue(ref int strlen)
        {
            string output;
            switch (type)
            {
                case DataType.Integer:
                    if (index < 4) return null;
                    val = BitConverter.ToInt32(buffer, 0);
                    if (intLookup.TryGetValue(val, out output))
                    {
                        return output;
                    }
                    else
                    {
                        output = val.ToString(CI);
                        intLookup.Add(val, output);
                        return output;
                    }

                case DataType.Float:
                    if (index < 4) return null;
                    val = BitConverter.ToInt32(buffer, 0);
                    if (floatLookup.TryGetValue(val, out output))
                    {
                        return output;
                    }
                    else
                    {
                        output = String.Format(CI, "{0:0.000}", val / 1000f);
                        floatLookup.Add(val, output);
                        return output;
                    }

                case DataType.Boolean:
                    return buffer[0] == 1 ? "yes" : "no";

                case DataType.Float5:
                    if (index < 8) return null;
                    val = BitConverter.ToInt32(buffer, 0);
                    if (float5Lookup.TryGetValue(val, out output))
                    {
                        return output;
                    }
                    else
                    {
                        output = String.Format(CI, "{0:0.00000}", val * 2 / 256f / 256f);
                        float5Lookup.Add(val, output);
                        return output;
                    }

                case DataType.Date:
                    if (index < 4) return null;
                    val = BitConverter.ToInt32(buffer, 0);
                    if (dateLookup.TryGetValue(val, out output))
                    {
                        return output;
                    }
                    else
                    {
                        output = DecodeDate(val);
                        dateLookup.Add(val, output);
                        return output;
                    }

                case DataType.String:
                    if (index < 2) return null;
                    strlen = buffer[1] << 8 | buffer[0];
                    if (strlen == 0)
                        strlen = -1; // signify empty string
                    return null;
            }
            return null;
        }

        private void InterpretVariable(string text, Stack<Node> hierarchy)
        {
            #region HoI4

            if (game.StartsWith("hoi4"))
            {
                switch (text)
                {
                    case "version":
                        if (hierarchy.Count() == 1)
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else
                        {
                            type = DataType.Integer;
                            quoted = false;
                        }
                        break;

                    case "session":
                        type = DataType.Integer;
                        quoted = false;
                        enforcedOffset = 9;
                        break;
                }
            }

            #endregion HoI4

            #region EU4

            if (game.StartsWith("eu4"))
            {
                switch (text)
                {
                    case "type":
                        bool leaderTypeAsToken = IsParent(hierarchy, 3, "history") || (IsParent(hierarchy, 2, "monarch") && IsParent(hierarchy, 4, "history")) || (IsParent(hierarchy, 2, "heir") && IsParent(hierarchy, 4, "history"));

                        if (IsParent(hierarchy, 1, "id") ||
                            (IsParent(hierarchy, 1, "leader") && !leaderTypeAsToken) ||
                            (IsParent(hierarchy, 1, "rebel_faction") && IsParent(hierarchy, -1, "provinces")) ||
                            (IsParent(hierarchy, 1, "advisor") && IsParent(hierarchy, -1, "active_advisors")) ||
                            IsParent(hierarchy, 1, "monarch"))
                        {
                            type = DataType.Integer;
                            quoted = false;
                        }
                        else if ((IsParent(hierarchy, 1, "advisor") && IsParent(hierarchy, 3, "history")) ||
                                 (IsParent(hierarchy, 1, "advisor") && IsParent(hierarchy, 2, "history")))
                        {
                            type = DataType.String;
                            quoted = false;
                        }
                        else if (IsParent(hierarchy, 1, "advisor"))
                        {
                            type = DataType.Integer;
                            quoted = false;
                        }
                        else if (IsParent(hierarchy, 1, "war_goal"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else if (IsParent(hierarchy, 1, "general") || (IsParent(hierarchy, 1, "leader") && leaderTypeAsToken))
                        {
                            type = DataType.Unspecified;
                        }
                        else if (IsParent(hierarchy, 1, "rebel_faction") ||
                            IsParent(hierarchy, 1, "revolt") ||
                            IsParent(hierarchy, 1, "mercenary") ||
                            IsParent(hierarchy, 2, "previous_war"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else if ((IsParent(hierarchy, 1, "regiment") && !IsParent(hierarchy, 2, "military_construction")) ||
                            IsParent(hierarchy, 1, "ship") ||
                            IsParent(hierarchy, 1, "faction") ||
                            IsParent(hierarchy, 1, "military_construction") ||
                            IsParent(hierarchy, 1, "possible_mercenary") ||
                            IsParent(hierarchy, 1, "active_major_mission") ||
                            IsParent(hierarchy, 1, "casus_belli") ||
                            IsParent(hierarchy, 1, "take_province") ||
                            IsParent(hierarchy, 1, "take_core") ||
                            IsParent(hierarchy, 2, "active_war"))
                        {
                            type = DataType.String;
                            quoted = false;
                        }
                        else
                        {
                            type = DataType.Integer;
                            quoted = false;
                        }
                        break;

                    case "discovered_by":
                        type = DataType.String;
                        if (hierarchy.Count > 0 && GetParent(hierarchy, hierarchy.Count - 1).StartsWith("-"))
                        {
                            quoted = false;
                        }
                        break;

                    case "action":
                        if (IsParent(hierarchy, 1, "diplomacy_construction"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else if (IsParent(hierarchy, 1, "previous_war"))
                        {
                            type = DataType.Date;
                            quoted = false;
                        }
                        else
                        {
                            type = DataType.Integer;
                        }
                        break;

                    case "steer_power":
                        if (IsParent(hierarchy, 1, "node"))
                            type = DataType.Float;
                        else
                            type = DataType.Integer;
                        break;

                    case "value":
                        if (IsParent(hierarchy, 1, "improve_relation") ||
                            IsParent(hierarchy, 1, "warningaction") ||
                            IsParent(hierarchy, 1, "requestpeace") ||
                            IsParent(hierarchy, 1, "invite_to_federation"))
                        {
                            type = DataType.Boolean;
                        }
                        else
                        {
                            type = DataType.Float;
                        }
                        break;

                    case "unit_type":
                        if (hierarchy.Count > 0 && GetParent(hierarchy, hierarchy.Count - 1).StartsWith("O0"))
                        {
                            type = DataType.Unspecified;
                        }
                        else
                        {
                            type = DataType.String;
                            quoted = false;
                        }
                        break;

                    case "active":
                        if (IsParent(hierarchy, 1, "rebel_faction") ||
                            IsParent(hierarchy, 1, "siege_combat") ||
                            IsParent(hierarchy, 1, "combat"))
                        {
                            type = DataType.Unspecified;
                        }
                        else if (IsParent(hierarchy, 1, "fervor"))
                        {
                            type = DataType.String;
                            quoted = false;
                        }
                        else
                        {
                            type = DataType.Boolean;
                        }
                        break;

                    case "revolution_target":
                        if (IsParent(hierarchy, 2, "history"))
                        {
                            type = DataType.Unspecified;
                        }
                        else
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        break;

                    case "succession":
                        if (IsParent(hierarchy, 1, "history"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else
                        {
                            type = DataType.Unspecified;
                        }
                        break;

                    case "ADM":
                    case "DIP":
                    case "MIL":
                        if (IsParent(hierarchy, 2, "countries"))
                        {
                            break;
                        }
                        type = DataType.Integer;
                        break;
                }
            }

            #endregion EU4

            #region CK2

            else if (game.StartsWith("ck2"))
            {
                switch (text)
                {
                    case "type":
                        if (IsParent(hierarchy, 3, "provinces") ||
                            IsParent(hierarchy, 1, "settlement") ||
                            IsParent(hierarchy, 1, "holder") ||
                            IsParent(hierarchy, 2, "character"))
                        {
                            type = DataType.Unspecified;
                        }
                        else if (IsParent(hierarchy, 1, "active_ambition") ||
                            IsParent(hierarchy, 1, "active_plot") ||
                            IsParent(hierarchy, 1, "active_faction") ||
                            IsParent(hierarchy, 1, "chronicle_entry") ||
                            IsParent(hierarchy, 1, "active_focus"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else
                        {
                            type = DataType.Integer;
                        }
                        break;

                    case "title":
                        if (IsParent(hierarchy, 1, "dyn_title") ||
                            IsParent(hierarchy, 1, "title") ||
                            IsParent(hierarchy, 1, "de_jure_liege") ||
                            IsParent(hierarchy, 1, "assimilating_liege") ||
                            IsParent(hierarchy, 2, "character") ||
                            IsParent(hierarchy, 2, "demesne") ||
                            IsParent(hierarchy, 1, "liege") ||
                            IsParent(hierarchy, 1, "pentarch") ||
                            IsParent(hierarchy, 1, "landed_title") ||
                            IsParent(hierarchy, 1, "owner"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else
                        {
                            quoted = false;
                        }
                        break;

                    case "build_time":
                        type = DataType.String;
                        quoted = true;

                        break;

                    case "event":
                        if (IsParent(hierarchy, 1, "delayed_event"))
                        {
                            type = DataType.String;
                            quoted = true;
                        }
                        else
                        {
                            type = DataType.Integer;
                        }
                        break;

                    case "value":
                        if (IsParent(hierarchy, 1, "battle"))
                        {
                            type = DataType.Unspecified;
                        }
                        else
                        {
                            type = DataType.Integer;
                        }
                        break;

                    default:
                        break;
                }
            }

            #endregion CK2
        }

        private bool IsParent(Stack<Node> hierarchy, int levelsUp, string text)
        {
            int parentsCount = hierarchy.Count;

            if (parentsCount < 1) return false;
            if (levelsUp > parentsCount || levelsUp == 0) return false;
            if (levelsUp < 0) return text == GetParent(hierarchy, 0);
            return GetParent(hierarchy, levelsUp - 1) == text;
        }
    }
}