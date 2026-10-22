using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using DevExpress.Xpf.DemoBase.Helpers;

namespace AIDemo.SemanticSearch {
    public class Movie {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Plot { get; set; }
        public string Genre { get; set; }
        public byte[] Photo { get; set; }
        public DateTime Release { get; set; }
        public string Directors { get; set; }
        public override string ToString() => Title;
    }

    public static class SemanticSearchDataHelper {
        [Flags]
        enum MovieGenre {
            None = 0, Action = 0x1, Adventure = 0x2, Animation = 0x4, Biography = 0x8, Comedy = 0x10, Crime = 0x20,
            Documentary = 0x40, Drama = 0x80, Family = 0x100, Fantasy = 0x200, History = 0x400, Horror = 0x800,
            Music = 0x1000, Musical = 0x2000, Mystery = 0x4000, Romance = 0x8000, SciFi = 0x10000, Sport = 0x20000,
            Thriller = 0x40000, War = 0x80000, Western = 0x100000
        }

        public static IList<Movie> Movies { get; private set; }

        public static void Load() {
            if(Movies != null)
                return;
            var assembly = typeof(SemanticSearchDataHelper).Assembly;
            var stream = assembly.GetManifestResourceStream(
                DemoHelper.GetPath($"{assembly.GetName().Name}.Data.", assembly) + "VideoRent.xml");
            var dataSet = new DataSet();
            dataSet.ReadXml(stream);
            var directorLine = dataSet.Tables["MovieArtistLine"].Rows.Cast<DataRow>()
                .FirstOrDefault(r => (string)r["Name"] == "Director");
            Guid? directorLineId = null;
            if(directorLine != null)
                directorLineId = (Guid?)directorLine["Oid"];
            if(directorLineId == null)
                return;
            var directors =
                from movieArtistRow in dataSet.Tables["MovieArtist"].Rows.Cast<DataRow>()
                where (Guid)movieArtistRow["Line"] == directorLineId
                from personRow in dataSet.Tables["Person"].Rows.Cast<DataRow>()
                where (Guid)personRow["Oid"] == (Guid)movieArtistRow["Artist"]
                select new {
                    MovieId = (Guid)movieArtistRow["Movie"],
                    Name = string.Join(" ", (string)personRow["FirstName"], (string)personRow["LastName"])
                };


            Movies = dataSet.Tables["Movie"].Rows.Cast<DataRow>()
                .Select(row => {
                    var movieId = (Guid)row["Oid"];
                    return new Movie {
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
}
