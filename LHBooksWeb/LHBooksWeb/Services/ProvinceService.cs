using Newtonsoft.Json;
using static LHBooksWeb.Models.AddressModel;

namespace LHBooksWeb.Services
{

    public interface IProvinceService
    {
        List<Province> GetProvinces();
    }
    public class ProvinceService : IProvinceService
    {
        private readonly List<Province> _provinces;

        public ProvinceService(IWebHostEnvironment env)
        {
            var filePath = Path.Combine(env.ContentRootPath, "Data", "provinces.json");

            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    _provinces = JsonConvert.DeserializeObject<List<Province>>(json) ?? new List<Province>();
                }
                catch (Exception ex)
                {
                    _provinces = new List<Province>();
                    Console.WriteLine("Lỗi đọc file JSON: " + ex.Message);
                }
            }
            else
            {
                _provinces = new List<Province>();
            }
        }

        public List<Province> GetProvinces() => _provinces;
    }
}
