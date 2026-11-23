// <copyright file="LipeXsdValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Xml;
using System.Xml.Schema;

namespace FatturaFacile.Application.Taxation;

/// <summary>
/// Validates a LIPE file against the structural XSD profile of the communication (element names, order, cardinality and the
/// amount format). It is not the official schema of the Agenzia delle Entrate: the file must still pass the official control
/// software before it is sent.
/// </summary>
public static class LipeXsdValidator
{
    private const string ResourceName = "FatturaFacile.Application.Taxation.lipe-ivp18-profile.xsd";
    private const string XsdPrefix = "xsd:";

    /// <summary>
    /// Validates a LIPE file.
    /// </summary>
    /// <param name="xml">The XML bytes.</param>
    /// <exception cref="InvalidOperationException">When the schema profile resource is missing from the assembly.</exception>
    /// <returns>The errors, each starting with "xsd:"; empty when the file is valid.</returns>
    public static IReadOnlyList<string> Validate(ReadOnlySpan<byte> xml)
    {
        XmlSchemaSet schemas = new() { XmlResolver = null };
        using (var resource = typeof(LipeXsdValidator).Assembly.GetManifestResourceStream(ResourceName) ?? throw new InvalidOperationException("The LIPE schema profile is missing."))
        using (var schemaReader = XmlReader.Create(resource, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
        {
            _ = schemas.Add(targetNamespace: null, schemaReader);
        }

        List<string> errors = [];
        XmlReaderSettings settings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
        };
        settings.ValidationEventHandler += (_, e) => errors.Add(XsdPrefix + e.Message);

        try
        {
            using var stream = new MemoryStream(xml.ToArray());
            using var reader = XmlReader.Create(stream, settings);
            while (reader.Read())
            {
                // Reading validates the document as a side effect.
            }
        }
        catch (XmlException ex)
        {
            errors.Add(XsdPrefix + ex.Message);
        }

        return errors;
    }
}
