using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Minerva.Services;

/// <summary>
/// Text query assistant.
/// </summary>
public class ItaTextQueryAssistant : ITextQueryAssistant
{
    private static readonly Regex _wsRegex = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex _termRegex = new(
        @"(?:trova\s)?(?:(?:la|le|il|i|un|una|delle|dei)\s)?" +
        // obj: parola/e, locuzione/i, lemma/i
        @"(?<obj>parola|parole|locuzione|locuzioni|lemma|lemmi)" +
        // op: uguale/i a, iniziante/i per, terminante/i per, contenente/i, jolly,
        // (molto|poco) simile/i a
        @"(?:\s(?<op>ugual[ei] a|iniziant[ei] (per|con|in)|" +
        @"terminant[ei] (per|con|in)|" +
        @"contenent[ei]|corrispondente a|(molto |poco )?simil[ei] a))?" +
        // val: valore
        @"\s""(?<val>[^""]+)""" +
        // POS
        @"(?:\scome\s(?<pos>aggettivo|apposizione|avverbio|ausiliare|" +
        @"congiunzione coordinante|determinante|sostantivo|numero|pronome|" +
        @"nome proprio|congiunzione subordinante|simbolo|verbo|altro))?" +
        // style: grassetto, corsivo, sottolineato
        @"(?:\sin\s(?<style>grassetto|corsivo|sottolineato))?" +
        // not: non,
        // tail: a inizio (di) frase, a fine (di) frase, vicino/a/i a/al/alla/ai,
        // attaccato/a/i a/al/alla/ai, prima di/del/della/dei, dopo di/del/della/dei/la/il
        @"(?:\s(?<not>non\s)?(?<tail>a inizio (?:di )?frase|a fine (?:di )?frase|" +
        @"vicin[oai] (?:a|al|alla|ai)|attaccat[oai] (?:a|al|alla|ai)|" +
        @"prima (?:di|del|della|dei)|dopo (?:di|del|della|dei|la|il)))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private float[] _simThresholds = [0.5f, 0.8f, 0.9f];

    /// <summary>
    /// Gets or sets the similarity thresholds. These are 3 values for
    /// the low, normal, and high similarity levels.
    /// </summary>
    /// <exception cref="ArgumentNullException">null value</exception>
    /// <exception cref="ArgumentException">invalid value</exception>
    public IList<float> SimilarityThresholds
    {
        get => _simThresholds;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value.Count != 3)
            {
                throw new ArgumentException(
                    "The similarity thresholds must be 3 values (low, mid, high).");
            }
            _simThresholds = value.ToArray();
        }
    }

    /// <summary>
    /// Gets or sets the POS (part of speech) map, a dictionary where key
    /// is the user friendly name for a POS and value is the real POS value.
    /// </summary>
    public IDictionary<string, string> PosMap { get; } =
        new Dictionary<string,string>()
    {
        ["aggettivo"] = "ADJ",
        ["apposizione"] = "ADP",
        ["avverbio"] = "ADV",
        ["ausiliare"] = "AUX",
        ["congiunzione coordinante"] = "CCONJ",
        ["determinante"] = "DET",
        ["sostantivo"] = "NOUN",
        ["numero"] = "NUM",
        ["pronome"] = "PRON",
        ["nome proprio"] = "PROPN",
        ["congiunzione subordinante"] = "SCONJ",
        ["simbolo"] = "SYM",
        ["verbo"] = "VERB",
        ["altro"] = "X",
    };

    public string GetHelp()
    {
        return "1. parola, locuzione, lemma\n" +
            "[2. uguale a, iniziante per, terminante per, contenente, " +
            "molto/poco simile a]\n" +
            "3. \"...\"\n" +
            "[4. come sostantivo, aggettivo, verbo]\n" +
            "[5. in grassetto, corsivo, sottolineato]\n" +
            "[6. (non) a inizio di frase, a fine di frase, vicino a, " +
            "attaccato a, prima di, dopo di]\n";
    }

    private static string NormalizeWS(string value)
        => _wsRegex.Replace(value, " ").Trim();

    private (string op, string suffix) GetOperator(Match m)
    {
        return m.Groups["op"].Value.ToLower() switch
        {
            "iniziante per" => ("^=", ""),
            "terminante per" => ("$=", ""),
            "contenente" => ("*=", ""),
            "corrispondente a" => ("?=", ""),
            "simile a" => ("%=",
                $":{_simThresholds[1].ToString(CultureInfo.InvariantCulture)}"),
            "molto simile a" => ("%=",
                $":{_simThresholds[2].ToString(CultureInfo.InvariantCulture)}"),
            "poco simile a" => ("%=",
                $":{_simThresholds[0].ToString(CultureInfo.InvariantCulture)}"),
            _ => ("=", ""),
        };
    }

    /// <summary>
    /// Converts to query.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The query or null.</returns>
    /// <exception cref="ArgumentException">Unknown POS: {pos}</exception>
    public string? ToQuery(string text)
    {
        if (string.IsNullOrEmpty(text)) return null;
        text = NormalizeWS(text);

        StringBuilder query = new();

        foreach (Match m in _termRegex.Matches(text))
        {
            // operator and suffix
            (string op, string suffix) = GetOperator(m);

            // object
            switch (m.Groups["obj"].Value.ToLower())
            {
                case "parola":
                case "parole":
                    query.Append("[value")
                         .Append(op)
                         .Append('"')
                         .Append(m.Groups["val"].Value)
                         .Append(suffix)
                         .Append("\"]");
                    break;
                case "locuzione":
                case "locuzioni":
                    query.Append("[fp-lat")
                         .Append(op)
                         .Append('"')
                         .Append(m.Groups["val"].Value)
                         .Append(suffix)
                         .Append("\"]");
                    break;
                case "lemma":
                case "lemmi":
                    query.Append("[lemma=")
                         .Append(op)
                         .Append('"')
                         .Append(m.Groups["val"].Value)
                         .Append(suffix)
                         .Append("\"]");
                    break;
            }

            // pos
            if (!string.IsNullOrEmpty(m.Groups["pos"].Value))
            {
                string pos = m.Groups["pos"].Value.ToLower();
                if (!PosMap.TryGetValue(pos, out string? posValue))
                {
                    throw new ArgumentException($"Unknown POS: {pos}");
                }
                query.Append(" AND [pos=\"")
                     .Append(posValue)
                     .Append("\"]");
            }

            // style
            if (!string.IsNullOrEmpty(m.Groups["style"].Value))
            {
                switch (m.Groups["style"].Value.ToLower())
                {
                    case "grassetto":
                        query.Append(" AND [b=\"1\"]");
                        break;
                    case "corsivo":
                        query.Append(" AND [i=\"1\"]");
                        break;
                    case "sottolineato":
                        query.Append(" AND [u=\"1\"]");
                        break;
                }
            }

            // not
            bool hasNot = !string.IsNullOrEmpty(m.Groups["not"].Value);

            // tail
            string tail = m.Groups["tail"].Value.ToLower();
            switch (tail)
            {
                case "a inizio frase":
                case "a inizio di frase":
                    query.Append(hasNot
                        ? " LALIGN(n=1) [$name=\"snt\"]"
                        : " LALIGN(m=0) [$name=\"snt\"]");
                    break;
                case "a fine frase":
                case "a fine di frase":
                    query.Append(hasNot
                        ? " RALIGN(n=1) [$name=\"snt\"]"
                        : " RALIGN(m=0) [$name=\"snt\"]");
                    break;
                case "prima di":
                case "prima del":
                case "prima della":
                    query.Append(" BEFORE(m=3,s=snt) ");
                    break;
                case "dopo di":
                case "dopo del":
                case "dopo della":
                case "dopo dei":
                case "dopo la":
                case "dopo il":
                    query.Append(" AFTER(m=3,s=snt) ");
                    break;
                default:
                    if (tail.StartsWith("vicin", StringComparison.Ordinal))
                    {
                        query.Append(" NEAR(m=3,s=snt) ");
                        break;
                    }
                    if (tail.StartsWith("attaccat", StringComparison.Ordinal))
                    {
                        query.Append(" NEAR(m=0,s=snt) ");
                        break;
                    }
                    break;
            }
        }

        return query.Length == 0? null : query.ToString();
    }
}
