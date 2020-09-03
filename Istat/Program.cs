using CsvHelper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Istat
{
    class Program
    {
        static void Main(string[] args)
        {
            var startedTimestamp = DateTime.Now;

            string inputFile = args[0];
            var directory = Path.GetDirectoryName(inputFile);

            using var reader = new StreamReader(inputFile, CodePagesEncodingProvider.Instance.GetEncoding(1252));
            using var csvInput = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csvInput.GetRecords<IstatCsvInput>();

            var output = records.SelectMany(r =>
                {
                    var day = int.Parse(r.GE.Substring(2));
                    var month = int.Parse(r.GE.Remove(2));

                    var records = new[]
                    {
                        (year: 2015, fDeaths: r.F_15, mDeaths: r.M_15),
                        (year: 2016, fDeaths: r.F_16, mDeaths: r.M_16),
                        (year: 2017, fDeaths: r.F_17, mDeaths: r.M_17),
                        (year: 2018, fDeaths: r.F_18, mDeaths: r.M_18),
                        (year: 2019, fDeaths: r.F_19, mDeaths: r.M_19),
                        (year: 2020, fDeaths: r.F_20, mDeaths: r.M_20)
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
                        int.TryParse(pair.fDeaths, out var fDeaths);
                        int.TryParse(pair.mDeaths, out var mDeaths);

                        return new IstatRecord
                        {
                            AgeClass = int.Parse(r.CL_ETA),
                            Date = date,
                            FDeaths = fDeaths,
                            MDeaths = mDeaths,
                            CityName = r.NOME_COMUNE,
                            ProvinceName = r.NOME_PROVINCIA,
                            RegionName = r.NOME_REGIONE,
                        };
                    });

                    return records;
                })
                .GroupBy(r => new { r.CityName, r.ProvinceName, r.RegionName, r.Date, r.AgeClass })
                .OrderBy(r => r.Key.AgeClass)
                .OrderBy(r => r.Key.CityName)
                .OrderBy(r => r.Key.ProvinceName)
                .OrderBy(r => r.Key.RegionName)
                .OrderBy(r => r.Key.Date)
                .Select(g =>
                {
                    var ageClass = g.Key.AgeClass switch
                    {
                        0 => "0",
                        1 => "1-4",
                        2 => "5-9",
                        3 => "10-14",
                        4 => "15-19",
                        5 => "20-24",
                        6 => "25-29",
                        7 => "30-34",
                        8 => "35-39",
                        9 => "40-44",
                        10 => "45-49",
                        11 => "50-54",
                        12 => "55-59",
                        13 => "60-64",
                        14 => "65-69",
                        15 => "70-74",
                        16 => "75-79",
                        17 => "80-84",
                        18 => "85-89",
                        19 => "90-94",
                        20 => "95-99",
                        21 => "100+",
                        _ => ""
                    };
                    return new IstatCsvOutput
                    {
                        AgeClass = ageClass,
                        Date = g.Key.Date.ToString("yyyy/MM/dd"),
                        RegionName = g.Key.RegionName,
                        ProvinceName = g.Key.ProvinceName,
                        CityName = g.Key.CityName,
                        FDeaths = g.Sum(r => r.FDeaths),
                        MDeaths = g.Sum(r => r.MDeaths)
                    };
                })
                .Where(r => r.TDeaths > 0);

            // Ouput
            using var writer = new StreamWriter(Path.Combine(directory, "output.csv"));
            using var csvOutput = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvOutput.WriteRecords(output);

            writer.Flush();

            Console.WriteLine("Ended in: " + (DateTime.Now - startedTimestamp).TotalSeconds + "s");
        }
    }
}
