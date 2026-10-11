using MudBlazor;
using Shared.Ui.Entities;
using Shared.Ui.Entities.RegionTemplate;
using Shared.Ui.Entities.StatCardTemplate;


namespace LMS.Host.Components.Pages;
public partial class Dashboard
{
    private List<StatTemplate> StatCards = new()
    {
        new StatTemplate
        {
            Title = "التسجيلات",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "156438",
            ChipLabel = "84%",
            ChipColor = "#0BFFBA2E",
            TextColor = "#045E22",
            ChipIcon = Icons.Material.Filled.ArrowDropUp

        },
         new StatTemplate
        {
            Title = "عدد الدورات",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "750",
            ChipLabel ="48%",
            ChipColor = "#0BFFBA2E",
            TextColor = "#045E22",
            ChipIcon = Icons.Material.Filled.ArrowDropUp

        },
          new StatTemplate
        {
            Title = "المُعلمون",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "57846",
            ChipLabel = "10%",
            ChipColor = "#FF0B0B2E",
            TextColor = "#5E0404",
            ChipIcon = Icons.Material.Filled.ArrowDropDown
        },  
        new StatTemplate
        {
            Title = "الطلاب",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "951748",
            ChipLabel ="14%",
            ChipColor = "#0BFFBA2E",
            TextColor = "#045E22",
            ChipIcon = Icons.Material.Filled.ArrowDropUp

        }, 
        new StatTemplate
        {
            Title = "الطلاب",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "951748",
            ChipLabel ="14%",
            ChipColor = "#0BFFBA2E",
            TextColor = "#045E22",
            ChipIcon = Icons.Material.Filled.ArrowDropUp

        }, 
        new StatTemplate
        {
            Title = "المستخدمون",
            ImageUrl = "assets/images/statcardsvg.svg",
            Value = "1054487",
            ChipLabel = "7%",
            ChipColor = "#FF0B0B2E",
            TextColor = "#5E0404",
            ChipIcon = Icons.Material.Filled.ArrowDropDown
        }

    };
    private List<RegionIncident> incidents;

    protected override void OnInitialized()
    {
        incidents = GenerateSampleData();
    }

    private List<RegionIncident> GenerateSampleData()
    {
        var regions = new[]
        {
            "الرياض", "مكة", "الشرقية", "عسير", "جازان",
            "نجران", "الباحة", "القسيم", "المدينة", "حائل",
            "الجوف", "تبوك", "الحدود الشمالية"
        };

        var rnd = new Random();
        var data = new List<RegionIncident>();

        for (int year = 2019; year <= 2025; year++)
        {
            foreach (var region in regions)
            {
                data.Add(new RegionIncident
                {
                    Region = region,
                    Year = year,
                    Severity = rnd.Next(1, 100)
                });
            }
        }

        return data;
    }
    private List<(string Label, int Value)> WeekDays = new()
    {
        ("الجمعة", 1400),
        ("الخميس", 7448),
        ("الأربعاء", 7894),
        ("الثلاثاء", 8975),
        ("الإثنين", 9042),
        ("الأحد", 4000),
        ("السبت", 1642),

    };
    private List<Table> tables = new()
{
    new Table { Name = "إدخال الميزانية", Date = "2025-07-01", Category = "المالية", Type = "تحليل", Tick = true, Disc = "تمت الموافقة" },
    new Table { Name = "مقابلة موظف", Date = "2025-07-02", Category = "الموارد البشرية", Type = "توظيف", Tick = false, Disc = "قيد الانتظار" },
    new Table { Name = "تحديث النظام", Date = "2025-07-03", Category = "تقنية المعلومات", Type = "صيانة", Tick = true, Disc = "تم النشر" },
    new Table { Name = "تقرير شهري", Date = "2025-07-04", Category = "المالية", Type = "تقارير", Tick = true, Disc = "تم الإنجاز" },
    new Table { Name = "دورة تدريبية", Date = "2025-07-05", Category = "الموارد البشرية", Type = "تدريب", Tick = false, Disc = "مجدولة" },
    new Table { Name = "مراجعة أمان", Date = "2025-07-06", Category = "تقنية المعلومات", Type = "أمان", Tick = true, Disc = "تم التنفيذ" },
    new Table { Name = "مشتريات جديدة", Date = "2025-07-07", Category = "المشتريات", Type = "طلب شراء", Tick = false, Disc = "بانتظار الموافقة" },
    new Table { Name = "تقرير الأداء", Date = "2025-07-08", Category = "الموارد البشرية", Type = "تقييم", Tick = true, Disc = "منتهي" },
    new Table { Name = "اختبار النظام", Date = "2025-07-09", Category = "تقنية المعلومات", Type = "اختبار", Tick = true, Disc = "نجح" },
    new Table { Name = "طلب إذن", Date = "2025-07-10", Category = "الموارد البشرية", Type = "إجازة", Tick = false, Disc = "قيد المراجعة" },
    new Table { Name = "فاتورة مورد", Date = "2025-07-11", Category = "المشتريات", Type = "دفع", Tick = true, Disc = "تم الدفع" },
    new Table { Name = "حملة تسويق", Date = "2025-07-12", Category = "التسويق", Type = "إعلان", Tick = true, Disc = "نشطة" },
    new Table { Name = "تقرير الفواتير", Date = "2025-07-13", Category = "المالية", Type = "محاسبة", Tick = true, Disc = "جاهز للطباعة" },
    new Table { Name = "ترقية خادم", Date = "2025-07-14", Category = "تقنية المعلومات", Type = "ترقية", Tick = true, Disc = "مكتمل" },
    new Table { Name = "اجتماع الفريق", Date = "2025-07-15", Category = "الإدارة", Type = "اجتماع", Tick = false, Disc = "قيد الجدولة" },
    new Table { Name = "تقييم المورد", Date = "2025-07-16", Category = "المشتريات", Type = "مراجعة", Tick = true, Disc = "تم بنجاح" }
};


    private string GetChipBackgroundColor(string type)
    {
        return type switch
        {
            "Type1" => "#FF0B0B2E", // red
            "Type2" => "#0B89FF2E", // blue
            "Type3" => "#0BFFBA2E", // green/cyan
            _ => "#E0E0E0"           // default gray
        };
    }

    private string GetChipTextColor(string type)
    {
        return type switch
        {
            "Type1" => "#5E0404",   // solid red text
            "Type2" => "#192A56",   // solid blue text
            "Type3" => "#045E22",   // solid green text
            _ => "#000000"          // black fallback
        };
    }

    private HashSet<Table> selectedTables = new();
    private string _clickedName = "None";

    private void HandleClick(TableRowClickEventArgs<Table> args)
    {
        _clickedName = args.Item.Name;
    }

}
  
