using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

if (args.Length != 2)
{
    Console.WriteLine("""
                      n26csv2mt940.exe {accountnr with currency} {start amount} {end amount} {input file}

                      Example:

                      n26csv2mt940.exe "DE49 1234 5678 9012 3456 78 EUR" 42.00 13.37 "T:\n26-csv-transactions.csv"
                      
                      USE AT YOUR OWN RISK!!
                      """);
    return;
}

var x25 = args[0];
decimal startAmount = decimal.Parse(args[1]);
decimal endAmountValidation = decimal.Parse(args[2]);
var inPath = args[3];
var outPath = inPath + ".mt940";

Console.WriteLine($"In:   {inPath}");
Console.WriteLine($"Out:  {outPath}");
Console.WriteLine($"IBAN: {x25}");

if (File.Exists(outPath))
{
    Console.WriteLine("Out path already exists, aborting");
    return;
}

using var rs = File.OpenRead(inPath);
using var ws = File.OpenWrite(outPath);
using var w = new StreamWriter(ws);
using var reader = new StreamReader(rs);
using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));

void WriteLine(string text)
{
    const int max = 66;

    if (text.Length < max)
    {
        w.WriteLine(text);
    }
    else
    {
        WriteLine(text.Substring(0, max - 1));
        WriteLine(" " + text.Substring(max - 1));
    }
}

//"Date","Payee","Account number","Transaction type","Payment reference","Amount (EUR)","Amount (Foreign Currency)","Type Foreign Currency","Exchange Rate"
csv.Read();
csv.ReadHeader();

WriteLine(":940:");

while (csv.Read())
{
    var x20 = Sha1(csv.Parser.RawRecord).Substring(0, 7);

    var date = DateTime.ParseExact(csv["Date"]!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    var payee = csv["Payee"];
    var accountNr = csv["Account number"];
    var transactionType = csv["Transaction type"];
    var paymentReference = csv["Payment reference"];
    var amountEur = csv["Amount (EUR)"];
    var amountForeign = csv["Amount (Foreign Currency)"];
    var currency = csv["Type Foreign Currency"];
    var exchangeRate = csv["Exchange Rate"];

    var amount = decimal.Parse(amountEur, CultureInfo.InvariantCulture);

    decimal endAmount = startAmount + amount;

    var y = date.Year - 2000;
    var m = date.Month;
    var d = date.Day;

    var yymmdd = $"{y}{m:D2}{d:D2}";

    if (accountNr.Length > 0) accountNr += " ";

    var isCredit = amount > 0;

    WriteLine($":20:940S{x20}");
    WriteLine($":25:{x25}");
    WriteLine($":28C:00000"); //5n[/5n]
    //1!a6!n3!a15d 
    //WL($":60F:{(isCredit ? "C" : "D")}{yymmdd}EUR{startAmount:000000000000.00}"); //000000000444,58
    WriteLine($":60F:C{yymmdd}EUR{startAmount:000000000000.00}"); //000000000444,58
    WriteLine($":61:{yymmdd}{(isCredit ? "C" : "D")}{Math.Abs(amount):000000000000.00}NMSCNONREF"); // NMSC = MIsch
                                                                                             //WL($":86:/BENM//NAME/{payee}/REMI/{transactionType} {amountEur} {amountEur} {amountForeign} {currency} {exchangeRate}"); //{paymentReference} 

    if (paymentReference.Length > 0)
    {
        if (paymentReference.Length > 4)
        {
            WriteLine(paymentReference);
        }

        paymentReference += " ";
    }
    WriteLine($":86:/BENM//NAME/{payee}/REMI/{accountNr}{transactionType} {paymentReference}{amountForeign} {currency} {exchangeRate}"); //{paymentReference} 
    WriteLine($":62F:C{yymmdd}EUR{endAmount:000000000000.00}");

    Console.WriteLine($"{amount,10} {startAmount,10} {endAmount,10}");
    startAmount = endAmount;
}

if (endAmountValidation == startAmount)
{
    Console.WriteLine("End amount matches");
}
else
{
    Console.WriteLine($"End amount mismatch, should be {endAmountValidation} but is {startAmount}");
}

static string Sha1(string input)
{
    using var sha1 = SHA1.Create();
    return Convert.ToHexString(sha1.ComputeHash(Encoding.UTF8.GetBytes(input)));
}