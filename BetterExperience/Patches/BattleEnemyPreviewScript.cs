using nel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XX;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 独立读取 CSV 行，使用复制的数据和本地表达式计算器。
    /// 未知表达式降级为候选名单，不调用 CsvReaderA 的游戏表达式回调。
    /// </summary>
    internal sealed class BattleEnemyPreviewScript
    {
        internal readonly List<BattleEnemyPreview.Spawn> Spawns = new List<BattleEnemyPreview.Spawn>();
        internal readonly List<BattleEnemyPreview.CountCap> Caps = new List<BattleEnemyPreview.CountCap>();
        internal int TotalCap = 99;
        internal bool Incomplete;
        internal bool DynamicReinforcements;
        internal string UndeterminedReason;
        private readonly BattleEnemyPreviewContext _context;
        private readonly Dictionary<string, string> _variables;
        private readonly HashSet<string> _cannotThunder = new HashSet<string>(StringComparer.Ordinal);
        private ENATTR _attributes;
        private BattleEnemySource _source;
        private int _unlockBudget;
        private readonly Stack<Branch> _branches = new Stack<Branch>();
        private Branch _closedBranch;
        private static readonly Regex Variable = new Regex(@"\$(?:\{\s*(\w+)\s*\}|(\w+))", RegexOptions.CultureInvariant);
        internal static readonly Regex ValueToken = new Regex(@"[A-Za-z_]\w*(?:\[[\w.\-]+\])?", RegexOptions.CultureInvariant);
        private const string Number = @"[-+]?(?:\d+(?:\.\d+)?|\.\d+)";
        private static readonly Regex Curve = new Regex(@"^(?<fn>Z1|ZLINE)?(?:<(?<start>" + Number + @")?\.\.(?<end>" + Number + @")?>)?(?:\*(?<scale>" + Number + @"))?$", RegexOptions.CultureInvariant);

        private sealed class Branch
        {
            internal bool Parent;
            internal bool Active;
            internal bool Taken;
        }

        internal BattleEnemyPreviewScript(BattleEnemyPreviewContext context)
        {
            _context = context;
            _variables = new Dictionary<string, string>(context.Variables, StringComparer.Ordinal);
            _unlockBudget = context.AdditionalCount;
        }

        private bool Active => _branches.Count == 0 || _branches.Peek().Active;

        internal bool TryRead(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                UndeterminedReason = "Missing battle script.";
                return false;
            }
            try
            {
                foreach (var tokens in Lines(script)) Read(tokens);
                if (_branches.Count != 0) throw new FormatException("Unclosed branch.");
                return true;
            }
            catch (FormatException exception)
            {
                UndeterminedReason = exception.Message;
                return false;
            }
        }

        private void Read(List<string> tokens)
        {
            while (tokens.Count > 0 && tokens[0] == "}")
            {
                if (_branches.Count == 0) throw new FormatException("Unbalanced branch.");
                _closedBranch = _branches.Pop();
                tokens.RemoveAt(0);
            }
            if (tokens.Count == 0) return;
            string command = tokens[0];
            if (command == "ELSE" || command.StartsWith("ELSIF", StringComparison.Ordinal))
            {
                var branch = _closedBranch;
                if (branch == null || tokens[tokens.Count - 1] != "{") throw new FormatException("Unsupported branch.");
                branch.Active = branch.Parent && !branch.Taken && (command == "ELSE" || Condition(command.Replace("ELSIF", "IF"), tokens));
                branch.Taken |= branch.Active;
                _branches.Push(branch);
                _closedBranch = null;
                return;
            }
            _closedBranch = null;
            if (command.StartsWith("IF", StringComparison.Ordinal))
            {
                if (tokens[tokens.Count - 1] != "{") throw new FormatException("Unsupported inline branch.");
                bool parent = Active;
                bool active = parent && Condition(command, tokens);
                _branches.Push(new Branch { Parent = parent, Active = active, Taken = active });
                return;
            }
            if (!Active) return;
            if (command == "{") throw new FormatException("Unexpected block.");
            if (command.StartsWith("#", StringComparison.Ordinal)) return;
            if (command == "DEFINE" || command == "DEFINENDEF" || Regex.IsMatch(command, @"^\w+=", RegexOptions.CultureInvariant))
            {
                string assignment = string.Join("", tokens.Skip(command.StartsWith("DEFINE", StringComparison.Ordinal) ? 1 : 0));
                int equal = assignment.IndexOf('=');
                if (equal < 1) throw new FormatException("Invalid variable assignment.");
                string key = assignment.Substring(0, equal);
                if (command != "DEFINENDEF" || !_variables.ContainsKey(key))
                    _variables[key] = Expand(assignment.Substring(equal + 1));
                return;
            }
            switch (command)
            {
                case "%ENATTR":
                    _attributes = ENATTR.NORMAL;
                    foreach (string raw in tokens.Skip(1))
                    {
                        string attribute = Expand(raw);
                        if (attribute.StartsWith("!", StringComparison.Ordinal))
                        {
                            _attributes |= ENATTR._FIXED;
                            attribute = attribute.Substring(1);
                        }
                        if (attribute.Length == 0) continue;
                        ENATTR parsed;
                        if (!Enum.TryParse(attribute, out parsed)) throw new FormatException("Unknown attribute.");
                        _attributes |= parsed;
                    }
                    return;
                case "%EN_HR":
                    _source = Get(tokens, 1).StartsWith("_FOLLOW_", StringComparison.Ordinal) ? BattleEnemySource.Follower : BattleEnemySource.Main;
                    return;
                case "%CANNOT_THUNDER_TO":
                    foreach (string key in tokens.Skip(1)) _cannotThunder.Add(Expand(key).ToUpperInvariant());
                    return;
                case "%ADD_COUNT":
                    string addition = Expand(string.Join("", tokens.Skip(1)));
                    if (addition.StartsWith("*=", StringComparison.Ordinal))
                        _unlockBudget = Round(_unlockBudget * Numeric(addition.Substring(2)));
                    else
                    {
                        if (addition.StartsWith("!", StringComparison.Ordinal))
                        {
                            _unlockBudget = 0;
                            addition = addition.Substring(1);
                        }
                        _unlockBudget += (int)Quantity(addition, "0");
                    }
                    // 随机增补循环使用最初的局部变量 _count_add。
                    // CI.count_add 仅控制数量为零或负数的脚本条目能否解锁。
                    return;
                case "%MAX_CNT":
                    string capKey = tokens.Count == 3 ? Get(tokens, 1).ToUpperInvariant() : null;
                    if (_context.QuestEnemy.HasValue && capKey == _context.QuestEnemy.Value.ToString()) return;
                    string capText = Get(tokens, capKey == null ? 1 : 2);
                    bool fixedCap = capText.StartsWith("!", StringComparison.Ordinal);
                    int cap = Round(Quantity(fixedCap ? capText.Substring(1) : capText, "99") + (fixedCap ? 0 : _context.CountCapAddition));
                    if (cap < 0) throw new FormatException("Negative count cap.");
                    if (capKey == null) TotalCap = cap;
                    else
                    {
                        Caps.RemoveAll(value => value.Key == capKey);
                        Caps.Add(new BattleEnemyPreview.CountCap { Key = capKey, Maximum = cap });
                    }
                    return;
                case "%EN":
                case "%EN_OD":
                    if ((int)Numeric(Get(tokens, 5), 1) == 0) return;
                    string countText = Get(tokens, 2);
                    bool fixedCount = countText.StartsWith("!", StringComparison.Ordinal);
                    int count = Round(Quantity(fixedCount ? countText.Substring(1) : countText, "1"));
                    if (count <= 0 && _unlockBudget > 0 && !fixedCount
                        && (int)Math.Max(_context.Night ? 1f : 0f, _context.DangerLevel + 0.375f) + count > 0)
                    {
                        _unlockBudget--;
                        count = 1;
                    }
                    if (count <= 0) return;
                    var entry = CreateEntry(Get(tokens, 1).ToUpperInvariant(), command == "%EN_OD", _attributes, _source, _context);
                    if (!entry.EnemyId.HasValue) throw new FormatException("Unknown enemy ID.");
                    if (!entry.Overdrive && _cannotThunder.Contains(entry.EnemyKey)) entry.Overdriveable = false;
                    Spawns.Add(new BattleEnemyPreview.Spawn
                    {
                        Entry = entry,
                        Count = count,
                        Weight = fixedCount ? 0 : Math.Max(0, Quantity(Get(tokens, 6), "1"))
                    });
                    return;
                case "%NEXT_SCRIPT":
                case "%SPEVENT":
                    Incomplete = DynamicReinforcements = true;
                    UndeterminedReason = "Additional script or special event.";
                    return;
                case "SEEK_END":
                case "GOTO":
                case "SEEK_SET":
                    throw new FormatException("Unsupported script control flow.");
                default:
                    if (command.StartsWith("%%", StringComparison.Ordinal))
                    {
                        Incomplete = DynamicReinforcements = true;
                        UndeterminedReason = "Special battle script.";
                    }
                    else if (!command.StartsWith("%", StringComparison.Ordinal) && command != "/*" && command != "/*___")
                        throw new FormatException("Unsupported statement.");
                    return;
            }
        }

        private bool Condition(string command, List<string> tokens)
        {
            string value = Get(tokens, 1);
            if (command == "IFDEF") return _variables.ContainsKey(value);
            if (command == "IFNDEF") return !_variables.ContainsKey(value);
            if (command == "IF" || command == "IF2") return Numeric(value) != 0;
            if (command == "IFSTR")
            {
                string right = Get(tokens, 3);
                switch (Get(tokens, 2))
                {
                    case "==": return value == right;
                    case "!=": return value != right;
                }
            }
            throw new FormatException("Unknown condition.");
        }

        private string Get(List<string> tokens, int index) => index < tokens.Count ? Expand(tokens[index]) : "";

        private string Expand(string text)
        {
            for (int depth = 0; depth < 8 && text.IndexOf('$') >= 0; depth++)
            {
                text = Variable.Replace(text, match =>
                {
                    string key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    string value;
                    if (!_variables.TryGetValue(key, out value) || value == null) throw new FormatException("Unknown variable.");
                    return value;
                });
            }
            if (text.IndexOf('$') >= 0) throw new FormatException("Recursive variable.");
            if (text.StartsWith("~", StringComparison.Ordinal)) return Numeric(text.Substring(1)).ToString("R", CultureInfo.InvariantCulture);
            return text;
        }

        private double Numeric(string text, double empty = 0)
        {
            if (string.IsNullOrEmpty(text)) return empty;
            text = Expand(text);
            text = ValueToken.Replace(text, match =>
            {
                if (string.Equals(match.Value, "true", StringComparison.OrdinalIgnoreCase)) return "1";
                if (string.Equals(match.Value, "false", StringComparison.OrdinalIgnoreCase)) return "0";
                double value;
                if (!_context.Values.TryGetValue(match.Value, out value) || double.IsNaN(value))
                    throw new FormatException("Unknown expression.");
                return "(" + value.ToString("0.################", CultureInfo.InvariantCulture) + ")";
            });
            if (!Regex.IsMatch(text, @"^[\d\s.()+*/%<>=!&|~\-]+$", RegexOptions.CultureInvariant))
                throw new FormatException("Unsupported numeric expression.");
            int nesting = 0;
            foreach (char c in text)
            {
                if (c == '(') nesting++;
                if (c == ')' && --nesting < 0) throw new FormatException("Invalid parentheses.");
            }
            if (nesting != 0) throw new FormatException("Invalid parentheses.");
            double result = new NumericExpression(text).Evaluate();
            if (double.IsNaN(result) || double.IsInfinity(result) || Math.Abs(result) > 100000)
                throw new FormatException("Invalid numeric result.");
            return result;
        }

        private float Quantity(string text, string fallback)
        {
            text = string.IsNullOrEmpty(text) ? fallback : Expand(text);
            float result = 0;
            foreach (string part in text.Split('|'))
            {
                float number;
                if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    result += _context.DangerLevel >= 0 ? number : 0;
                    continue;
                }
                var match = Curve.Match(part);
                if (!match.Success || (!match.Groups["fn"].Success && part.IndexOf('<') < 0)) throw new FormatException("Unsupported quantity curve.");
                float start = match.Groups["start"].Success ? float.Parse(match.Groups["start"].Value, CultureInfo.InvariantCulture) : 0f;
                float end = match.Groups["end"].Success ? float.Parse(match.Groups["end"].Value, CultureInfo.InvariantCulture) : part.IndexOf('<') >= 0 ? 5f : 1f;
                float scale = match.Groups["scale"].Success ? float.Parse(match.Groups["scale"].Value, CultureInfo.InvariantCulture) : 1f;
                if (end <= start) throw new FormatException("Invalid curve interval.");
                float ratio = (_context.DangerLevel - start) / (end - start);
                result += (match.Groups["fn"].Value == "Z1" ? (ratio >= 0 ? 1f : 0f) : Math.Max(0f, Math.Min(1f, ratio))) * scale;
            }
            if (float.IsNaN(result) || float.IsInfinity(result) || Math.Abs(result) > 10000)
                throw new FormatException("Invalid quantity.");
            return result;
        }

        private static int Round(double value) => (int)Math.Round((float)value, MidpointRounding.ToEven);

        // 此处只计算数值，不解析标识符或调用函数。
        // 原版按从左到右的顺序计算连续比较，因此比较运算符采用相同优先级。
        private sealed class NumericExpression
        {
            private readonly string _text;
            private int _offset;
            internal NumericExpression(string text) { _text = text; }
            internal double Evaluate()
            {
                double value = Read(0);
                Skip();
                if (_offset != _text.Length) throw new FormatException("Invalid numeric expression.");
                return value;
            }

            private double Read(int precedence)
            {
                if (precedence == 6) return Atom();
                double left = Read(precedence + 1);
                while (true)
                {
                    Skip();
                    string op = Operator();
                    if (Priority(op) != precedence) return left;
                    _offset += op.Length;
                    double right = Read(precedence == 0 ? 0 : precedence + 1);
                    switch (op)
                    {
                        case "||": return left != 0 || right != 0 ? 1 : 0;
                        case "&&": return left != 0 && right != 0 ? 1 : 0;
                        case "==": left = left == right ? 1 : 0; break;
                        case "!=": left = left != right ? 1 : 0; break;
                        case ">=": left = left >= right ? 1 : 0; break;
                        case "<=": left = left <= right ? 1 : 0; break;
                        case ">": left = left > right ? 1 : 0; break;
                        case "<": left = left < right ? 1 : 0; break;
                        case "+": left += right; break;
                        case "-": left -= right; break;
                        case "*": left *= right; break;
                        case "/": if (right == 0) throw new FormatException("Division by zero."); left /= right; break;
                        case "%": if (right == 0) throw new FormatException("Division by zero."); left %= right; break;
                        case "&": left = (int)left & (int)right; break;
                        case "|": left = (int)left | (int)right; break;
                    }
                }
            }

            private double Atom()
            {
                Skip();
                if (_offset == _text.Length) throw new FormatException("Missing operand.");
                char c = _text[_offset];
                if (c == '+' || c == '-' || c == '!' || c == '~')
                {
                    _offset++;
                    double value = Atom();
                    return c == '-' ? -value : c == '!' ? (value == 0 ? 1 : 0) : c == '~' ? ~(int)value : value;
                }
                if (c == '(')
                {
                    _offset++;
                    double value = Read(0);
                    Skip();
                    if (_offset == _text.Length || _text[_offset++] != ')') throw new FormatException("Missing parenthesis.");
                    return value;
                }
                int start = _offset;
                while (_offset < _text.Length && (char.IsDigit(_text[_offset]) || _text[_offset] == '.')) _offset++;
                double number;
                if (!double.TryParse(_text.Substring(start, _offset - start), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number))
                    throw new FormatException("Invalid number.");
                return number;
            }

            private string Operator()
            {
                if (_offset + 1 < _text.Length)
                {
                    string pair = _text.Substring(_offset, 2);
                    if (pair == "||" || pair == "&&" || pair == "==" || pair == "!=" || pair == ">=" || pair == "<=") return pair;
                }
                return _offset < _text.Length ? _text[_offset].ToString() : "";
            }
            private static int Priority(string op)
            {
                switch (op)
                {
                    case "||": case "&&": return 0;
                    case "==": case "!=": case "<": case ">": case "<=": case ">=": return 2;
                    case "+": case "-": return 3;
                    // 原版 TX 中，位运算符与乘法的优先级相同。
                    case "*": case "/": case "%": case "&": case "|": return 4;
                    default: return -1;
                }
            }
            private void Skip() { while (_offset < _text.Length && char.IsWhiteSpace(_text[_offset])) _offset++; }
        }

        internal static BattleEnemyPreviewEntry CreateEntry(string key, bool od, ENATTR attributes, BattleEnemySource source, BattleEnemyPreviewContext context, bool replaceQuest = true)
        {
            ENEMYID id;
            bool known = Enum.TryParse(key, out id) && Enum.IsDefined(typeof(ENEMYID), id);
            NDAT.EnemyDescryption info = known ? context.GetEnemyDescription(key) : default(NDAT.EnemyDescryption);
            known &= info.valid;
            if (known && replaceQuest && !info.no_replacable_by_quest && context.QuestEnemy.HasValue)
            {
                var replacement = context.GetEnemyDescription(context.QuestEnemy.Value.ToString());
                if (replacement.valid && (!od || replacement.overdriveable))
                {
                    id = context.QuestEnemy.Value;
                    key = id.ToString();
                    info = replacement;
                }
            }
            return new BattleEnemyPreviewEntry
            {
                EnemyKey = key,
                EnemyId = known ? (ENEMYID?)id : null,
                Overdrive = od,
                Attributes = attributes & ~ENATTR.__OPTIONAL & ~info.nattr_decline,
                FixedAttributes = (attributes & ENATTR._FIXED) != 0,
                Source = source,
                Overdriveable = info.overdriveable,
                DeclinedAttributes = info.nattr_decline,
                AttributeThreshold = info.nattr_addable == 0 ? 16 : info.nattr_addable,
                OverdriveAttributeThreshold = info.nattr_addable_od == 0 ? 32 : info.nattr_addable_od
            };
        }

        internal BattleEnemyPreviewSnapshot Candidates(string script)
        {
            var result = new BattleEnemyPreviewSnapshot
            {
                Incomplete = true, Maximum = null, DynamicReinforcements = _context.SpecialBattle,
                UndeterminedReason = UndeterminedReason
            };
            foreach (var tokens in Lines(script ?? ""))
            {
                if (tokens.Count < 2) continue;
                if (tokens[0] == "%NEXT_SCRIPT" || tokens[0] == "%SPEVENT" || tokens[0] == "%EN_HR") result.DynamicReinforcements = true;
                if (tokens[0] != "%EN" && tokens[0] != "%EN_OD") continue;
                string key;
                try { key = Expand(tokens[1]).ToUpperInvariant(); }
                catch (FormatException) { key = tokens[1]; }
                var entry = CreateEntry(key, tokens[0] == "%EN_OD", ENATTR.NORMAL, BattleEnemySource.Main, _context);
                entry.Maximum = null;
                entry.UndeterminedReason = UndeterminedReason;
                result.Entries.Add(entry);
                result.DynamicReinforcements |= BattleEnemyPreview.CanSummon(entry.EnemyId);
            }
            MergeCandidates(result.Entries);
            return result;
        }

        private static void MergeCandidates(List<BattleEnemyPreviewEntry> entries)
        {
            BattleEnemyPreview.Merge(entries);
        }

        private static IEnumerable<List<string>> Lines(string script)
        {
            // readCorrectly 只逐行读取，不展开变量、不计算波浪号表达式，也不执行事件。
            var reader = new CsvReader(script, CsvReader.RegSpace, false);
            while (reader.readCorrectly())
            {
                var tokens = Tokenize(reader.getLastStr());
                if (tokens.Count > 0) yield return tokens;
            }
        }

        internal static List<string> Tokenize(string line)
        {
            var result = new List<string>();
            var token = new StringBuilder();
            char quote = '\0';
            bool started = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (quote != '\0')
                {
                    if (c == quote) quote = '\0'; else token.Append(c);
                    continue;
                }
                if (c == '/' && i + 1 < line.Length && line[i + 1] == '/') break;
                if (c == '\'' || c == '"') { quote = c; started = true; continue; }
                if (char.IsWhiteSpace(c) || c == ',')
                {
                    if (started) result.Add(token.ToString());
                    token.Clear();
                    started = false;
                }
                else { token.Append(c); started = true; }
            }
            if (quote != '\0') throw new FormatException("Unclosed quote.");
            if (started) result.Add(token.ToString());
            return result;
        }
    }
}
