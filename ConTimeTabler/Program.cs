using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

class Program
{
    static void Main()
    {
        var workbook = new XLWorkbook("Data/DataBase.xlsx");
        var worksheet = workbook.Worksheet(1);

        var name = worksheet.Cell(2, 2).GetString();
        var age = worksheet.Cell(2, 3).GetString();

        Console.WriteLine($"이름: {name}, 나이: {age}");
    }
}