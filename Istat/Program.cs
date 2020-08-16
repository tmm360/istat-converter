using CsvHelper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Istat
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = args[0];
            var directory = Path.GetDirectoryName(inputFile);

            using var reader = new StreamReader(inputFile);
            using var csvInput = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvInput.GetRecords<IstatCsvInput>();

            var output = records.SelectMany(r =>
                {
                    var day = int.Parse(r.GE.Substring(2));
                    var month = int.Parse(r.GE.Remove(2));

                    var records = new[]
                    {
                        (year: 2015, deaths: r.T_15),
                        (year: 2016, deaths: r.T_16),
                        (year: 2017, deaths: r.T_17),
                        (year: 2018, deaths: r.T_18),
                        (year: 2019, deaths: r.T_19),
                        (year: 2020, deaths: r.T_20)
                    }
                    .Where(pair =>
                    {
                        try { new DateTime(pair.year, month, day); }
                        catch (ArgumentOutOfRangeException) { return false; }
                        return true;
                    })
                    .Select(pair =>
                    {
                        var date = new DateTime(pair.year, month, day);
                        int.TryParse(pair.deaths, out var deaths);

                        return new IstatRecord
                        {
                            AgeClass = int.Parse(r.CL_ETA),
                            CityName = r.NOME_COMUNE,
                            Date = date,
                            Deaths = deaths,
                            ProvinceName = r.NOME_PROVINCIA,
                            RegionName = r.NOME_REGIONE,
                        };
                    });

                    return records;
                })
                .GroupBy(r => new { r.RegionName, r.Date, r.AgeClass })
                .Select(g =>
                {
                    return new IstatCsvOutput
                    {
                        AgeClass = g.Key.AgeClass,
                        Date = g.Key.Date.ToShortDateString(),
                        RegionName = g.Key.RegionName,
                        TotalDeaths = g.Sum(r => r.Deaths)
                    };
                })
                .Where(r => r.TotalDeaths != 0)
                .OrderBy(r => r.AgeClass)
                .OrderBy(r => r.Date);

            // Ouput
            using var writer = new StreamWriter(Path.Combine(directory, "output.csv"));
            using var csvOutput = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvOutput.WriteRecords(output);

            writer.Flush();
        }
    }
}
