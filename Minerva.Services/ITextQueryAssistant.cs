using System.Collections.Generic;

namespace Minerva.Services;

/// <summary>
/// Text query assistant. This converts some text-based input to a Pythia
/// query.
/// </summary>
public interface ITextQueryAssistant
{
    /// <summary>
    /// Gets or sets the similarity thresholds. This is a 3-element array
    /// for the low, normal, and high similarity levels.
    /// </summary>
    IList<float> SimilarityThresholds { get; set; }

    /// <summary>
    /// Gets or sets the POS (part of speech) map, a dictionary where key
    /// is the user friendly name for a POS and value is the real POS value.
    /// </summary>
    IDictionary<string, string> PosMap { get; }

    /// <summary>
    /// Gets the help text for this assistant.
    /// </summary>
    /// <returns>Text.</returns>
    string GetHelp();

    /// <summary>
    /// Converts the input text to query.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The query or null.</returns>
    string? ToQuery(string text);
}