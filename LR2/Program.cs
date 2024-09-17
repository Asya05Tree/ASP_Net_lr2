using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Configuration
    .AddXmlFile("Config/companies-xml.xml", optional: false, reloadOnChange: true)
    .AddJsonFile("Config/companies-json.json", optional: false, reloadOnChange: true)
    .AddIniFile("Config/companies-ini.ini", optional: false, reloadOnChange: true)
    .AddJsonFile("PersonalInfo/personal_info.json", optional: false, reloadOnChange: true);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Personal}/{action=Index}/{id?}");

app.MapGet("/", (IConfiguration configuration) => {
    var companies = new List<CompanyInfo>();
    var fileFormats = new Dictionary<string, string>
    {
        {"Microsoft", "XML"},
        {"Apple", "JSON"},
        {"Google", "INI"}
    };

    foreach (var company in new[] { "Microsoft", "Apple", "Google" })
    {
        if (int.TryParse(configuration.GetSection(company)["Employees"], out int employees))
        {
            companies.Add(new CompanyInfo(company, employees, fileFormats[company]));
        }
    }

    var companiesWithMoreThan2Employees = companies.Where(c => c.Employees > 2).ToList();

    var personalInfo = new PersonalInfo(
        configuration["Name"],
        configuration["Surname"],
        configuration["MiddleName"],
        int.Parse(configuration["Age"]),
        configuration["City"],
        configuration["University"]
    );

    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = true
    };

    var result = new
    {
        Companies = companiesWithMoreThan2Employees,
        PersonalInfo = personalInfo
    };

    return Results.Text(JsonSerializer.Serialize(result, options), "application/json");
});

app.Run();

public class CompanyInfo
{
    public string Name { get; set; }
    public int Employees { get; set; }
    public string FileFormat { get; set; }
    public CompanyInfo(string name, int employees, string fileFormat)
    {
        Name = name;
        Employees = employees;
        FileFormat = fileFormat;
    }
}

public class PersonalInfo
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string MiddleName { get; set; }
    public int Age { get; set; }
    public string City { get; set; }
    public string University { get; set; }

    public PersonalInfo(string name, string surname, string middleName, int age, string city, string university)
    {
        Name = name;
        Surname = surname;
        MiddleName = middleName;
        Age = age;
        City = city;
        University = university;
    }

    public override string ToString()
    {
        return $"{Name} {Surname} {MiddleName} ({Age}, {City}), навчаюся в {University}.";
    }
}