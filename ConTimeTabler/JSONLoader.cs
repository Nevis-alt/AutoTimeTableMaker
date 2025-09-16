namespace ConTimeTabler;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

class JsonLoader
{
    static public void Load(List<Course> courses)
    {
        string jsonPath = "courses.json";
        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All), // 모든 유니코드 문자 지원
            PropertyNameCaseInsensitive = true
        };

        // UTF-8 인코딩으로 저장
        string json = JsonSerializer.Serialize(courses, jsonOptions);
        File.WriteAllText(jsonPath, json, Encoding.UTF8);

        Console.WriteLine($"JSON 생성 완료: {jsonPath}");
    }

    static public List<Course> Read()
    {
        string jsonPath = "courses.json";
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"파일을 찾을 수 없습니다: {jsonPath}");
        }

        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNameCaseInsensitive = true
        };

        // UTF-8 인코딩으로 읽기
        string jsonString = File.ReadAllText(jsonPath, Encoding.UTF8);
        return JsonSerializer.Deserialize<List<Course>>(jsonString, jsonOptions) 
            ?? throw new Exception("JSON 파싱 실패");
    }
}