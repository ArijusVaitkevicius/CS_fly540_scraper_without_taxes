using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;

var scraper = new Fly540Scraper();

List<DateTime> firstDates = new List<DateTime>() { DateTime.Today.AddDays(10), DateTime.Today.AddDays(17) };
List<DateTime> secondDates = new List<DateTime>() { DateTime.Today.AddDays(20), DateTime.Today.AddDays(27) };
List<List<DateTime>> dates = new List<List<DateTime>>() { firstDates, secondDates };
string departureIATA = "NBO";
string arrivalIATA = "MBA";
string currency = "USD";

List<string> URLS = scraper.Urls(dates, departureIATA, arrivalIATA, currency);
scraper.FlightsScraper(URLS);

class Fly540Scraper
{
    public List<string> Urls(List<List<DateTime>> dates, string depIATA, string arrIATA, string cur)
    {
        List<string> URLS = new List<string>();

        //This loop generates URL by list variables.
        foreach (var date in dates)
        {
            string URL = $"https://www.fly540.com/flights/?isoneway=0&currency={cur}&depairportcode={depIATA}&arrvairportcode={arrIATA}&date_from={date[0].ToString("ddd")}%2C+{date[0].ToString("dd")}+{date[0].ToString("MMM")}+{date[0].ToString("yyy")}&date_to={date[1].ToString("ddd")}%2C+{date[1].ToString("dd")}+{date[1].ToString("MMM")}+{date[1].ToString("yyy")}&adult_no=1&children_no=0&infant_no=0&searchFlight=&change_flight=";
            URLS.Add(URL);
        }

        return URLS;

    }
    public void FlightsScraper(List<string> URLS)
    {
        HtmlWeb web = new HtmlWeb();
        string reIATA = @"[A-Z]{3}";
        string reYear = @"\d{4}";
        var csv = new StringBuilder();
        string csvPATH = "C:/Users/Arijus/source/repos/ConsoleApp1/ConsoleApp1/result.csv"; //Specify the path to csv file

        //This loop iterates through URLS list and gets each URL page source.
        foreach (string url in URLS)
        {
            HtmlDocument document = web.Load(url);
            var outbounds = document.DocumentNode.SelectNodes("//div[@class='fly5-flights fly5-depart th']/div[@class='fly5-results']/div");
            var inbounds = document.DocumentNode.SelectNodes("//div[@class='fly5-flights fly5-return th']/div[@class='fly5-results']/div");

            foreach (var outbound in outbounds)
            {
                foreach (var inbound in inbounds)
                {
                    //Outbound details (outbound departure and arrival airports codes, price, dates and times).
                    var outDepCol = outbound.SelectSingleNode(".//td[@data-title='Departs']");
                    var outArrCol = outbound.SelectSingleNode(".//td[@data-title='Arrives']");


                    string outFrom = outDepCol.SelectSingleNode(".//span[@class='flfrom']").InnerText;
                    string outFromIATA = Regex.Matches(outFrom, reIATA)[0].Groups[0].Value;

                    string outTo = outArrCol.SelectSingleNode(".//span[@class='flfrom']").InnerText;
                    string outToIATA = Regex.Matches(outTo, reIATA)[0].Groups[0].Value;

                    string outDate = document.DocumentNode.SelectSingleNode("//span[contains(text(), 'Departing')]").NextSibling.InnerText;
                    string outYear = Regex.Matches(outDate, reYear)[0].Groups[0].Value;

                    string outDepDate = outDepCol.SelectSingleNode(".//span[@class='fldate']").InnerText.Trim();

                    string outDepTime = outDepCol.SelectSingleNode(".//span[contains(@class, 'fltime')]").InnerText.Trim();

                    string fullOutDepDate = DateTime.ParseExact($"{outYear} {outDepDate} {outDepTime}", "yyyy ddd dd, MMM h:mmtt",
                        null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string outArrDate = outArrCol.SelectSingleNode(".//span[@class='fldate']").InnerText.Trim();

                    string outArrTime = outArrCol.SelectSingleNode(".//span[contains(@class, 'fltime')]").InnerText.Trim();

                    string fullOutArrDate = DateTime.ParseExact($"{outYear} {outArrDate} {outArrTime}", "yyyy ddd dd, MMM h:mmtt",
                        null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string outPrice = outbound.SelectSingleNode(".//td[contains(@data-title, 'Lowest')]/span[@class='flprice']").InnerText;


                    //Inbound details(inbound departure and arrival airports codes, price, dates and times).
                    var inDepCol = inbound.SelectSingleNode(".//td[@data-title='Departs']");
                    var inArrCol = inbound.SelectSingleNode(".//td[@data-title='Arrives']");


                    string inFrom = inDepCol.SelectSingleNode(".//span[@class='flfrom']").InnerText;
                    string inFromIATA = Regex.Matches(inFrom, reIATA)[0].Groups[0].Value;

                    string inTo = inArrCol.SelectSingleNode(".//span[@class='flfrom']").InnerText;
                    string inToIATA = Regex.Matches(inTo, reIATA)[0].Groups[0].Value;

                    string inDate = document.DocumentNode.SelectSingleNode("//span[contains(text(), 'Returning')]").NextSibling.InnerText;
                    string inYear = Regex.Matches(inDate, reYear)[0].Groups[0].Value;

                    string inDepDate = inDepCol.SelectSingleNode(".//span[@class='fldate']").InnerText.Trim();

                    string inDepTime = inDepCol.SelectSingleNode(".//span[contains(@class, 'fltime')]").InnerText.Trim();

                    string fullInDepDate = DateTime.ParseExact($"{inYear} {inDepDate} {inDepTime}", "yyyy ddd dd, MMM h:mmtt",
                        null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string inArrDate = inArrCol.SelectSingleNode(".//span[@class='fldate']").InnerText.Trim();

                    string inArrTime = inArrCol.SelectSingleNode(".//span[contains(@class, 'fltime')]").InnerText.Trim();

                    string fullInArrDate = DateTime.ParseExact($"{inYear} {inArrDate} {inArrTime}", "yyyy ddd dd, MMM h:mmtt",
                        null).ToString("ddd MMM dd HH:mm:ss 'GMT' yyyy");

                    string inPrice = inbound.SelectSingleNode(".//td[contains(@data-title, 'Lowest')]/span[@class='flprice']").InnerText;

                    int fullPrice = Convert.ToInt32(outPrice) + Convert.ToInt32(inPrice);

                    var newLine = $"{outFromIATA};{outToIATA};{fullOutDepDate};{fullOutArrDate};{inFromIATA};{inToIATA};{fullInDepDate};{fullInArrDate};{fullPrice.ToString("F" + 2)}";

                    //Adds line to csv variable.
                    csv.AppendLine(newLine);
                }
            }
        }


        if (!File.Exists(csvPATH))
        {
            string header = $"outbound_departure_airport;outbound_arrival_airport;outbound_departure_time;outbound_arrival_time;inbound_departure_airport;inbound_arrival_airport;inbound_departure_time;inbound_arrival_time;total_price{Environment.NewLine}";
            File.WriteAllText(csvPATH, header);
        }

        File.AppendAllText(csvPATH, csv.ToString());
    }
}
