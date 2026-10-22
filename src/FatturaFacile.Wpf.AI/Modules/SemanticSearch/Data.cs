// <copyright file="Data.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Data;

using DevExpress.Xpf.DemoBase.Helpers;

namespace FatturaFacile.SemanticSearch;

/// <summary>
/// Demonstration movie data entity for semantic search tests.
/// </summary>
public class Movie
{
    /// <summary>Gets or sets the unique primary key identifier for the movie.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the release title of the motion picture.</summary>
    public string Title { get; set; }

    /// <summary>Gets or sets the textual narrative synopsis or synopsis outline.</summary>
    public string Plot { get; set; }

    /// <summary>Gets or sets the primary cinematic genre flags or categories.</summary>
    public string Genre { get; set; }

    /// <summary>Gets or sets raw binary image data for promotional cover artwork or posters.</summary>
    public byte[] Photo { get; set; }

    /// <summary>Gets or sets the original theatrical release date.</summary>
    public DateTime Release { get; set; }

    /// <summary>Gets or sets comma-separated names of credited directors.</summary>
    public string Directors { get; set; }

    /// <inheritdoc />
    public override string ToString() => Title;
}

/// <summary>
/// Helper class providing sample movie datasets for semantic search testing.
/// </summary>
public static class SemanticSearchDataHelper
{
    [Flags]
    private enum MovieGenre
    {
        None = 0,
        Action = 0x1,
        Adventure = 0x2,
        Animation = 0x4,
        Biography = 0x8,
        Comedy = 0x10,
        Crime = 0x20,
        Documentary = 0x40,
        Drama = 0x80,
        Family = 0x100,
        Fantasy = 0x200,
        History = 0x400,
        Horror = 0x800,
        Music = 0x1000,
        Musical = 0x2000,
        Mystery = 0x4000,
        Romance = 0x8000,
        SciFi = 0x10000,
        Sport = 0x20000,
        Thriller = 0x40000,
        War = 0x80000,
        Western = 0x100000
    }

    /// <summary>
    /// Gets the cached collection of demonstration movies loaded from sample XML resources.
    /// </summary>
    public static IList<Movie> Movies { get; private set; }

    /// <summary>
    /// Loads demonstration movie datasets and credited director relationships from the embedded VideoRent XML resource.
    /// </summary>
    public static void Load()
    {
        if (Movies != null)
        {
            return;
        }

        var assembly = typeof(SemanticSearchDataHelper).Assembly;
        var stream = assembly.GetManifestResourceStream(
            DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "VideoRent.xml");
        var dataSet = new DataSet();
        _ = dataSet.ReadXml(stream);
        var directorLine = dataSet.Tables["MovieArtistLine"].Rows.Cast<DataRow>()
            .FirstOrDefault(r => (string)r["Name"] == "Director");
        Guid? directorLineId = null;
        if (directorLine != null)
        {
            directorLineId = (Guid?)directorLine["Oid"];
        }

        if (directorLineId == null)
        {
            return;
        }

        var directors =
            from movieArtistRow in dataSet.Tables["MovieArtist"].Rows.Cast<DataRow>()
            where (Guid)movieArtistRow["Line"] == directorLineId
            from personRow in dataSet.Tables["Person"].Rows.Cast<DataRow>()
            where (Guid)personRow["Oid"] == (Guid)movieArtistRow["Artist"]
            select new
            {
                MovieId = (Guid)movieArtistRow["Movie"],
                Name = string.Join(" ", (string)personRow["FirstName"], (string)personRow["LastName"])
            };


        Movies = dataSet.Tables["Movie"].Rows.Cast<DataRow>()
            .Select(row =>
            {
                var movieId = (Guid)row["Oid"];
                return new Movie
                {
                    Id = movieId,
                    Title = row["Title"] as string,
                    Release = (DateTime)row["ReleaseDate"],
                    Genre = ((MovieGenre)row["Genre"]).ToString("G"),
                    Plot = row["Plot"] as string,
                    Photo = row["Photo"] as byte[],
                    Directors = string.Join(", ", directors.Where(d => d.MovieId == movieId).Select(d => d.Name))
                };
            })
            .ToList();
    }
}
