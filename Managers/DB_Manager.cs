using CarLeasingViewer.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace CarLeasingViewer
{
    public class DB_Manager
    {
        static LogSet m_loger = LogManager.GetDefaultLogSet<DB_Manager>();

        readonly string m_connectionString;

        private bool? m_Available;
        const string invoice = "Invoice ";

        public bool Available
        {
            get
            {
                if (m_Available == null)
                    TryConnect();

                return (bool)m_Available;
            }
        }

        public static DB_Manager Default { get { return new DB_Manager(Properties.Settings.Default.ConnectionString); } }

        public DB_Manager(string connectionString)
        {
            m_connectionString = connectionString;

            TryConnect();
        }

        void TryConnect()
        {
            try
            {
                using (var connection = new SqlConnection(m_connectionString))
                    connection.Open();

                m_Available = true;
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при проверке подключения к БД", ex);
                m_Available = false;
            }
        }

        #region queries

        

        /// <summary>
        /// Получение списка доступных методов
        /// </summary>
        /// <param name="invoice">Флаг поиска в архиве</param>
        /// <returns>Возвращает список месяцев</returns>
        public IEnumerable<Month> GetAvailableMonthes(SearchSettings settings = null, int year = 0)
        {
            List<Month> monthes = new List<Month>();
            if (settings == null)
                settings = App.SearchSettings;

            var sql = string.Empty; 

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetAvailableMonthesQuery(oldSettings, year));
                sb.Append(")").Append("\r\n UNION \r\n").Append("("); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetAvailableMonthesQuery(curentSettings, year));
                sb.Append(")");
                sb.Replace("ORDER BY Year", string.Empty);
                sb.Append("ORDER BY Year");

                sql = sb.ToString();
            }
            else
                sql = GetAvailableMonthesQuery(settings, year);

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            year = (int)reader["Year"];

                            if (year > 2016)
                                monthes.Add(new Month(year, (Monthes)(int)reader["Month"]));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе доступных месяцев из БД", ex,
                    new LogParameter("Запрос", sql));
            }

            return monthes;
        }

        [Obsolete("Перешёл на 'начальный-конечный' месяц")]
        public MonthBusiness GetBusinessByMonth(Month month, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            var carBusinesses = new List<CarBusiness>();

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetBusinessByMonthQuery(month, oldSettings, region));
                sb.Append(")\r\n UNION \r\n("); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetBusinessByMonthQuery(month, curentSettings, region));
                sb.Append(")");

                sql = sb.ToString();
            }
            else
                sql = GetBusinessByMonthQuery(month, settings, region);

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var curentCar = string.Empty;
                        var previosCar = string.Empty;
                        var buyer = string.Empty;
                        CarBusiness cb = null;
                        while (reader.Read())
                        {
                            curentCar = (string)reader["CarName"];

                            var no_ = (string)reader["No_"];

                            cb = carBusinesses.FirstOrDefault(_cb => _cb.ItemNo.Equals(no_));
                            if (cb == null)
                            {
                                previosCar = curentCar;

                                cb = new CarBusiness();
                                cb.Month = month;
                                cb.ItemNo = (string)reader["No_"];
                                carBusinesses.Add(cb);

                                var number = ((string)reader["CarNumber"]).Trim();
                                var lastCharIndex = number.Last(c => char.IsLetter(c)) + 1;

                                if ((lastCharIndex) < number.Length)
                                    number = number.Substring(lastCharIndex, number.Length - lastCharIndex);

                                cb.Name = $"{curentCar} ({number})";
                            }

                            buyer = (string)reader["Buyer"];

                            if (cb.Count > 0 && cb.Business.Last().Title.Equals(buyer))
                                cb.Business.Last().DateEnd = ((DateTime)reader["DateEnd"]);
                            else
                            {
                                var b = new Leasing();
                                b.DateStart = ((DateTime)reader["DateStart"]);
                                b.DateEnd = ((DateTime)reader["DateEnd"]);
                                b.Title = buyer;
                                b.Type = BusinessType.Leasing;
                                b.Comment = (string)reader["Comment"];
                                b.CurrentMonth = month;

                                cb.Add(b);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", ex);
            }

            var monthBusiness = new MonthBusiness(carBusinesses);
            monthBusiness.Month = month;

            return monthBusiness;
        }

        public MonthBusiness GetBusinessByMonthes(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            var carBusinesses = new List<CarBusiness>();

            var sql = string.Empty;

            if (settings.SelectedDBSearchType == DBSearchType.All)
            {
                var sb = new StringBuilder(1000);
                sb.Append("(");
                //настройки с поиском старых заказов
                var oldSettings = new SearchSettings(settings);
                oldSettings.SelectedDBSearchType = DBSearchType.Old;

                sb.Append(GetBusinessByMonthesQuery(start, end, oldSettings, region));
                sb.Append(")");
                sb.Append("\r\n UNION \r\n").Append("("); //объединяем два запроса

                //настройки с поиском актуальных заказов
                var curentSettings = new SearchSettings(settings);
                curentSettings.SelectedDBSearchType = DBSearchType.Curent;
                sb.Append(GetBusinessByMonthesQuery(start, end, curentSettings, region));
                sb.Append(")");//.Append(" ORDER BY [Document No_]");

                sql = sb.ToString();
            }
            else
                sql = GetBusinessByMonthesQuery(start, end, settings, region);

            try
            {
                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var curentCar = string.Empty;
                        var previosCar = string.Empty;
                        var buyer = string.Empty;
                        CarBusiness cb = null;
                        while (reader.Read())
                        {
                            curentCar = (string)reader["CarName"];

                            var no_ = (string)reader["No_"];

                            cb = carBusinesses.FirstOrDefault(_cb => _cb.ItemNo.Equals(no_));
                            if (cb == null)
                            {
                                previosCar = curentCar;

                                cb = new CarBusiness();
                                cb.Monthes = Month.GetMonthes(start, end);
                                cb.ItemNo = (string)reader["No_"];
                                carBusinesses.Add(cb);

                                var number = ((string)reader["CarNumber"]).Trim();
                                var lastCharIndex = number.Last(c => char.IsLetter(c)) + 1;

                                if ((lastCharIndex) < number.Length)
                                    number = number.Substring(lastCharIndex, number.Length - lastCharIndex);

                                cb.Name = $"{curentCar} ({number})";
                            }

                            buyer = (string)reader["Buyer"];

                            if (cb.Count > 0 && cb.Business.Last().Title.Equals(buyer))
                                cb.Business.Last().DateEnd = ((DateTime)reader["DateEnd"]).Add(((DateTime)reader["TimeEnd"]).TimeOfDay);
                            else
                            {
                                var b = new Leasing();
                                b.DateStart = ((DateTime)reader["DateStart"]).Add(((DateTime)reader["TimeStart"]).TimeOfDay);
                                b.DateEnd = ((DateTime)reader["DateEnd"]).Add(((DateTime)reader["TimeEnd"]).TimeOfDay);
                                b.Title = buyer;
                                b.Type = BusinessType.Leasing;
                                b.Comment = (string)reader["Comment"];
                                b.Monthes = Month.GetMonthes(b.DateStart, b.DateEnd);
                                b.Saler = (string)reader["Saler"];
                                b.Blocked = ((byte)reader["Blocked"]) > 0;

                                cb.Add(b);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", sqlEx,
                    new LogParameter("Запрос", sql));
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе выборки из БД", ex);
            }

            //добавляем не занятые авто
            AddFreeCars(carBusinesses);

            var monthBusiness = new MonthBusiness(carBusinesses); //.OrderBy(cb => cb.Name)); сортировка теперь при выборке
            monthBusiness.Monthes = Month.GetMonthes(new DateTime(start.Year, start.Index, 1), new DateTime(end.Year, end.Index, 1));

            return monthBusiness;
        }

        /// <summary>
        /// Получение списка всех авто
        /// </summary>
        /// <param name="settings">Текущие настройки</param>
        /// <param name="region">Регион</param>
        /// <returns>Возвращает выбранный набор машин или пустой список</returns>
        public IEnumerable<Car> GetAllCars(SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            if (region == null)
                region = settings.SelectedRegion;

            List<Car> cars = new List<Car>();
            try
            {
                var sql = $@"DECLARE @defultDate AS Datetime = '1753-01-01 00:00:00'
                        SELECT DISTINCT
                          i.[No_] as ID
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , p.[Unit Price] as Price

                         FROM Carlson$Item i
                            LEFT JOIN [Carlson$Sales Price] p ON p.[Item No_] = i.No_
                    
                         WHERE 1 = 1
                           {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            {((region == null || region.IsTotal) ? string.Empty : ("AND i.[Responsibility Center] = " + region.DBKey))}
                            AND p.[Minimum Quantity] = 1.0
							AND p.[Ending Date] = @defultDate
                        ORDER BY Price, ID";

                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        var carName = string.Empty;
                        var carNumber = string.Empty;
                        var id = string.Empty;
                        Car car = null;
                        var includeBlocked = settings.IncludeBlocked;
                        while (reader.Read())
                        {

                            carName = (string)reader["CarName"];
                            carNumber = (string)reader["CarNumber"];
                            id = (string)reader["ID"];

                            car = new Car(carName, carNumber);
                            car.No = id;

                            if (includeBlocked)
                                car.Blocked = ((byte)reader["Blocked"]) > 0;

                            cars.Add(car);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе полного набора машин из БД", ex);
            }

            return cars;
        }

        public IEnumerable<Region> GetRegions()
        {
            List<Region> regions = new List<Region>();
            

            if (App.SearchSettings.TestData)
            {
                regions = new List<Region>()
                {
                    new Region(){ Address = "ул.Краснококшайская, д.72", DBKey = "KZ", DisplayName = "Казань", PostCode = "420032" }
                    , new Region(){ Address = "ул.Ахметшина,автостоянка \"Вектор-Ч\"", DBKey = "NCH", DisplayName = "Набережные Челны", PostCode = "423814" }
                    , new Region(){ Address = "ул. М.Ямская, д.78А", DBKey = "NN", DisplayName = "Нижний Новгород", PostCode = "603022" }
                };
            }
            else
            {
                try
                {
                    var sql = $@"SELECT [Code]
                                	    , [Name]
                                	    , [Address]
                                	    , [Post Code]
                                     FROM [Carlson$Responsibility Center]";

                    using (var con = new SqlConnection(m_connectionString))
                    {
                        var com = new SqlCommand(sql);
                        com.Connection = con;

                        con.Open();

                        using (var reader = com.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var r = new Region();
                                r.DBKey = (string)reader["Code"];
                                r.DisplayName = (string)reader["Name"];
                                r.Address = (string)reader["Address"];
                                r.PostCode = (string)reader["Post code"];
                                regions.Add(r);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    m_loger.Log("Возникло исключение при получении списка регионов из БД", ex);
                }
            }
            regions.Insert(0, Region.Total);

            return regions;
        }

        /// <summary>
        /// Получение порядка цен на авто
        /// </summary>
        /// <returns>Возвращает набор [ID, Price]</returns>
        public IEnumerable<Tuple<string, decimal>> GetDayPriceOrder()
        {
            List<Tuple<string, decimal>> prices = new List<Tuple<string, decimal>>();

            try
            {
                var sql = @"DECLARE @defaultDate AS Datetime = '1753-01-01 00:00:00'

                            SELECT
                                  [Item No_] AS ID
                                  ,[Unit Price] AS Price
                              FROM [CARLSON_Test_10052018].[dbo].[Carlson$Sales Price] AS p
                            	WHERE 1 = 1
                            		AND [Minimum Quantity] = 1.0
                            		AND p.[Ending Date] = @defaultDate
                            		AND p.[Unit of Measure Code] = 'ДЕНЬ'
                            		ORDER BY p.[Item No_]";

                using (var con = new SqlConnection(m_connectionString))
                {
                    var com = new SqlCommand(sql);
                    com.Connection = con;

                    con.Open();

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prices.Add(new Tuple<string, decimal>((string)reader["ID"], (decimal)reader["Price"]));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                m_loger.Log("Возникло исключение при запросе порядка цен на авто из БД", ex);
            }

            return prices;
        }

        #endregion

        #region private        

        string GetAvailableMonthesQuery(SearchSettings settings, int year = 0)
        {
            if (settings == null)
                settings = App.SearchSettings;

            return $@"SELECT DISTINCT 
                            MONTH(h.[Date Begin]) as Month
                            , YEAR(h.[Date Begin]) as Year
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {(year > 0 ? ("AND YEAR(h.[Date Begin]) = " + year.ToString()) : "")}
                        
                        ORDER BY Year";
        }

        String GetBusinessByMonthesQuery(Month start, Month end, SearchSettings settings = null, Region region = null)
        {
            if (settings == null)
                settings = App.SearchSettings;

            if (region == null)
                region = settings.SelectedRegion;

            return $@"SELECT 
                         i.[No_]
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , l.[Document No_]
	                    , h.[Salesperson Code] as Saler
	                    , h.[Bal_ Account No_]
	                    , h.[Sell-to Customer Name] as Buyer
	                    , h.[Bill-to Name]
	                    , h.[Ship-to Name]
	                    , h.[Venicle Operation Area]
	                    , h.[Date Begin] as DateStart
	                    , h.[Time Begin] as TimeStart
	                    , h.[Date End] as DateEnd
	                    , h.[Time End] as TimeEnd
	                    , h.[Comment Text] as Comment
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {((region == null || region.IsTotal) ? string.Empty : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
                            AND ((h.[Date Begin] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{start.GetSqlDate(1)}' AND '{end.Next().GetSqlDate(1)}'))";
        }

        String GetBusinessByMonthQuery(Month month, SearchSettings settings = null, Region region = null)
        {
            return $@"SELECT 
                         i.[No_]
                        , i.[Description] as CarName
                        , i.[Vehicle Reg_ No_] as CarNumber
                        , i.[Blocked] as Blocked
                        , l.[Document No_]
	                    ,h.[Salesperson Code] as Saler
	                    ,h.[Bal_ Account No_]
	                    ,h.[Sell-to Customer Name] as Buyer
	                    ,h.[Bill-to Name]
	                    ,h.[Ship-to Name]
	                    ,h.[Venicle Operation Area]
	                    ,h.[Date Begin] as DateStart
	                    ,h.[Time Begin]
	                    ,h.[Date End] as DateEnd
	                    ,h.[Time End]
	                    ,h.[Comment Text] as Comment
                         FROM Carlson$Item i
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Line] l ON l.No_ = i.No_
                        	LEFT JOIN [Carlson$Sales {(settings.SelectedDBSearchType == DBSearchType.Curent ? string.Empty : invoice)}Header] h ON h.No_ = l.[Document No_]
                        
                        WHERE 1 = 1
                            {(settings.IncludeBlocked ? string.Empty : "AND i.Blocked = 0")}
                        	AND i.IsService = 0
                        	AND i.IsFranchise = 0
                            AND h.[Date Begin] IS NOT NULL
                            {((region == null || region.IsTotal) ? string.Empty : "AND i.[Responsibility Center] = '" + region.DBKey + "'")}
                            AND ((h.[Date Begin] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}') OR (h.[Date End] BETWEEN '{month.GetSqlDate(1)}' AND '{month.Next().GetSqlDate(1)}'))
                        
                        ORDER BY CarName";
        }

        void AddFreeCars(List<CarBusiness> carBusinesses)
        {
            foreach (var car in App.Cars)
            {
                if (carBusinesses.Any(b => b.ItemNo.Equals(car.No)))
                    continue;
                else
                {
                    var cb = new CarBusiness();
                    cb.ItemNo = car.No;
                    cb.Name = car.FullName;
                    cb.Monthes = new Month[0];
                    //cb.Add(new Leasing() { }); //пустой экземпляр - костыль для текущей логики
                    carBusinesses.Add(cb);
                }
            }
        }

        #endregion
    }
}
